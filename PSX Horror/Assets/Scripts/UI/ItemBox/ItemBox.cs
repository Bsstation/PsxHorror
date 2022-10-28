using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemBox : MonoBehaviour
{
    public static ItemBox instance;

    public int slotsAvailable;
    public SlotItemBoxBehaviour[] slots;

    [Space]
    public bool moving;
    public SlotItemBoxBehaviour movingSlot;
    public BoxSlotBehaviour selectedSlot;

    public SlotItemBoxBehaviour amountSlotSelected;

    public GameObject guide;
    public Text moveInputTxt;

    [Space]
    [Header("Box Data")]
    public int itemsMax = 50;
    public Transform boxGrid;
    public GameObject boxSlotPrefab;
    public List<BoxSlotBehaviour> itemsInData;

    [Space]
    public GameObject amountPanel;
    public Text amountText;
    public bool amountActive;
    public int amountCount;

    public float currentTime;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        moveInputTxt.text = InputManager.instance.kKeys.reload.ToString();
        guide.SetActive(!amountActive);

        #region Normal
        if (GameManager.instance.gameStatus == GameStatus.ItemBox &&
           (Input.GetButtonDown("Cancel") || Input.GetKeyDown(InputManager.instance.kKeys.sprint) ||
           InputManager.instance.GetJoyButtonDown("B")))
        {
            if (selectedSlot)
            {
                InventoryUI.instance.PlayCancelAudio();
                selectedSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                GameManager.instance.ChangeSelected(selectedSlot.gameObject);
                selectedSlot = null;
            }
            else
            {
                if (moving)
                {
                    InventoryUI.instance.PlayMoveAudio();
                    moving = false;
                    movingSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                    GameManager.instance.ChangeSelected(movingSlot.gameObject);
                    movingSlot = null;
                }
                else if (amountActive)
                {
                    if (amountSlotSelected)
                    {
                        InventoryUI.instance.PlayCancelAudio();
                        GameManager.instance.ChangeSelected(amountSlotSelected.gameObject);
                        amountSlotSelected = null;
                    }
                }
                else
                {
                    InventoryUI.instance.PlayCancelAudio();
                    GameManager.instance.ChangeStateToGame();
                    UpdateSlots();
                }
            }
        }

        if (selectedSlot || moving)
        {
            foreach (Transform c in boxGrid)
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
            foreach (Transform c in boxGrid)
            {
                if (c.GetComponent<Button>() && c.GetComponent<Button>().navigation.mode == Navigation.Mode.None)
                {
                    c.GetComponent<Button>().navigation = new Navigation() { mode = Navigation.Mode.Automatic };
                    c.GetComponent<Button>().interactable = true;
                }
            }
        }
        #endregion

        #region Amount

        amountActive = amountSlotSelected != null;
        amountPanel.SetActive(amountActive);

        if (amountActive)
        {
            amountText.text = amountCount.ToString();

            Vector2 move = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.UiMovementWithMouse() : InputManager.instance.JoystickMove();

            if (move.x > 0)
                ChangeAmount(13f);
            else if(move.x < 0)
                ChangeAmount(-13f);
            else
                currentTime = amountCount;

            if (Input.GetKeyDown(InputManager.instance.kKeys.action) || InputManager.instance.GetJoyButtonDown("A") ||
                Input.GetButtonDown("Submit") || (Input.GetKeyDown(KeyCode.Mouse0) && !Settings.instance.cursorOn))
                Pass();
        }

        #endregion
    }

    bool passing;

    public void Pass()
    {
        if(!passing)
            StartCoroutine(PassItem());
    }

    IEnumerator PassItem()
    {
        passing = true;
        InventoryUI.instance.PlayAcceptAudio();
        yield return new WaitForSecondsRealtime(0.1f);
        amountSlotSelected.currentItem.amount -= amountCount;
        GameManager.instance.ChangeSelected(amountSlotSelected.gameObject);

        AddItem(amountSlotSelected.currentItem);
        passing = false;
    }

    public void StartAmount()
    {
        amountCount = 1;
        GameManager.instance.ChangeSelected(null);
    }

    public void ChangeAmount(int amount)
    {
        InventoryUI.instance.PlayMoveAudio();
        amountCount += amount;

        if (amountSlotSelected)
            amountCount = Mathf.Clamp(amountCount, 1, amountSlotSelected.currentItem.amount);
    }

    public void ChangeAmount(float amount)
    {
        currentTime += amount * Time.unscaledDeltaTime;
        amountCount = (int)currentTime;

        if (amountSlotSelected)
            amountCount = Mathf.Clamp(amountCount, 1, amountSlotSelected.currentItem.amount);
    }

    public void UpdateSlotsBox()
    {
        slotsAvailable = InventoryUI.instance.slotsAvailable;

        for (int i = 0; i < slots.Length; i++)
            slots[i].gameObject.SetActive(false);

        for (int i = 0; i < slotsAvailable; i++)
            slots[i].gameObject.SetActive(true);

        for (int i = 0; i < slotsAvailable; i++)
        {
            if (slots[i] && InventoryUI.instance.slots[i] && InventoryUI.instance.slots[i].currentItem)
            {
                slots[i].currentItem = InventoryUI.instance.slots[i].currentItem;
                slots[i].Update();
            }
        }

        GameManager.instance.ChangeSelected(slots[0].gameObject);
    }

    public void UpdateSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] && InventoryUI.instance.slots[i])
            {
                InventoryUI.instance.slots[i].currentItem = slots[i].currentItem;
                InventoryUI.instance.slots[i].Update();
                InventoryUI.instance.ClearItems();
                GameManager.instance.player.WeaponsInBody();
            }
        }
    }

    public SlotItemBoxBehaviour SearchAvailableSlot(ItemBase item)
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

    public void AddItem(ItemBase item)
    {
        if (item.isStock)
        {
            for (int i = 0; i < itemsInData.Count; i++)
            {
                if (item.isStock && itemsInData[i].currentItem.itemNameId == item.itemNameId)
                {
                    itemsInData[i].currentItem.amount += amountCount;
                    amountSlotSelected = null;
                    return;
                }
            }

            NewItem("Items/" + item.itemNameId, item);
        }
        else
            NewItem("Items/" + item.itemNameId, item);
    }

    void NewItem(string path, ItemBase item)
    {
        if (Resources.Load(path))
        {
            GameObject temp = Instantiate(boxSlotPrefab, boxGrid) as GameObject;
            var tempItem = Resources.Load(path) as GameObject;

            temp.GetComponent<BoxSlotBehaviour>().currentItem = Instantiate(tempItem).GetComponent<ItemBase>();

            if (temp.GetComponent<BoxSlotBehaviour>().currentItem.type != ItemType.Weapon)
                temp.GetComponent<BoxSlotBehaviour>().currentItem.amount = item.amount;
            else
            {
                temp.GetComponent<BoxSlotBehaviour>().currentItem.GetComponent<WeaponBase>().currentAmmo = item.GetComponent<WeaponBase>().currentAmmo;

                if (item.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                    GameManager.instance.player.EquipWeapon(GameManager.instance.player.currentWeapon);
            }

            temp.GetComponent<BoxSlotBehaviour>().currentItem.gameObject.SetActive(false);

            if (amountSlotSelected)
            {
                temp.GetComponent<BoxSlotBehaviour>().currentItem.amount = amountCount;

                amountSlotSelected = null;
            }

            temp.GetComponent<BoxSlotBehaviour>().Update();

            itemsInData.Add(temp.GetComponent<BoxSlotBehaviour>());
        }
    }

    public void SaveItems()
    {
        PlayerPrefs.SetInt("ItemsCount", itemsInData.Count);

        for(int i = 0; i < itemsInData.Count; i++)
        {
            PlayerPrefs.SetString("Slot ItemBox Temp " + i, itemsInData[i].currentItem.itemNameId);

            if (itemsInData[i].currentItem.type != ItemType.Weapon)
            {
                if (itemsInData[i].currentItem.isStock)
                    PlayerPrefs.SetInt("Slot ItemBox Amount Temp " + i, itemsInData[i].currentItem.amount);
            }
            else
                PlayerPrefs.SetInt("Slot ItemBox Amount Temp " + i, itemsInData[i].currentItem.GetComponent<WeaponBase>().currentAmmo);
        }
    }

    public void LoadItems()
    {
        int itemsCount = PlayerPrefs.GetInt("ItemsCount");

        for (int i = 0; i < itemsCount; i++)
        {
            if (PlayerPrefs.HasKey("Slot ItemBox Temp " + i))
            {
                if (Resources.Load("Items/" + PlayerPrefs.GetString("Slot ItemBox Temp " + i)))
                {
                    GameObject tempItem = Resources.Load("Items/" + PlayerPrefs.GetString("Slot ItemBox Temp " + i)) as GameObject;
                    AddItem(tempItem.GetComponent<ItemBase>());

                    if (itemsInData[i].currentItem.type != ItemType.Weapon)
                    {
                        if (itemsInData[i].currentItem.isStock)
                            itemsInData[i].currentItem.amount = PlayerPrefs.GetInt("Slot ItemBox Amount Temp " + i);
                    }
                    else
                        itemsInData[i].currentItem.GetComponent<WeaponBase>().currentAmmo = PlayerPrefs.GetInt("Slot ItemBox Amount Temp " + i);

                    itemsInData[i].currentItem.gameObject.SetActive(false);
                }
            }
        }
    }
}
