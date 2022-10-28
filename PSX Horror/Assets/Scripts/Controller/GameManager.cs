using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum GameStatus
{
    Game, Puzzle, Inventory, Pause, SaveGame, Cutscene, Loading, Fading, Dead, ItemBox
}

public class GameManager : MonoBehaviour
{
    public GameStatus gameStatus;
    public PlayerController player;
    public UIController uicontroller;
    public InventoryUI inventory;
    public GameObject puzzles;
    public GameObject saveScreen;
    public GameObject loadingScreen;

    public Cinemachine.CinemachineVirtualCamera deathCam;

    public bool cursorOn;

    public bool debugAim;

    public static GameManager instance;

    public EventSystem eventSystem;

    public float timeScale;

    [Header("SFX")]
    public AudioClip deathSound;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        player = FindObjectOfType(typeof(PlayerController)) as PlayerController;

        deathCam = player.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        deathCam.gameObject.SetActive(false);

        uicontroller = FindObjectOfType(typeof(UIController)) as UIController;
        inventory = FindObjectOfType(typeof(InventoryUI)) as InventoryUI;
        eventSystem = EventSystem.current;
        puzzles = uicontroller.transform.GetChild(0).transform.Find("Puzzles").gameObject;
        saveScreen = uicontroller.transform.GetChild(0).transform.Find("Save Screen").gameObject;
        loadingScreen = uicontroller.transform.GetChild(0).transform.Find("Loading Screen").gameObject;

        gameStatus = GameStatus.Pause;
        gameStatus = GameStatus.ItemBox;
        gameStatus = GameStatus.Inventory;
        gameStatus = GameStatus.Game;
        Time.timeScale = 1;
    }

    private void Start()
    {
        if (Settings.instance.cursorOn && InputManager.instance.mode == InputMode.keyboard)
            EnableCursor(true);
        else
            EnableCursor(false);
    }

    public void EnableCursor(bool value)
    {
        if (value)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameStatus)
        {
            case GameStatus.Game:
                cursorOn = false;
                timeScale = (MessagesBehaviour.instance.examing) ? 0 : 1;

                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);

                uicontroller.ShowHud();

                if (!player.dead && !player.undamage && !player.uncontrol && !player.reloading && !player.aim && !MessagesBehaviour.instance.examing)
                {
                    if (Input.GetKeyDown(InputManager.instance.kKeys.inventory) ||
                        InputManager.instance.GetJoyButtonDown("start"))
                    {
                        inventory.PlayAcceptAudio();
                        gameStatus = GameStatus.Inventory;
                        inventory.ShowItems();
                        ChangeSelected(uicontroller.buttons.backButton);
                        inventory.ClearItems();
                        inventory.Update();

                        inventory.UpdateSlots();

                        ChangeSelected(inventory.slots[0].gameObject);
                    }
                    else if (Input.GetButtonDown("Cancel") ||
                        InputManager.instance.GetJoyButtonDown("back"))
                    {
                        inventory.PlayCancelAudio();
                        StartCoroutine(ChangeToPause());
                    }
                    else if (Input.GetKeyDown(InputManager.instance.kKeys.map) ||
                        InputManager.instance.GetJoyButtonDown("Y"))
                    {
                        inventory.PlayAcceptAudio();
                        gameStatus = GameStatus.Inventory;
                        inventory.OpenMap();
                        ChangeSelected(uicontroller.buttons.mapButton);
                        inventory.ClearItems();
                        inventory.Update();
                    }
                } 

                break;

            case GameStatus.Puzzle:
                cursorOn = true;
                timeScale = 0;
                saveScreen.SetActive(false);
                puzzles.SetActive(true);
                loadingScreen.SetActive(false);
                uicontroller.HideAll();
                break;

            case GameStatus.Dead:
                cursorOn = true;
                timeScale = 0;

                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                uicontroller.ShowDeathMenu();
                break;

            case GameStatus.Inventory:
                cursorOn = true;
                timeScale = (inventory.itemToAdd || inventory.upgrading) ? 0 : 1;

                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                uicontroller.OpenInventory();

                if (Input.GetKeyDown(InputManager.instance.kKeys.inventory) ||
                    InputManager.instance.GetJoyButtonDown("start"))
                {
                    if (!inventory.upgrading && !inventory.selectedItem && !inventory.itemToAdd &&
                        !inventory.selectedSlot && !inventory.itemToShortcut && !inventory.moving)
                    {
                        inventory.PlayCancelAudio();
                        gameStatus = GameStatus.Game;
                    }
                }

                break;

            case GameStatus.Pause:
                cursorOn = true;
                timeScale = 0;

                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                uicontroller.ShowPause();
                break;

            case GameStatus.SaveGame:
                cursorOn = true;
                timeScale = 0;

                uicontroller.HideAll();

                saveScreen.SetActive(true);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                break;

            case GameStatus.Fading:
                cursorOn = false;
                timeScale = 0;

                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                uicontroller.HideAll();
                break;

            case GameStatus.Loading:
                cursorOn = false;
                timeScale = 1;

                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                uicontroller.HideAll();
                loadingScreen.SetActive(true);
                break;

            case GameStatus.Cutscene:
                cursorOn = false;
                timeScale = 1;

                uicontroller.ShowCutscene();
                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                break;

            case GameStatus.ItemBox:
                cursorOn = true;
                timeScale = 0;

                uicontroller.ShowItemBoxMenu();
                saveScreen.SetActive(false);
                puzzles.SetActive(false);
                loadingScreen.SetActive(false);
                break;
        }
    }

    private void LateUpdate()
    {
        Time.timeScale = timeScale;

        if (InputManager.instance.mode == InputMode.keyboard && Settings.instance.cursorOn && cursorOn)
        {
            if (Cursor.lockState == CursorLockMode.None && Cursor.visible == true)
            {
                if (InputManager.instance.UiMovementWithoutMouse() != Vector2.zero || Input.GetKey(KeyCode.Return))
                {
                    EnableCursor(false);
                }
            }
            else if (Cursor.lockState == CursorLockMode.Locked && Cursor.visible == false)
            {
                if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 || Input.GetKey(KeyCode.Mouse0) ||
                    Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2))
                {
                    EnableCursor(true);
                }
            }
        }
        else
        {
            EnableCursor(false);
        }
    }

    public void ChangeSelected(GameObject selected)
    {
        StartCoroutine(SelectContinueButtonLater(selected));
    }

    IEnumerator SelectContinueButtonLater(GameObject selected)
    {
        yield return null;
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(selected);
    }

    IEnumerator ChangeToPause()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        gameStatus = GameStatus.Pause;
        ChangeSelected(uicontroller.buttons.resumeButton);
    }

    public void ChangeToItemBox()
    {
        ItemBox.instance.UpdateSlotsBox();
        gameStatus = GameStatus.ItemBox;
    }

    public void ChangeStateToGame()
    {
        StartCoroutine(WaitToGame());
    }

    IEnumerator WaitToGame()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        gameStatus = GameStatus.Game;
    }

    public void StateToDie()
    {
        deathCam.gameObject.SetActive(true);
        deathCam.m_Priority = 50;

        gameStatus = GameStatus.Dead;
        ChangeSelected(uicontroller.buttons.loadGameInDeathPanel);
    }

    private void OnApplicationPause(bool pause)
    {
        if (gameStatus == GameStatus.Game)
            StartCoroutine(ChangeToPause());
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
