using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutSlot : MonoBehaviour
{
    public ItemBase currentItem;
    private Image image;
    private Text text;
    Button button;

    public bool isEmpty;

    InventoryUI inventory;
    Navigation navigation;

    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetChild(2).GetComponent<Image>();
        text = image.transform.GetChild(0).GetComponent<Text>();
        button = GetComponent<Button>();
        navigation = button.navigation;

        inventory = InventoryUI.instance;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!image || !text) return;

        isEmpty = currentItem == null ? true : false;
        image.enabled = !isEmpty;
        text.enabled = !isEmpty;

        if (currentItem != null)
        {
            image.sprite = currentItem.icon;
            image.type = Image.Type.Sliced;
            if (currentItem.isStock)
            {
                text.text = string.Format("{0}", currentItem.amount);
                text.color = (currentItem.amount == currentItem.amountLimit) ? Color.green : Color.white;
                text.enabled = true;
            }
            else
            {
                if (currentItem.type == ItemType.Weapon && currentItem.GetComponent<WeaponBase>().weaponType != WeaponType.Melee)
                {
                    int current = currentItem.GetComponent<WeaponBase>().currentAmmo;
                    int capacity = currentItem.GetComponent<WeaponBase>().capacityAmmo;

                    text.text = string.Format("{0}", current);
                    text.color = (current == capacity) ? Color.green : ((current == 0) ? Color.red : Color.white);
                    text.enabled = true;
                }
                else
                    text.enabled = false;
            }
        }

        if (inventory.itemToShortcut)
        {
            if (button && button.navigation.mode == Navigation.Mode.None)
            {
                button.navigation = navigation;
                button.interactable = true;
            }
        }
        else
        {
            if (button && button.navigation.mode == navigation.mode)
            {
                button.navigation = new Navigation() { mode = Navigation.Mode.None };
                button.interactable = false;
            }
        }
    }

    public void Interact()
    {
        if (inventory.itemToShortcut)
        {
            currentItem = inventory.itemToShortcut;
            inventory.PlayAcceptAudio();

            GameManager.instance.ChangeSelected(inventory.GetSlotSelectedItem(currentItem));

            inventory.itemToShortcut = null;

            CheckCurrentItemInOtherShotcut();
        }
    }

    void CheckCurrentItemInOtherShotcut()
    {
        if (!inventory) inventory = InventoryUI.instance;

        foreach (ShortcutSlot slot in inventory.shortcuts)
        {
            if(slot != this && slot.currentItem && slot.currentItem == currentItem)
                slot.currentItem = null;
        }
    }

    public void CheckCurrentItemExistsInInventory()
    {
        if (!inventory) inventory = InventoryUI.instance;

        foreach(SlotItemBehaviour slot in inventory.slots)
        {
            if (slot.currentItem && slot.currentItem == currentItem)
                return;
        }
        currentItem = null;
        Update();
    }
}