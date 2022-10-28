using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SliderFunction : MonoBehaviour
{
    public UnityEvent onPressLeft;
    public UnityEvent onPressRight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;

        Vector2 move = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.UiMovementWithoutMouse() : InputManager.instance.JoystickMove();

        if (move.x > 0)
        {
            onPressRight.Invoke();
        }
        else if(move.x < 0)
        {
            onPressLeft.Invoke();
        }
    }
}
