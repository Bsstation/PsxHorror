using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInpuManagerGroup : MonoBehaviour
{
    public CheckInputManagerMode[] checks;
    public InputManager inputManager;

    // Start is called before the first frame update
    void Init()
    {
        checks = FindObjectsOfTypeAll(typeof(CheckInputManagerMode)) as CheckInputManagerMode[];

        if(InputManager.instance && !inputManager)
            inputManager = InputManager.instance;
    }

    // Update is called once per frame
    public void SetupAll()
    {
        Init();

        if (inputManager)
        {
            for (int i = 0; i < checks.Length; i++)
            {
                if (inputManager.mode == InputMode.joystick)
                {
                    checks[i].joystickInputs.SetActive(true);
                    checks[i].keyboardInputs.SetActive(false);
                }
                else
                {
                    checks[i].keyboardInputs.SetActive(true);
                    checks[i].joystickInputs.SetActive(false);
                }
            }
        }

        Destroy(gameObject);
    }
}
