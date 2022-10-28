using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    #region Variables

    private PlayerController player;

    AudioSource audioSource;

    [HideInInspector]
    public ItemBase itemToAdd, selectedItem;
    public GameObject examinePanel;

    //items
    [Header("Items")]
    public int slotsAvailable;
    public GameObject itemsPanel;
    public GameObject equipButton, useButton, combineButton, examineButton, dropButton;
    public SlotItemBehaviour[] slots;
    [HideInInspector]
    public bool upgrading;
    public ScrollUpdateY itemsScroll;

    [Space]
    public bool examing;
    public Transform examinePivot;
    public Camera examineCam;
    public float speedAutoRotate, speedExamination;
    public GameObject commentaryPanel, normalGuide, examineGuide;
    public Text commentaryText, movingInputText;
    [HideInInspector]
    public bool typewriting;

    [Space]
    public bool moving;
    public SlotItemBehaviour selectedSlot, movingSlot;
    public RectTransform itemMenu;

    [Space]
    public ShortcutSlot[] shortcuts;
    public ItemBase itemToShortcut;
    public GameObject shortInHud;

    //files
    [Header("Files")]
    public GameObject filesPanel;
    public FilesPanel filesReader;
    public Transform filesContent;

    //audios
    [Header("SFX")]
    public AudioClip moveSfx;
    public AudioClip acceptSfx;
    public AudioClip cancelSfx;
    public AudioClip errorSfx;

    [Space]
    public AudioClip flipPageSfx;

    [Space]
    public AudioClip recoverySfx;
    public AudioClip equipSfx;
    public AudioClip flashlightSfx;

    [System.Serializable]
    public class Files
    {
        public List<ItemFile> files = new List<ItemFile>();
    }

    public Files currentFiles;

    [System.Serializable]
    public struct Colors
    {
        public Color normalColor;
        public Color combineColor;
        public Color movingColor;
        public Color selectingColor;
    }

    public Colors colors;

    //maps
    [Header("Maps")]
    public MapPanel mapPanel;
    public bool[] maps;

    //all
    public Transform inputs;
    public Image itemsMask;
    public Image filesMask;

    public static InventoryUI instance;

    #endregion

    #region Init
    private void Awake()
    {
        instance = this;
        player = FindObjectOfType(typeof(PlayerController)) as PlayerController;
        mapPanel.Init();
        mapPanel.DisableCam();
        itemsScroll.Start();
        ShowItems();

        ResetHotkeysInHud();
    }

    void Start()
    {
        if (!player)
            player = GameManager.instance.player;

        //UpdateSlots();
        audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        movingInputText.text = InputManager.instance.kKeys.reload.ToString();

        if (shortInHud.activeSelf && GameManager.instance.gameStatus != GameStatus.Game) ResetHotkeysInHud();

        //items
        if (selectedSlot || selectedItem || itemToAdd || moving || mapPanel.zoom || filesReader.currentFile)
        {
            foreach (Transform c in inputs)
            {
                if (c.GetComponent<Button>() && c.GetComponent<Button>().navigation.mode == Navigation.Mode.Automatic)
                {
                    c.GetComponent<Button>().navigation = new Navigation() { mode = Navigation.Mode.None };
                    c.GetComponent<Button>().interactable = false;
                }
            }
        }
        else
        {
            foreach (Transform c in inputs)
            {
                if (c.GetComponent<Button>() && c.GetComponent<Button>().navigation.mode == Navigation.Mode.None)
                {
                    c.GetComponent<Button>().navigation = new Navigation() { mode = Navigation.Mode.Automatic };
                    c.GetComponent<Button>().interactable = true;
                }
            }
        }

        if (!typewriting)
        {
            if (upgrading)
            {
                commentaryText.text = MessagesBehaviour.instance.youUpgradeInventory.msgs[Settings.instance.currentLanguage];
                GameManager.instance.ChangeSelected(null);
            }
            else if (selectedItem && !examing)
            {
                commentaryText.text = selectedItem.itemName[Settings.instance.currentLanguage];
            }
            else if (!moving && !itemToAdd && !selectedSlot && !examing && itemsPanel.activeInHierarchy &&
                        commentaryText.text != "" && typewriting == true)
            {
                ClearMessage();
            }
        }

        RotateExamineObject();

        examinePanel.SetActive(itemToAdd || examing || selectedItem);
        itemsMask.enabled = upgrading || examing || selectedItem || itemToShortcut;
        filesMask.gameObject.SetActive(filesReader.currentFile);

        examineGuide.SetActive(examing);
        normalGuide.SetActive(!examing);

        if (itemToAdd)
            examinePivot.Rotate(0, speedAutoRotate * Time.unscaledDeltaTime, 0);

        if (selectedItem)
        {
            if (selectedItem.type == ItemType.Weapon)
            {
                equipButton.SetActive(true);
                useButton.SetActive(false);
            }
            else
            {
                equipButton.SetActive(false);
                useButton.SetActive(true);
            }

            combineButton.SetActive(true);
            dropButton.SetActive(true);

            if (!examing)
            {
                itemMenu.gameObject.SetActive(true);
                examinePivot.Rotate(0, speedAutoRotate * Time.unscaledDeltaTime, 0);
            }
            else
                itemMenu.gameObject.SetActive(false);
        }
        else
        {
            itemMenu.gameObject.SetActive(false);

            equipButton.SetActive(false);
            useButton.SetActive(false);
            combineButton.SetActive(false);
            dropButton.SetActive(false);
        }

        if (GameManager.instance.gameStatus == GameStatus.Inventory && !upgrading && !moving &&
            (Input.GetButtonDown("Cancel") || Input.GetKeyDown(InputManager.instance.kKeys.sprint) ||
            InputManager.instance.GetJoyButtonDown("B")))
        {
            PlayCancelAudio();

            if (itemToAdd)
            {
                GameManager.instance.ChangeStateToGame();
                ClearItems();
            }
            else
            {
                if (selectedSlot)
                {
                    selectedSlot.GetComponent<Image>().color = colors.normalColor;

                    foreach (SlotItemBehaviour slot in slots)
                    {
                        if (slot == selectedSlot)
                        {
                            selectedItem = selectedSlot.currentItem;
                            selectedSlot = null;
                            slot.Interact();
                            GameManager.instance.ChangeSelected(combineButton);
                            break;
                        }
                    }
                }
                else if (selectedItem)
                {
                    if (examing)
                    {
                        examing = false;
                        GameManager.instance.ChangeSelected(examineButton);
                        examinePivot.localEulerAngles = new Vector3(0, -90, 0);
                    }
                    else
                    {
                        foreach (SlotItemBehaviour slot in slots)
                        {
                            if (slot.currentItem == selectedItem)
                                GameManager.instance.ChangeSelected(slot.gameObject);
                        }
                        ClearItems();
                    }
                }
                else if (itemToShortcut)
                {
                    GameManager.instance.ChangeSelected(GetSlotSelectedItem(itemToShortcut));
                    itemToShortcut = null;
                }
                else if (itemsPanel.activeInHierarchy)
                {
                    if (EventSystem.current.currentSelectedGameObject.transform.parent == inputs || !EventSystem.current.currentSelectedGameObject)
                    {
                        if (EventSystem.current.currentSelectedGameObject == inputs.GetChild(3).gameObject)
                            GameManager.instance.ChangeStateToGame();
                        else
                            GameManager.instance.ChangeSelected(inputs.GetChild(3).gameObject);
                    }
                    else
                            GameManager.instance.ChangeSelected(inputs.GetChild(0).gameObject);
                }
                else if (filesPanel.activeInHierarchy)
                {
                    if (filesReader.currentFile)
                    {
                        foreach (Transform child in filesContent)
                        {
                            if (child.GetComponent<FileUI>().currentFile == filesReader.currentFile)
                                GameManager.instance.ChangeSelected(child.gameObject);
                        }
                    }
                    else
                    {
                        if (EventSystem.current.currentSelectedGameObject.transform.parent == inputs)
                        {
                            if (EventSystem.current.currentSelectedGameObject == inputs.GetChild(3).gameObject)
                                GameManager.instance.ChangeStateToGame();
                            else
                                GameManager.instance.ChangeSelected(inputs.GetChild(3).gameObject);
                        }
                        else
                            GameManager.instance.ChangeSelected(inputs.GetChild(1).gameObject);
                    }

                    filesReader.ClearSelectArchives();
                }
            }
        }


        if (moving && GameManager.instance.gameStatus == GameStatus.Inventory && !upgrading &&
            (Input.GetButtonDown("Cancel") || Input.GetKeyDown(InputManager.instance.kKeys.sprint) ||
            InputManager.instance.GetJoyButtonDown("B")))
        {
            PlayMoveAudio();
            moving = false;
            movingSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
            GameManager.instance.ChangeSelected(movingSlot.gameObject);
            movingSlot = null;
        }
    }
    #endregion

    #region Messages

    IEnumerator ShowText(string message)
    {
        typewriting = true;
        commentaryText.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            commentaryText.text = commentaryText.text + message[i];

            yield return new WaitForSecondsRealtime(0.02f);
        }

        yield return new WaitForSecondsRealtime(2f);

        typewriting = false;
    }

    public void TakeItemMessage()
    {
        StopAllCoroutines();
        StartCoroutine(ShowText("Where do you want to place this item?"));
    }

    public void CannotPut()
    {
        StopAllCoroutines();
        StartCoroutine(ShowText("You can not put here."));
        PlayErrorAudio();
        Invoke("TakeItemMessage", 4);
    }

    public void CannotMix()
    {
        StopAllCoroutines();
        StartCoroutine(ShowText("You can not mix with this item."));
        PlayErrorAudio();
        Invoke("ClearMessage", 4);
    }

    public void ClearMessage()
    {
        StopAllCoroutines();
        commentaryText.text = "";
        typewriting = false;
    }

    #endregion

    public void ActiveAvailableSlots()
    {
        for(int i = 0; i < slots.Length; i++)
            slots[i].gameObject.SetActive(false);

        for(int i = 0; i < slotsAvailable; i++)
            slots[i].gameObject.SetActive(true);
    }

    public void RotateExamineObject()
    {
        if (examing)
        {
            Vector2 velocity = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.UiMovementWithMouse() :
                InputManager.instance.JoystickMove();

            float xAxis = velocity.x * speedExamination * Time.unscaledDeltaTime;
            float yAxis = velocity.y * speedExamination * Time.unscaledDeltaTime;

            examinePivot.Rotate(Vector3.up, -xAxis, Space.World);
            examinePivot.Rotate(Vector3.right, yAxis, Space.World);
        }
    }

    public SlotItemBehaviour SearchAvailableSlot(ItemBase item)
    {
        bool stage1 = false;
        for (int i = 0; i < slotsAvailable; i++)
        {
            if (item.isStock)
            {
                if (!stage1)
                {
                    if (slots[i].currentItem && item.itemNameId == slots[i].currentItem.itemNameId &&
                        slots[i].currentItem.amount < slots[i].currentItem.amountLimit)
                        return slots[i];

                    if (i == slotsAvailable - 1)
                    {
                        i = 0;
                        stage1 = true;
                    }
                }

                if (stage1)
                {
                    if (!slots[i].currentItem)
                        return slots[i];
                }
            }
            else
            {
                if (!slots[i].currentItem)
                    return slots[i];
            }
        }
        return slots[0];
    }

    public int CheckAmmo(WeaponType type)
    {
        int ammoAmount = 0;
        for (int i = 0; i < slotsAvailable; i++)
        {
            if (slots[i].currentItem && slots[i].currentItem.type == ItemType.Ammo &&
                slots[i].currentItem.GetComponent<ItemAmmo>().ammoType == type)
                ammoAmount = slots[i].currentItem.amount;
        }
        return ammoAmount;
    }

    #region Save & Load

    public void SaveItems()
    {
        PlayerPrefs.SetInt("SlotsAvailable", slotsAvailable);

        for (int i = 0; i < slotsAvailable; i++)
        {
            if (slots[i].currentItem)
            {
                PlayerPrefs.SetString("Slot Item Temp " + i, slots[i].currentItem.itemNameId);
                if (slots[i].currentItem.type != ItemType.Weapon)
                {
                    if (slots[i].currentItem.isStock)
                        PlayerPrefs.SetInt("Slot Amount Temp " + i, slots[i].currentItem.amount);
                }
                else
                    PlayerPrefs.SetInt("Slot Amount Temp " + i, slots[i].currentItem.GetComponent<WeaponBase>().currentAmmo);
            }
            else
            {
                PlayerPrefs.DeleteKey("Slot Item Temp " + i);
                PlayerPrefs.DeleteKey("Slot Amount Temp " + i);
            }
        }

        for(int i = 0; i < shortcuts.Length; i++)
        {
            if (shortcuts[i] && shortcuts[i].currentItem)
                PlayerPrefs.SetInt("Shortcut" + i, GetSlotNumber(shortcuts[i].currentItem));
            else
                PlayerPrefs.DeleteKey("Shortcut" + i);
        }

        SaveFiles();

        PlayerPrefs.SetInt("Map Amount", maps.Length);

        for (int i = 0; i < maps.Length; i++)
        {
            if(maps[i] == true)
                PlayerPrefs.SetInt("Map " + i, 1);
            else
                PlayerPrefs.SetInt("Map " + i, 0);
        }
    }

    public void LoadItems()
    {
        slotsAvailable = PlayerPrefs.GetInt("SlotsAvailable");
        if (slotsAvailable == 0) slotsAvailable = 4;

        ClearItems();
        for (int i = 0; i < slotsAvailable; i++)
        {
            if (PlayerPrefs.HasKey("Slot Item Temp " + i))
            {
                GameObject tempItem = Resources.Load("Items/" + PlayerPrefs.GetString("Slot Item Temp " + i)) as GameObject;
                if (slots[i].currentItem)
                    Destroy(slots[i].currentItem);

                slots[i].currentItem = Instantiate(tempItem).GetComponent<ItemBase>();

                if (slots[i].currentItem.type != ItemType.Weapon)
                {
                    if (slots[i].currentItem.isStock)
                        slots[i].currentItem.amount = PlayerPrefs.GetInt("Slot Amount Temp " + i);
                }
                else
                    slots[i].currentItem.GetComponent<WeaponBase>().currentAmmo = PlayerPrefs.GetInt("Slot Amount Temp " + i);

                slots[i].currentItem.gameObject.layer = 8;

                Transform[] childs = slots[i].currentItem.GetComponentsInChildren<Transform>() as Transform[];

                foreach (Transform child in childs)
                    child.gameObject.layer = 8;

                slots[i].currentItem.gameObject.SetActive(false);
            }
        }

        LoadFiles();

        System.Array.Resize(ref maps, PlayerPrefs.GetInt("Map Amount"));

        for (int i = 0; i < maps.Length; i++)
        {
            if (PlayerPrefs.GetInt("Map " + i) == 1)
                maps[i] = true;
            else
                maps[i] = false;
        }
        MapController.instance.Start();
        mapPanel.Update();

        for(int i =0; i < shortcuts.Length; i++)
        {
            if(PlayerPrefs.HasKey("Shortcut" + i) && slots[PlayerPrefs.GetInt("Shortcut" + i)].currentItem)
                shortcuts[i].currentItem = slots[PlayerPrefs.GetInt("Shortcut" + i)].currentItem;
        }

        ActiveAvailableSlots();
        //files
        RefreshFiles();

        Update();
        UpdateSlots();
    }

    public void SaveFiles()
    {
        PlayerPrefs.SetInt("Files Count", currentFiles.files.Count);

        for (int i = 0; i < currentFiles.files.Count; i++)
        {
            if (currentFiles.files[i])
            {
                PlayerPrefs.SetString("Slot File Temp " + i, currentFiles.files[i].itemNameId);
                if (currentFiles.files[i].hasReaded)
                    PlayerPrefs.SetString("Slot Readed Temp " + i, currentFiles.files[i].itemNameId);
                else
                    PlayerPrefs.DeleteKey("Slot Readed Temp " + i);
            }
        }
    }

    public void LoadFiles()
    {
        int count = PlayerPrefs.GetInt("Files Count");

        for (int i = 0; i < count; i++)
        {
            var temp = Resources.Load("Files/" + PlayerPrefs.GetString("Slot File Temp " + i)) as GameObject;
            GameObject tempFile = Instantiate(temp);
            currentFiles.files.Add(tempFile.GetComponent<ItemFile>());
            tempFile.SetActive(false);

            if (PlayerPrefs.HasKey("Slot Readed Temp " + i))
                tempFile.GetComponent<ItemFile>().hasReaded = true;
            else
                tempFile.GetComponent<ItemFile>().hasReaded = false;
        }
    }

    #endregion

    public void ClearItems()
    {
        itemToAdd = selectedItem = null;
        selectedSlot = null;

        filesReader.ClearSelectArchives();

        ClearExamineItem();
        ClearMessage();
        itemToShortcut = null;

        foreach(ShortcutSlot slot in shortcuts)
            slot.CheckCurrentItemExistsInInventory();
    }

    public void ClearExamineItem()
    {
        foreach(Transform child in examinePivot)
            Destroy(child.gameObject);
    }

    public void AddInExaminePanel(string path)
    {
        ClearExamineItem();

        var tempItem = Resources.Load("Items/" + path) as GameObject;
        GameObject newItem = Instantiate(tempItem);
        newItem.transform.parent = examinePivot;
        newItem.transform.localEulerAngles = newItem.transform.localPosition = Vector3.zero;

        newItem.layer = 12;

        Transform[] childs = newItem.GetComponentsInChildren<Transform>() as Transform[];

        foreach (Transform child in childs)
            child.gameObject.layer = 12;

        examinePivot.localEulerAngles = new Vector3(0, -90, 0);
        Vector3 newPos = new Vector3(examinePivot.localPosition.x, examinePivot.localPosition.y, newItem.GetComponent<ItemBase>().examineDistance);
        examinePivot.localPosition = newPos;
    }

    public void UpdateSlots()
    {
        foreach(SlotItemBehaviour slot in slots)
            slot.Update();

        foreach (ShortcutSlot slot in shortcuts)
        {
            slot.Update();
            slot.CheckCurrentItemExistsInInventory();
        }
    }

    #region Inputs

    public void UseItem()
    {
        foreach (SlotItemBehaviour slot in slots)
        {
            if (slot.currentItem == selectedItem)
            {
                GameManager.instance.ChangeSelected(slot.gameObject);
                break;
            }
        }
        selectedItem.Use();
        ClearItems();
    }

    public void CombineItem()
    {
        ClearMessage();
        foreach (SlotItemBehaviour slot in slots)
        {
            if (slot.currentItem == selectedItem)
            {
                selectedSlot = slot;
                selectedSlot.GetComponent<Image>().color = colors.combineColor;
                GameManager.instance.ChangeSelected(slot.gameObject);
                break;
            }
        }
        PlayMoveAudio();
        itemToAdd = selectedItem = null;
    }

    public void ExamineItem()
    {
        PlayMoveAudio();
        examing = true;
        GameManager.instance.ChangeSelected(null);
        StopAllCoroutines();
        StartCoroutine(ShowText(selectedItem.itemName[Settings.instance.currentLanguage]));
    }

    public void DragExamine()
    {
        if (examing)
        {
            Vector2 velocity = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            float xAxis = velocity.x * speedExamination * Time.unscaledDeltaTime;
            float yAxis = velocity.y * speedExamination * Time.unscaledDeltaTime;

            examinePivot.Rotate(Vector3.up, -xAxis, Space.World);
            examinePivot.Rotate(Vector3.right, yAxis, Space.World);
        }
    }

    public void ShortcutSelect()
    {
        PlayMoveAudio();
        itemToShortcut = selectedItem;
        selectedItem = null;

        GameManager.instance.ChangeSelected(shortcuts[0].gameObject);
    }

    public void DropItem()
    {
        foreach (SlotItemBehaviour slot in slots)
        {
            if (slot.currentItem == selectedItem)
            {
                if (selectedItem.canDiscard)
                {
                    PlayAcceptAudio();
                    selectedItem = null;
                    Destroy(slot.currentItem.gameObject);
                    slot.currentItem = null;

                    GameManager.instance.ChangeSelected(slot.gameObject);
                    ClearItems();
                    break;
                }
                else
                {
                    //colocar mensagem
                    typewriting = true;
                    StopAllCoroutines();
                    StartCoroutine(ShowText(MessagesBehaviour.instance.cannotDiscard.msgs[Settings.instance.currentLanguage]));
                    PlayErrorAudio();
                    break;
                }
            }
        }
    }

    public void SearchShortcutFree(ItemBase item)
    {
        if (item.type != ItemType.Weapon) return;

        UpdateSlots();

        foreach(ShortcutSlot slot in shortcuts)
        {
            if (!slot.currentItem)
            {
                slot.currentItem = item;
                break;
            }
        }
    }

    #endregion

    int hot = 0;

    Coroutine shortcutRoutine;

    public void ShowHotkeys(int slot)
    {
        UpdateSlots();
        ResetHotkeysInHud();
        hot = slot - 1;
        shortcutRoutine = StartCoroutine(Sh());
    }

    IEnumerator Sh()
    {
        for(int i = 0; i < shortcuts.Length; i++)
        {
            //mark
            shortInHud.transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<Image>().enabled =
                (i == hot);

            //sprite
            shortInHud.transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<Image>().enabled =
                shortcuts[i].transform.GetChild(2).GetComponent<Image>().enabled;

            shortInHud.transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<Image>().sprite =
                shortcuts[i].transform.GetChild(2).GetComponent<Image>().sprite;

            //amount
            shortInHud.transform.GetChild(0).GetChild(i).GetChild(2).GetChild(0).GetComponent<Text>().enabled =
                shortcuts[i].transform.GetChild(2).GetChild(0).GetComponent<Text>().enabled;

            shortInHud.transform.GetChild(0).GetChild(i).GetChild(2).GetChild(0).GetComponent<Text>().text = 
                shortcuts[i].transform.GetChild(2).GetChild(0).GetComponent<Text>().text;

            shortInHud.transform.GetChild(0).GetChild(i).GetChild(2).GetChild(0).GetComponent<Text>().color =
                shortcuts[i].transform.GetChild(2).GetChild(0).GetComponent<Text>().color;
        }

        shortInHud.SetActive(true);

        yield return new WaitForSecondsRealtime(1);

        ResetHotkeysInHud();
    }

    void ResetHotkeysInHud()
    {
        if(shortcutRoutine != null)
            StopCoroutine(shortcutRoutine);
        shortInHud.SetActive(false);
    }

    public void ShowNewFile(ItemFile file)
    {
        ClearItems();
        ShowFiles();
        filesReader.currentFile = file;
        filesReader.Update();

        Update();
        UpdateSlots();
        ShowFiles();
        GameManager.instance.gameStatus = GameStatus.Inventory;
    }

    public void SelectItem(ItemBase item)
    {
        selectedItem = item;
        PlayMoveAudio();

        itemsScroll.SnapTo(GetSlotSelectedItem(selectedItem));
        itemMenu.position = GetSlotSelectedItem(selectedItem).transform.GetChild(0).position;

        AddInExaminePanel(item.itemNameId);

        GameManager.instance.ChangeSelected((selectedItem.type == ItemType.Weapon) ? equipButton : useButton);
    }

    public GameObject GetSlotSelectedItem(ItemBase item)
    {
        if (item)
        {
            foreach (SlotItemBehaviour slot in slots)
            {
                if (slot && slot.currentItem && slot.currentItem == item)
                    return slot.gameObject;
            }
        }
        return null;
    }

    public int GetSlotNumber(ItemBase item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].currentItem && slots[i].currentItem == item)
                return i;
        }

        return -1;
    }

    public void RefreshFiles()
    {
        foreach (Transform child in filesContent)
        {
            if(child)
                GameObject.Destroy(child.gameObject);
        }

        List<ItemFile> files = currentFiles.files;
        var tempUiFile = Resources.Load("Files/FileUIPrefab") as GameObject;

        foreach (ItemFile file in files)
        {
            tempUiFile.GetComponent<FileUI>().currentFile = file;
            Instantiate(tempUiFile, filesContent);
        }
    }

    #region Open & Close

    public void ShowItems()
    {
        if (!inputs.gameObject.activeSelf) inputs.gameObject.SetActive(true);

        mapPanel.DisableCam();

        itemsScroll.SnapTo(slots[0].gameObject);

        itemsPanel.SetActive(true);
        filesPanel.gameObject.SetActive(false);
        mapPanel.gameObject.SetActive(false);
    }

    public void ShowFiles()
    {
        if (!inputs.gameObject.activeSelf)  inputs.gameObject.SetActive(true);

        mapPanel.DisableCam();

        filesReader.Update();
        itemsPanel.SetActive(false);
        filesPanel.gameObject.SetActive(true);
        mapPanel.gameObject.SetActive(false);
    }

    public void OpenMap()
    {
        GameManager.instance.ChangeSelected(null);
        if (inputs.gameObject.activeSelf) inputs.gameObject.SetActive(false);

        mapPanel.zoom = false;

        itemsPanel.SetActive(false);
        filesPanel.gameObject.SetActive(false);

        mapPanel.EnableCam();
        mapPanel.Update();
        mapPanel.gameObject.SetActive(true);
    }

    #endregion

    #region Audio

    public void PlayFlipPageAudio()
    {
        audioSource.PlayOneShot(flipPageSfx);
    }

    public void PlayAudio(AudioClip audio)
    {
        audioSource.PlayOneShot(audio);
    }

    public void PlayAcceptAudio()
    {
        audioSource.PlayOneShot(acceptSfx);
    }

    public void PlayCancelAudio()
    {
        audioSource.PlayOneShot(cancelSfx);
    }

    public void PlayMoveAudio()
    {
        audioSource.PlayOneShot(moveSfx);
    }

    public void PlayErrorAudio()
    {
        audioSource.PlayOneShot(errorSfx);
    }

    public void PlayRecoveryAudio()
    {
        audioSource.PlayOneShot(recoverySfx);
    }

    public void PlayEquipAudio()
    {
        audioSource.PlayOneShot(equipSfx);
    }

    public void PlayFlashlight()
    {
        audioSource.PlayOneShot(flashlightSfx);
    }
    #endregion
}