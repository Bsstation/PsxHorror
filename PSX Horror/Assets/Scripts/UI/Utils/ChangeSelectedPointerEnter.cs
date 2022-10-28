using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeSelectedPointerEnter : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current && EventSystem.current.currentSelectedGameObject != gameObject &&
            GetComponent<Button>() && GetComponent<Button>().interactable &&
            GetComponent<Button>().navigation.mode != Navigation.Mode.None &&
            Settings.instance.cursorOn && InputManager.instance.mode == InputMode.keyboard)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);

            if (InventoryUI.instance)
                InventoryUI.instance.PlayMoveAudio();
            else if (MainMenu.instance)
                MainMenu.instance.PlayMoveAudio();
            else if (FirstConfigurations.instance)
                FirstConfigurations.instance.PlayMoveAudio();
        }
    }
}
