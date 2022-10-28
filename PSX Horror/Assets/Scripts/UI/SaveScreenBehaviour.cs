using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SaveScreenBehaviour : MonoBehaviour
{
    public SaveMode mode;

    SaveSlotBehaviour selectedSlot;

    public Text selectSlotText;
    public GameObject adsPanel, replacePanel;
    public Transform adsGrid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (mode)
        {
            case SaveMode.Save:
                SaveUpdate();
                break;
            case SaveMode.Load:
                LoadUpdate();
                break;
        }
    }

    void SaveUpdate()
    {
        if (!selectedSlot)
        {
            selectSlotText.enabled = true;
            adsPanel.SetActive(false);
            GetComponent<EscToBack>().enabled = true;
            replacePanel.SetActive(false);
        }
        else
        {
            selectSlotText.enabled = false;
            GetComponent<EscToBack>().enabled = false;
        }
    }

    void LoadUpdate()
    {
        if (!selectedSlot)
        {
            selectSlotText.enabled = true;
            adsPanel.SetActive(false);
            GetComponent<EscToBack>().enabled = true;
        }
        else
        {
            selectSlotText.enabled = false;
            GetComponent<EscToBack>().enabled = false;
        }
    }

    public void SelectSlot(SaveSlotBehaviour slot)
    {
        switch (mode)
        {
            case SaveMode.Load:
                if (!slot.isEmpty)
                {
                    selectedSlot = slot;

                    StartCoroutine(Acess());
                }
                break;
            case SaveMode.Save:
                if (slot.isEmpty)
                {
                    selectedSlot = slot;
                    StartCoroutine(Acess());
                
                }
                else
                {
                    selectedSlot = slot;
                    replacePanel.SetActive(true);
                    GameManager.instance.ChangeSelected(UIController.instance.buttons.yesSave);
                }
                break;
        }
    }

    public void InputAcess()
    {
        StartCoroutine(Acess());
        replacePanel.SetActive(false);
    }

    public void Clear()
    {
        replacePanel.SetActive(false);
        GameManager.instance.ChangeSelected(selectedSlot.gameObject);
        selectedSlot = null;
    }

    IEnumerator Acess()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (InventoryUI.instance)
            InventoryUI.instance.PlayAcceptAudio();
        else if (MainMenu.instance)
            MainMenu.instance.PlayAcceptAudio();

        foreach (Transform child in adsGrid)
        {
            child.gameObject.SetActive(false);
        }

        adsPanel.SetActive(true);

        for (int i = 0; i < adsGrid.transform.childCount; i++)
        {
            adsGrid.transform.GetChild(i).gameObject.SetActive(true);
            if (i < adsGrid.transform.childCount - 1)
            {
                yield return new WaitForSecondsRealtime(3);
                adsGrid.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        if (InventoryUI.instance)
            InventoryUI.instance.PlayAcceptAudio();
        else if (MainMenu.instance)
            MainMenu.instance.PlayAcceptAudio();

        selectedSlot.Interact();
        selectedSlot = null;

        if(mode == SaveMode.Save)
        {
            if (GameManager.instance)
            {
                GameManager.instance.ChangeStateToGame();
            }
        }

        adsPanel.SetActive(false);
    }
}
