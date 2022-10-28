using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FilesPanel : MonoBehaviour
{
    public ItemFile currentFile;

    public Image fileIcon;
    public Text title;
    public Text fileText;
    public Text pags;
    public int pageOfFile;

    public GameObject pageUp, pageDown;
    public GameObject normalGuide, readingGuide;
    bool m_isAxisInUse = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        int lang = PlayerPrefs.GetInt("Language");
        if (currentFile && GameManager.instance.gameStatus == GameStatus.Inventory)
        {
            normalGuide.SetActive(false);
            readingGuide.SetActive(true);
            pags.enabled = true;
            pags.text = pageOfFile + 1 + "/" + currentFile.fileLangs[lang].text.Capacity;
            fileIcon.sprite = currentFile.backgroundImage;
            fileIcon.enabled = true;
            title.text = currentFile.fileLangs[lang].fileTitle;
            title.enabled = false;
            if (pageOfFile == 0)
                title.enabled = true;
            fileText.text = currentFile.fileLangs[lang].text[pageOfFile];
            fileText.enabled = true;

            if ((InputManager.instance.UiMovementWithoutMouse().x < 0 && !m_isAxisInUse) || (InputManager.instance.JoystickMove().x < 0 && !m_isAxisInUse))
            {
                PageDown();
                m_isAxisInUse = true;
            }
            if ((InputManager.instance.UiMovementWithoutMouse().x > 0 && !m_isAxisInUse) || (InputManager.instance.JoystickMove().x > 0 && !m_isAxisInUse))
            {
                PageUp();
                m_isAxisInUse = true;
            }

            if (InputManager.instance.JoystickMove().x == 0 && InputManager.instance.UiMovementWithoutMouse().x == 0)
                m_isAxisInUse = false;

            if (pageOfFile < currentFile.fileLangs[lang].text.Capacity - 1)
            {
                pageUp.gameObject.SetActive(true);
            }
            else
            {
                pageUp.gameObject.SetActive(false);
            }

            if (pageOfFile > 0)
            {
                pageDown.gameObject.SetActive(true);
            }
            else
            {
                pageDown.gameObject.SetActive(false);
            }
        }
        else
        {
            normalGuide.SetActive(true);
            readingGuide.SetActive(false);

            fileIcon.enabled = false;
            pags.enabled = false;
            title.enabled = false;
            fileText.enabled = false;
            pageUp.gameObject.SetActive(false);
            pageDown.gameObject.SetActive(false);
        }
    }

    public void PageUp()
    {
        int lang = PlayerPrefs.GetInt("Language");

        if (pageOfFile < currentFile.fileLangs[lang].text.Capacity - 1)
        {
            pageOfFile += 1;
            InventoryUI.instance.PlayFlipPageAudio();
        }
    }

    public void PageDown()
    {
        if (pageOfFile > 0)
        {
            pageOfFile -= 1;
            InventoryUI.instance.PlayFlipPageAudio();
        }
    }

    public void SelectFile(FileUI slot)
    {
        currentFile = slot.currentFile;
        pageOfFile = 0;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ClearSelectArchives()
    {
        currentFile = null;
        pageOfFile = 0;
    }
}
