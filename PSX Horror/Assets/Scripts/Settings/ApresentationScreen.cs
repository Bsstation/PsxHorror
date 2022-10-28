using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ApresentationScreen : MonoBehaviour
{
    private void Start()
    {
        EnableCursor(true);
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
        if (InputManager.instance.mode == InputMode.keyboard && (PlayerPrefs.GetInt("Cursor") == 0))
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
}
