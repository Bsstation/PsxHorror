using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Difficulty { Easy, Normal, Hard, Extreme }

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    AudioSource audioS;

    [Header("SFX")]
    public AudioClip moveSfx;
    public AudioClip acceptSfx;
    public AudioClip cancelSfx;


    [Header("New Game Values")]
    public Difficulty difficulty;
    public bool isTank;
    public Text tankText;
    

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        audioS = GetComponent<AudioSource>();

        isTank = (PlayerPrefs.GetInt("Tank") == 0) ? true : false;
        tankText.text = (isTank) ? "Classic" : "Alternative";

        SceneController s = FindObjectOfType(typeof(SceneController)) as SceneController;
        PlayerStates p = FindObjectOfType(typeof(PlayerStates)) as PlayerStates;

        if (p) Destroy(p.gameObject);
        if (s) Destroy(s.gameObject);

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

    public void Update()
    {
        if (InputManager.instance.mode == InputMode.keyboard && Settings.instance.cursorOn)
        {
            if (Cursor.lockState == CursorLockMode.None || Cursor.visible == true)
            {
                if(InputManager.instance.UiMovementWithoutMouse() != Vector2.zero || Input.GetKey(KeyCode.Return))
                {
                    EnableCursor(false);
                }
            }
            else if (Cursor.lockState == CursorLockMode.Locked || Cursor.visible == false)
            {
                if(Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 || Input.GetKey(KeyCode.Mouse0) ||
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

    public void NewGame(int value)
    {
        DeleteKeys();

        PlayAcceptAudio();

        difficulty = (Difficulty)value;

        PlayerPrefs.SetInt("Difficulty", (int)difficulty);
        PlayerPrefs.SetInt("Tank", BoolToInt(!isTank));

        EventSystem.current.SetSelectedGameObject(null);

        MusicController.instance.Fade(MusicController.FadeMusic.OUT);

        FadeInOut fadeInOut = FindObjectOfType(typeof(FadeInOut)) as FadeInOut;
        fadeInOut.NextScene();
    }

    public void DeleteK()
    {
        DeleteKeys();
    }

    public static void DeleteKeys()
    {
        //player
        PlayerPrefs.DeleteKey("Player Life");
        PlayerPrefs.DeleteKey("Current Weapon");

        PlayerPrefs.DeleteKey("Flash On");

        //inventory
        PlayerPrefs.DeleteKey("SlotsAvailable");
        for (int i = 0; i < 16; i++)
        {
            PlayerPrefs.DeleteKey("Slot Item Temp " + i);
            PlayerPrefs.DeleteKey("Slot Amount Temp " + i);
        }

        int y = PlayerPrefs.GetInt("Files Count");

        for (int i = 0; i < y; i++)
        {
            PlayerPrefs.DeleteKey("Slot File Temp " + i);
            PlayerPrefs.DeleteKey("Slot Readed Temp " + i);
        }
        PlayerPrefs.DeleteKey("Files Count");

        for (int i = 0; i < 20; i++)
        {
            PlayerPrefs.DeleteKey("Map " + i);
        }

        for (int i = 0; i < 4; i++)
        {
            PlayerPrefs.DeleteKey("Shortcut" + i);
        }

        //item box

        int x = PlayerPrefs.GetInt("ItemsCount");
        for (int i = 0; i < x; i++)
        {
            PlayerPrefs.DeleteKey("Slot ItemBox Temp " + i);
            PlayerPrefs.DeleteKey("Slot ItemBox Amount Temp " + i);
        }

        PlayerPrefs.DeleteKey("ItemsCount");
    }

    public void TnkValue()
    {
        PlayMoveAudio();

        isTank = !isTank;
        tankText.text = (isTank) ? "Classic" : "Alternative";
    }

    public int BoolToInt(bool b)
    {
        return (b) ? 1 : 0; 
    }

    public void Quit()
    {
        Application.Quit();
    }

    #region Audio

    public void PlayAudio(AudioClip audio)
    {
        audioS.PlayOneShot(audio);
    }

    public void PlayAcceptAudio()
    {
        audioS.PlayOneShot(acceptSfx);
    }

    public void PlayCancelAudio()
    {
        audioS.PlayOneShot(cancelSfx);
    }

    public void PlayMoveAudio()
    {
        audioS.PlayOneShot(moveSfx);
    }

    #endregion
}
