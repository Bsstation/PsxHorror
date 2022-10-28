using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FileUI : MonoBehaviour, IPointerClickHandler, ISubmitHandler, IPointerEnterHandler
{
    public ItemFile currentFile;
    private Text text;
    public GameObject popin;

    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetChild(0).GetComponent<Text>();
        text.text = currentFile.fileLangs[0].fileTitle;

        popin.SetActive(!currentFile.hasReaded);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        currentFile.hasReaded = true;
        popin.SetActive(!currentFile.hasReaded);

        InventoryUI.instance.filesReader.SelectFile(this);
        InventoryUI.instance.filesReader.Update();
        InventoryUI.instance.filesPanel.gameObject.SetActive(true);
        InventoryUI.instance.PlayAcceptAudio();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        currentFile.hasReaded = true;
        popin.SetActive(!currentFile.hasReaded);

        InventoryUI.instance.filesReader.SelectFile(this);
        InventoryUI.instance.filesReader.Update();
        InventoryUI.instance.filesPanel.gameObject.SetActive(true);
        InventoryUI.instance.PlayAcceptAudio();
    }

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
        }
    }
}
