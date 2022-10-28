using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PressSideFunction : MonoBehaviour
{
    public UnityEvent onPressLeft;
    public UnityEvent onPressRight;

    public bool pressing;

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;

        Vector2 move = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.UiMovementWithoutMouse() : InputManager.instance.JoystickMove();

        if (pressing == false)
        {
            if (move.x > 0)
            {
                pressing = true;
                onPressRight.Invoke();
            }
            else if (move.x < 0)
            {
                pressing = true;
                onPressLeft.Invoke();
            }
        }

        pressing = move != Vector2.zero;
    }
}
