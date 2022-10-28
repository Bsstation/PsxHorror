using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotItemBoxBehaviour : MonoBehaviour
{
    public ItemBase currentItem;
    private Image image;
    private Text text;

    public bool isEmpty;

    // Start is called before the first frame update
    public void Start()
    {
        image = transform.GetChild(2).GetComponent<Image>();
        text = image.transform.GetChild(0).GetComponent<Text>();
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

            if (currentItem.isStock && currentItem.amount <= 0)
            {
                Destroy(currentItem.gameObject);
                currentItem = null;
            }

            if ((Input.GetKeyDown(InputManager.instance.kKeys.reload) || InputManager.instance.GetJoyButtonDown("X")) &&
                EventSystem.current.currentSelectedGameObject == gameObject && !ItemBox.instance.moving && !ItemBox.instance.selectedSlot)
            {
                InventoryUI.instance.PlayMoveAudio();
                ItemBox.instance.moving = true;
                ItemBox.instance.movingSlot = this;
                GetComponent<Image>().color = InventoryUI.instance.colors.movingColor;
            }
        }
    }

    public void Interact()
    {
        StartCoroutine(Itct());
    }

    IEnumerator Itct()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        ItemBox inventory = ItemBox.instance;

        if (inventory.selectedSlot)
        {
            BoxSlotBehaviour boxSlot = inventory.selectedSlot;

            if (!currentItem)
            {
                InventoryUI.instance.PlayAcceptAudio();
                if (boxSlot.currentItem.isStock && boxSlot.currentItem.amount > boxSlot.currentItem.amountLimit)
                {
                    boxSlot.currentItem.amount -= boxSlot.currentItem.amountLimit;
                    currentItem = Instantiate(boxSlot.currentItem);
                    currentItem.amount = boxSlot.currentItem.amountLimit;
                    inventory.selectedSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                    inventory.selectedSlot = null;
                }
                else
                {
                    currentItem = boxSlot.currentItem;
                    inventory.itemsInData.Remove(boxSlot);
                    Destroy(boxSlot.gameObject);
                    inventory.selectedSlot = null;
                }
            }
            else
            {
                if (boxSlot.currentItem.isStock && boxSlot.currentItem.itemNameId == currentItem.itemNameId && currentItem.amount < currentItem.amountLimit)
                {
                    InventoryUI.instance.PlayAcceptAudio();
                    if (currentItem.amount + boxSlot.currentItem.amount > currentItem.amountLimit)
                    {
                        boxSlot.currentItem.amount = currentItem.amount - currentItem.amountLimit;
                        currentItem.amount = currentItem.amountLimit;
                        inventory.selectedSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                        inventory.selectedSlot = null;
                    }
                    else
                    {
                        currentItem.amount += boxSlot.currentItem.amount;
                        inventory.itemsInData.Remove(boxSlot);
                        Destroy(boxSlot.gameObject);
                        inventory.selectedSlot = null;
                    }
                }
                else
                    InventoryUI.instance.PlayErrorAudio();
            }
        }
        else
        {
            if (inventory.moving)
            {
                if (!currentItem)
                {
                    InventoryUI.instance.PlayAcceptAudio();
                    currentItem = inventory.movingSlot.currentItem;
                    inventory.movingSlot.currentItem = null;
                }
                else if (inventory.movingSlot != this)
                {
                    ItemBase originItem = currentItem;
                    ItemBase newItem = inventory.movingSlot.currentItem;

                    if (newItem.itemNameId == originItem.itemNameId && originItem.isStock)
                    {
                        InventoryUI.instance.PlayAcceptAudio();
                        if (newItem.amount + originItem.amount <= originItem.amountLimit)
                        {
                            originItem.amount += newItem.amount;
                            inventory.movingSlot.currentItem = null;
                            Destroy(newItem.gameObject);
                        }
                        else
                        {
                            originItem.amount += newItem.amount;
                            newItem.amount = originItem.amount - originItem.amountLimit;
                            originItem.amount = originItem.amountLimit;
                        }
                    }
                    else
                    {
                        InventoryUI.instance.PlayAcceptAudio();
                        if (CraftManager.instance.CheckRecipe(originItem, newItem))
                        {
                            GameObject result = Instantiate(CraftManager.instance.CheckRecipe(originItem, newItem));
                            result.gameObject.SetActive(false);

                            Destroy(currentItem.gameObject);
                            Destroy(inventory.movingSlot.currentItem.gameObject);

                            currentItem = result.GetComponent<ItemBase>();

                            if (originItem.type == ItemType.Weapon)
                                currentItem.GetComponent<WeaponBase>().currentAmmo = originItem.GetComponent<WeaponBase>().currentAmmo;
                            else if (newItem.type == ItemType.Weapon)
                                currentItem.GetComponent<WeaponBase>().currentAmmo = newItem.GetComponent<WeaponBase>().currentAmmo;

                            Destroy(newItem.gameObject);
                            Destroy(originItem.gameObject);
                        }
                        else
                        {
                            currentItem = newItem;
                            inventory.movingSlot.currentItem = originItem;
                        }
                    }
                }
                inventory.moving = false;
                inventory.movingSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                inventory.movingSlot = null;
            }
            else
            {
                if (currentItem)
                {
                    InventoryUI.instance.PlayAcceptAudio();
                    if (!currentItem.isStock)
                    {
                        inventory.AddItem(currentItem);
                        currentItem = null;
                    }
                    else
                    {
                        inventory.amountSlotSelected = this;
                        inventory.StartAmount();
                    }
                }
            }
        }
    }
}