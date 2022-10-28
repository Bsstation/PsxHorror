using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.UI;

public enum InputMode { keyboard, joystick }

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public InputMode mode;

    [System.Serializable]
    public class Keys
    {
        //movement
        public KeyCode forward { get; set; }
        public KeyCode backward { get; set; }
        public KeyCode left { get; set; }
        public KeyCode right { get; set; }
        //actions
        public KeyCode action { get; set; }
        public KeyCode sprint { get; set; }
        public KeyCode aim { get; set; }
        public KeyCode focus { get; set; }
        public KeyCode reload { get; set; }
        public KeyCode flashlight { get; set; }

        //inventory
        public KeyCode inventory { get; set; }
        public KeyCode map { get; set; }
        public KeyCode pause { get; set; }
    }

    public Keys kKeys;

    public bool benchmarkMode;

    [Header("Xinput")]
    public GamePadState state;
    public GamePadState prevState;
    bool playerIndexSet = false;
    PlayerIndex playerIndex;

    [Header("UI")]
    public GameObject joystickAds;
    public GameObject keyboardAds;
    public Text fpsCounter;
    public float timeToShowAds;
    float currentTimeAds;

    void Awake()
    {
        //Singleton pattern
        transform.parent = null;

        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60;

        Init();

        UpdateAll();
    }

    #region KeyboardMap

    public void Init()
    {
        //keyboard
        kKeys.forward = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("forwardKey", "W"));
        kKeys.backward = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("backwardKey", "S"));
        kKeys.left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("leftKey", "A"));
        kKeys.right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("rightKey", "D"));

        kKeys.sprint = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("sprintKey", "LeftShift"));
        kKeys.aim = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("aimKey", "RightShift"));
        kKeys.focus = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("focusKey", "Q"));
        kKeys.action = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("actionKey", "F"));
        kKeys.reload = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("reloadKey", "R"));
        kKeys.flashlight = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("flashlightKey", "L"));

        kKeys.inventory = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("inventoryKey", "E"));
        kKeys.map = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("mapKey", "M"));
        kKeys.pause = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("pauseKey", "P"));
    }

    public void Default()
    {
        PlayerPrefs.DeleteKey("forwardKey");
        PlayerPrefs.DeleteKey("backwardKey");
        PlayerPrefs.DeleteKey("leftKey");
        PlayerPrefs.DeleteKey("rightKey");

        PlayerPrefs.DeleteKey("sprintKey");
        PlayerPrefs.DeleteKey("aimKey");
        PlayerPrefs.DeleteKey("focusKey");
        PlayerPrefs.DeleteKey("actionKey");
        PlayerPrefs.DeleteKey("reloadKey");
        PlayerPrefs.DeleteKey("flashlightKey");

        PlayerPrefs.DeleteKey("inventoryKey");
        PlayerPrefs.DeleteKey("mapKey");
        PlayerPrefs.DeleteKey("pauseKey");

        Init();
    }

    #endregion

    private void OnLevelWasLoaded(int level)
    {
        CheckInpuManagerGroup group = new GameObject().AddComponent<CheckInpuManagerGroup>();
        group.SetupAll();
    }

    float counter = 1;

    void Update()
    {
        JoystickUpdate();

        CheckInputMode();

        currentTimeAds = (currentTimeAds > 0) ?  currentTimeAds - Time.deltaTime : 0;

        joystickAds.SetActive(false);
        keyboardAds.SetActive(false);
        if (currentTimeAds > 0)
        {
            if(mode == InputMode.joystick)
                joystickAds.SetActive(true);
            else
                keyboardAds.SetActive(true);
        }

        fpsCounter.enabled = benchmarkMode;

        if (benchmarkMode) {
            float deltatime = 0;
            deltatime += (Time.unscaledDeltaTime - deltatime);
            float fps = 1.0f / deltatime;
            counter -= Time.unscaledDeltaTime;

            if (counter <= 0)
                UpdateCounter(Mathf.Ceil(fps).ToString() + " fps");
        }
    }

    void UpdateCounter(string text)
    {
        fpsCounter.text = text;
        counter = 1;
    }

    #region Keyboard Axis

    public Vector2 Movement()
    {
        Vector2 dpad = Vector2.zero;

        if ((Input.GetKey(kKeys.forward)) &&
            !Input.GetKey(kKeys.backward)) dpad.y = 1;

        else if ((Input.GetKey(kKeys.backward)) &&
            !Input.GetKey(kKeys.forward)) dpad.y = -1;

        if ((Input.GetKey(kKeys.right)) &&
            !Input.GetKey(kKeys.left)) dpad.x = 1;

        else if ((Input.GetKey(kKeys.left)) &&
            !Input.GetKey(kKeys.right)) dpad.x = -1;

        return dpad;
    }

    public Vector2 UiMovementWithMouse()
    {
        Vector2 uipad = Vector2.zero;

        if ((Input.GetKey(KeyCode.UpArrow)) &&
            !Input.GetKey(kKeys.backward)) uipad.y = 1;

        else if ((Input.GetKey(KeyCode.DownArrow)) &&
            !Input.GetKey(kKeys.forward)) uipad.y = -1;

        if ((Input.GetKey(KeyCode.RightArrow)) &&
            !Input.GetKey(kKeys.left)) uipad.x = 1;

        else if ((Input.GetKey(KeyCode.LeftArrow)) &&
            !Input.GetKey(kKeys.right)) uipad.x = -1;

        if (Settings.instance && !Settings.instance.cursorOn)
        {
            float x = Mathf.Clamp(uipad.x + Movement().x + Input.GetAxisRaw("Mouse X"), -1, 1);
            float y = Mathf.Clamp(uipad.y + Movement().y + Input.GetAxisRaw("Mouse Y"), -1, 1);

            return new Vector2(x, y);
        }

        return UiMovementWithoutMouse();
    }

    public Vector2 UiMovementWithoutMouse()
    {
        Vector2 uipad = Vector2.zero;

        if ((Input.GetKey(KeyCode.UpArrow)) &&
            !Input.GetKey(kKeys.backward)) uipad.y = 1;

        else if ((Input.GetKey(KeyCode.DownArrow)) &&
            !Input.GetKey(kKeys.forward)) uipad.y = -1;

        if ((Input.GetKey(KeyCode.RightArrow)) &&
            !Input.GetKey(kKeys.left)) uipad.x = 1;

        else if ((Input.GetKey(KeyCode.LeftArrow)) &&
            !Input.GetKey(kKeys.right)) uipad.x = -1;

        float x = Mathf.Clamp(uipad.x + Movement().x, -1, 1);
        float y = Mathf.Clamp(uipad.y + Movement().y, -1, 1);

        return new Vector2(x, y);
    }

    #endregion

    #region JoyInputs

    void JoystickUpdate()
    {
        // Find a PlayerIndex, for a single player game
        // Will find the first controller that is connected ans use it
        if (!playerIndexSet)
        {
            PlayerIndex testPlayerIndex = (PlayerIndex)1;
            GamePadState testState = GamePad.GetState(testPlayerIndex);
            if (testState.IsConnected)
            {
                Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                playerIndex = testPlayerIndex;
                playerIndexSet = true;
            }
        }

        prevState = state;
        state = GamePad.GetState(playerIndex);
    }

    #region Joy Axis

    public Vector2 JoystickMove()
    {
        return (JoystickLAxis() + JoystickDpadAxis()).normalized;
    }

    public Vector2 JoystickDpadAxis()
    {
        Vector2 dpad = Vector2.zero;

        if (state.DPad.Up == ButtonState.Pressed  &&  state.DPad.Down != ButtonState.Pressed) dpad.y = 1;

        else if (state.DPad.Down == ButtonState.Pressed  &&  state.DPad.Up != ButtonState.Pressed) dpad.y = -1;

        else if (state.DPad.Right == ButtonState.Pressed  &&  state.DPad.Left != ButtonState.Pressed) dpad.x = 1;

        else if (state.DPad.Left == ButtonState.Pressed  &&  state.DPad.Right != ButtonState.Pressed) dpad.x = -1;

        else dpad = Vector2.zero;

        return dpad.normalized;
    }

    public Vector2 JoystickLAxis()
    {
        return new Vector2(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y).normalized;
    }

    public Vector2 JoystickRAxis()
    {
        return new Vector2(state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y).normalized;
    }

    #endregion

    #region Triggers

    public bool GetJoyTrigger(string trigger)
    {
        switch (trigger)
        {
            case "left":
                return state.Triggers.Left == 1;
            case "right":
                return state.Triggers.Right == 1;
        }
        return false;
    }

    public bool GetJoyTriggerDown(string trigger)
    {
        switch (trigger)
        {
            case "left":
                return state.Triggers.Left == 1 && prevState.Triggers.Left == 0;
            case "right":
                return state.Triggers.Right == 1 && prevState.Triggers.Right == 0;
        }
        return false;
    }

    public bool GetJoyTriggerUp(string trigger)
    {
        switch (trigger)
        {
            case "left":
                return state.Triggers.Left == 0 && prevState.Triggers.Left == 1;
            case "right":
                return state.Triggers.Right == 0 && prevState.Triggers.Right == 1;
        }
        return false;
    }

    #endregion

    #region Inputs

    public bool GetJoyButton(string input)
    {
        switch (input)
        {
            //Normal Buttons
            case "A":
                return state.Buttons.A == ButtonState.Pressed;
            case "B":
                return state.Buttons.B == ButtonState.Pressed;
            case "Y":
                return state.Buttons.Y == ButtonState.Pressed;
            case "X":
                return state.Buttons.X == ButtonState.Pressed;
            
            //Dpad Buttons
            case "left":
                return state.DPad.Left == ButtonState.Pressed;
            case "right":
                return state.DPad.Right == ButtonState.Pressed;
            case "up":
                return state.DPad.Up == ButtonState.Pressed;
            case "down":
                return state.DPad.Down == ButtonState.Pressed;

            //Middle buttons
            case "start":
                return state.Buttons.Start == ButtonState.Pressed;
            case "back":
                return state.Buttons.Back == ButtonState.Pressed;

            //Upper & Stick Buttons
            case "lshoulder":
                return state.Buttons.LeftShoulder == ButtonState.Pressed;
            case "rshoulder":
                return state.Buttons.RightShoulder == ButtonState.Pressed;
            case "lstick":
                return state.Buttons.LeftStick == ButtonState.Pressed;
            case "rstick":
                return state.Buttons.RightStick == ButtonState.Pressed;
        }
        return false;
    }

    public bool GetJoyButtonDown(string input)
    {
        switch (input)
        {
            //Normal Buttons
            case "A":
                return state.Buttons.A == ButtonState.Pressed && prevState.Buttons.A == ButtonState.Released;
            case "B":
                return state.Buttons.B == ButtonState.Pressed && prevState.Buttons.B == ButtonState.Released;
            case "Y":
                return state.Buttons.Y == ButtonState.Pressed && prevState.Buttons.Y == ButtonState.Released;
            case "X":
                return state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released;

            //Dpad Buttons
            case "left":
                return state.DPad.Left == ButtonState.Pressed && prevState.DPad.Left == ButtonState.Released;
            case "right":
                return state.DPad.Right == ButtonState.Pressed && prevState.DPad.Right == ButtonState.Released;
            case "up":
                return state.DPad.Up == ButtonState.Pressed && prevState.DPad.Up == ButtonState.Released;
            case "down":
                return state.DPad.Down == ButtonState.Pressed && prevState.DPad.Down == ButtonState.Released;

            //Middle buttons
            case "start":
                return state.Buttons.Start == ButtonState.Pressed && prevState.Buttons.Start == ButtonState.Released;
            case "back":
                return state.Buttons.Back == ButtonState.Pressed && prevState.Buttons.Back == ButtonState.Released;

            //Upper & Stick Buttons
            case "lshoulder":
                return state.Buttons.LeftShoulder == ButtonState.Pressed && prevState.Buttons.LeftShoulder == ButtonState.Released;
            case "rshoulder":
                return state.Buttons.RightShoulder == ButtonState.Pressed && prevState.Buttons.RightShoulder == ButtonState.Released;
            case "lstick":
                return state.Buttons.LeftStick == ButtonState.Pressed && prevState.Buttons.LeftStick == ButtonState.Released;
            case "rstick":
                return state.Buttons.RightStick == ButtonState.Pressed && prevState.Buttons.RightStick == ButtonState.Released;
        }
        return false;
    }

    public bool GetJoyButtonUp(string input)
    {
        switch (input)
        {
            //Normal Buttons
            case "A":
                return state.Buttons.A == ButtonState.Released && prevState.Buttons.A == ButtonState.Pressed;
            case "B":
                return state.Buttons.B == ButtonState.Released && prevState.Buttons.B == ButtonState.Pressed;
            case "Y":
                return state.Buttons.Y == ButtonState.Released && prevState.Buttons.Y == ButtonState.Pressed;
            case "X":
                return state.Buttons.X == ButtonState.Released && prevState.Buttons.X == ButtonState.Pressed;

            //Dpad Buttons
            case "left":
                return state.DPad.Left == ButtonState.Released && prevState.DPad.Left == ButtonState.Pressed;
            case "right":
                return state.DPad.Right == ButtonState.Released && prevState.DPad.Right == ButtonState.Pressed;
            case "up":
                return state.DPad.Up == ButtonState.Released && prevState.DPad.Up == ButtonState.Pressed;
            case "down":
                return state.DPad.Down == ButtonState.Released && prevState.DPad.Down == ButtonState.Pressed;

            //Middle buttons
            case "start":
                return state.Buttons.Start == ButtonState.Released && prevState.Buttons.Start == ButtonState.Pressed;
            case "back":
                return state.Buttons.Back == ButtonState.Released && prevState.Buttons.Back == ButtonState.Pressed;

            //Upper & Stick Buttons
            case "lshoulder":
                return state.Buttons.LeftShoulder == ButtonState.Pressed && prevState.Buttons.LeftShoulder == ButtonState.Pressed;
            case "rshoulder":
                return state.Buttons.RightShoulder == ButtonState.Pressed && prevState.Buttons.RightShoulder == ButtonState.Pressed;
            case "lstick":
                return state.Buttons.LeftStick == ButtonState.Pressed && prevState.Buttons.LeftStick == ButtonState.Pressed;
            case "rstick":
                return state.Buttons.RightStick == ButtonState.Pressed && prevState.Buttons.RightStick == ButtonState.Pressed;
        }
        return false;
    }

    #endregion

    #endregion

    void CheckInputMode()
    {
        if (JoystickLAxis() != Vector2.zero ||
            JoystickDpadAxis() != Vector2.zero)
        {
            ChangeToJoystick();
        }

        if(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) != Vector2.zero)
        {
            ChangeToKeyboard();
        }

        for (int i = 0; i < 20; i++)
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown("joystick 1 button " + i))
                {
                    ChangeToJoystick();
                    break;
                }
                
                if(i == 19)
                {
                    ChangeToKeyboard();
                }
            }
        }
    }

    void ChangeToJoystick()
    {
        if (mode == InputMode.keyboard)
        {
            mode = InputMode.joystick;
            currentTimeAds = timeToShowAds;

            UpdateAll();
        }
    }

    void ChangeToKeyboard()
    {
        if (mode == InputMode.joystick)
        {
            mode = InputMode.keyboard;
            currentTimeAds = timeToShowAds;

            UpdateAll();
        }
    }

    void UpdateAll()
    {
        CheckInpuManagerGroup group = new GameObject().AddComponent<CheckInpuManagerGroup>();
        group.SetupAll();
    }
}