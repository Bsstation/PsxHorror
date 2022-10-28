using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputMenu : MonoBehaviour
{
    Event keyEvent;

    Text buttonText;

    KeyCode newKey;

    public Text[] movementTexts = new Text[4];
    public Text[] actionTexts = new Text[6];
    public Text[] interfaceTexts = new Text[3];

    bool waitingForKey;

    public GameObject waitingPanel;
    public GameObject guide;
    public EscToBack esc;

    bool cancel;

    // Start is called before the first frame update
    public void Start()
    {
        waitingForKey = false;

        //init movement texts
        movementTexts[0].text = InputManager.instance.kKeys.forward.ToString();
        movementTexts[1].text = InputManager.instance.kKeys.backward.ToString();
        movementTexts[2].text = InputManager.instance.kKeys.left.ToString();
        movementTexts[3].text = InputManager.instance.kKeys.right.ToString();

        //int action texts
        actionTexts[0].text = InputManager.instance.kKeys.action.ToString();
        actionTexts[1].text = InputManager.instance.kKeys.sprint.ToString();
        actionTexts[2].text = InputManager.instance.kKeys.aim.ToString();
        actionTexts[3].text = InputManager.instance.kKeys.focus.ToString();
        actionTexts[4].text = InputManager.instance.kKeys.reload.ToString();
        actionTexts[5].text = InputManager.instance.kKeys.flashlight.ToString();

        //interface
        interfaceTexts[0].text = InputManager.instance.kKeys.inventory.ToString();
        interfaceTexts[1].text = InputManager.instance.kKeys.map.ToString();
        interfaceTexts[2].text = InputManager.instance.kKeys.pause.ToString();

        keyEvent = Event.current;
    }

    // Update is called once per frame
    void Update()
    {
        waitingPanel.SetActive(waitingForKey);
        esc.enabled = !waitingForKey;
        guide.SetActive(!waitingForKey);

        if (waitingForKey)
        {
            if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(InputManager.instance.kKeys.sprint))
            {
                ClearWaiting();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                newKey = KeyCode.LeftShift;
            }
            else if (Input.GetKeyDown(KeyCode.RightShift))
            {
                newKey = KeyCode.RightShift;
            }
        }
    }

    public void ClearWaiting()
    {
        waitingForKey = false;
    }

    void OnGUI()
    {
        /*keyEvent dictates what key our user presses
         * bt using Event.current to detect the current
         * event
         */

        keyEvent = Event.current;

        //Executes if a button gets pressed and
        //the user presses a key

        if ((keyEvent.isKey || keyEvent.shift) && waitingForKey)
        {
            if (keyEvent.shift)
            {
                print("shift");
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    newKey = KeyCode.LeftShift;
                }
                else if (Input.GetKey(KeyCode.RightShift))
                {
                    newKey = KeyCode.RightShift;
                }
            }
            else
            {
                newKey = keyEvent.keyCode; //Assigns newKey to the key user presses
            }

            waitingForKey = false;
        }
    }

    IEnumerator WaitForKey()
    {
        while (!keyEvent.shift && !keyEvent.isKey && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            yield return null;

        if (keyEvent.isKey)
        {
            if (keyEvent.keyCode == KeyCode.Escape || newKey == KeyCode.Escape)
            {
                cancel = true;
            }

            if (newKey == KeyCode.Return || newKey == KeyCode.Alpha1 || newKey == KeyCode.Alpha2 || newKey == KeyCode.Alpha3 || newKey == KeyCode.Alpha4)
                newKey = KeyCode.None;
        }
        else if(keyEvent.shift){
            print("SHIFT");
        }
    }

    public void SendText(Text text)
    {
        buttonText = text;
    }

    public void StartAssignment(string keyName)
    {
        if (!waitingForKey)
            StartCoroutine(AssignKey(keyName));
    }

    public IEnumerator AssignKey(string keyName)
    {
        waitingForKey = true;

        yield return WaitForKey(); //Executes endlessly until user presses a key

        if (!cancel)
        {
            switch (keyName)
            {
                //movement

                case "forward":
                    InputManager.instance.kKeys.forward = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.forward.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("forwardKey", InputManager.instance.kKeys.forward.ToString()); //save new key to PlayerPrefs
                    break;
                case "back":
                    InputManager.instance.kKeys.backward = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.backward.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("backwardKey", InputManager.instance.kKeys.backward.ToString()); //save new key to PlayerPrefs
                    break;
                case "left":
                    InputManager.instance.kKeys.left = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.left.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("leftKey", InputManager.instance.kKeys.left.ToString()); //save new key to PlayerPrefs
                    break;
                case "right":
                    InputManager.instance.kKeys.right = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.right.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("rightKey", InputManager.instance.kKeys.right.ToString()); //save new key to PlayerPrefs
                    break;

                    //action

                case "action":
                    InputManager.instance.kKeys.action = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.action.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("actionKey", InputManager.instance.kKeys.action.ToString()); //save new key to PlayerPrefs
                    break;
                case "sprint":
                    InputManager.instance.kKeys.sprint = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.sprint.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("sprintKey", InputManager.instance.kKeys.sprint.ToString()); //save new key to PlayerPrefs
                    break;
                case "aim":
                    InputManager.instance.kKeys.aim = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.aim.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("aimKey", InputManager.instance.kKeys.aim.ToString()); //save new key to PlayerPrefs
                    break;
                case "focus":
                    InputManager.instance.kKeys.focus = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.focus.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("focusKey", InputManager.instance.kKeys.focus.ToString()); //save new key to PlayerPrefs
                    break;
                case "reload":
                    InputManager.instance.kKeys.reload = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.reload.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("reloadKey", InputManager.instance.kKeys.reload.ToString()); //save new key to PlayerPrefs
                    break;
                case "flashlight":
                    InputManager.instance.kKeys.flashlight = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.flashlight.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("flashlightKey", InputManager.instance.kKeys.flashlight.ToString()); //save new key to PlayerPrefs
                    break;

                //Ui
                case "inventory":
                    InputManager.instance.kKeys.inventory = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.inventory.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("inventoryKey", InputManager.instance.kKeys.inventory.ToString()); //save new key to PlayerPrefs
                    break;
                case "map":
                    InputManager.instance.kKeys.map = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.map.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("mapKey", InputManager.instance.kKeys.map.ToString()); //save new key to PlayerPrefs
                    break;
                case "pause":
                    InputManager.instance.kKeys.pause = newKey; //Set forward to new keycode
                    buttonText.text = InputManager.instance.kKeys.pause.ToString(); //Set button text to new key
                    PlayerPrefs.SetString("pauseKey", InputManager.instance.kKeys.pause.ToString()); //save new key to PlayerPrefs
                    break;
            }
        }

        cancel = false;

        newKey = KeyCode.None;

        waitingForKey = false;

        yield return null;
    }
}
