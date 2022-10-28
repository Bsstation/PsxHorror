using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class FirstConfigurations : MonoBehaviour
{
    public static FirstConfigurations instance;
    public GameObject languageScreen, mouseScreen, firstLanguage, firstMouse, mask;

    public bool canSkip;

    [Header("SFX")]
    public AudioClip moveSfx;
    public AudioClip acceptSfx;
    public AudioClip cancelSfx;

    AudioSource audioS;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        //languageScreen.SetActive(false);
        mouseScreen.SetActive(false);

        if (canSkip && PlayerPrefs.HasKey("Language"))
        {
            languageScreen.SetActive(false);

            SceneManager.LoadScene(1);
        }
        else
        {
            Settings.instance.cursorOn = true;

            EnableCursor(true);

            languageScreen.SetActive(true);

            EventSystem.current.SetSelectedGameObject(firstLanguage);
        }
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

    public void SelectLanguage(int index)
    {
        PlayerPrefs.SetInt("Language", index);

        languageScreen.SetActive(false);

        if (InputManager.instance.mode == InputMode.keyboard)
        {
            mouseScreen.SetActive(true);
            EventSystem.current.SetSelectedGameObject(firstMouse);
            PlayMoveAudio();
        }
        else
        {
            StartCoroutine(WaitSoundFx());
        }
    }

    public void SelectMouseMode(int index)
    {
        PlayerPrefs.SetInt("Cursor", index);
        mouseScreen.SetActive(false);

        StartCoroutine(WaitSoundFx());
    }

    IEnumerator WaitSoundFx()
    {
        EventSystem.current.SetSelectedGameObject(null);

        PlayAcceptAudio();
        mask.SetActive(true);

        yield return new WaitForSecondsRealtime(acceptSfx.length);
        SceneManager.LoadScene(1);
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
