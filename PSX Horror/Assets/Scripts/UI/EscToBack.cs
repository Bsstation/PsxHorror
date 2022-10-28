using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EscToBack : MonoBehaviour
{
    public UnityEvent onPressEsc;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel") || (InputManager.instance && (Input.GetKeyDown(InputManager.instance.kKeys.sprint) ||
            InputManager.instance.GetJoyButtonDown("B"))))
        {
            StartCoroutine(Event());
        }
    }

    IEnumerator Event()
    {
        yield return null;
        onPressEsc.Invoke();
        if (InventoryUI.instance)
        {
            InventoryUI.instance.PlayCancelAudio();
        }
        else if (MainMenu.instance)
        {
            MainMenu.instance.PlayCancelAudio();
        }
    }
}
