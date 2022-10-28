using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SlotItemBehaviour : MonoBehaviour, IScrollHandler
{
    public ItemBase currentItem;
    private Image image;
    private Text text;
    private GameObject equiped;

    public bool isEmpty;

    // Start is called before the first frame update
    public void Start()
    {
        scroll = GetComponentInParent<ScrollUpdateY>();

        image = transform.GetChild(2).GetComponent<Image>();
        text = image.transform.GetChild(0).GetComponent<Text>();
        equiped = transform.GetChild(3).gameObject;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!image || !text) return;

        isEmpty = currentItem == null ? true : false;
        image.enabled = !isEmpty;
        text.enabled = !isEmpty;
        equiped.SetActive(currentItem && currentItem.GetComponent<WeaponBase>() && 
            currentItem.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon);

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
                EventSystem.current.currentSelectedGameObject == gameObject && !InventoryUI.instance.moving && !InventoryUI.instance.selectedSlot)
            {
                InventoryUI.instance.PlayMoveAudio();
                InventoryUI.instance.moving = true;
                InventoryUI.instance.movingSlot = this;
                GetComponent<Image>().color = InventoryUI.instance.colors.movingColor;
            }
        }
    }

    public void Interact()
    {
        InventoryUI inventory = InventoryUI.instance;

        if (inventory.moving)
        {
            InventoryUI.instance.PlayAcceptAudio();
            if (!currentItem)
            {
                currentItem = inventory.movingSlot.currentItem;
                inventory.movingSlot.currentItem = null;
            }
            else if (inventory.movingSlot != this)
            {
                ItemBase originItem = currentItem;
                ItemBase newItem = inventory.movingSlot.currentItem;

                if (newItem.itemNameId == originItem.itemNameId && originItem.isStock)
                {
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
                    if (CraftManager.instance.CheckRecipe(originItem, newItem))
                    {
                        GameObject result = Instantiate(CraftManager.instance.CheckRecipe(originItem, newItem));
                        result.gameObject.SetActive(false);

                        Destroy(currentItem.gameObject);
                        Destroy(inventory.movingSlot.currentItem.gameObject);

                        currentItem = result.GetComponent<ItemBase>();
                        inventory.SearchShortcutFree(currentItem);

                        //Checa se o resultado é uma arma e se está equipada
                        if (originItem.type == ItemType.Weapon)
                        {
                            currentItem.GetComponent<WeaponBase>().currentAmmo = originItem.GetComponent<WeaponBase>().currentAmmo;
                            if(originItem.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                                currentItem.Use();
                        }
                        else if (newItem.type == ItemType.Weapon)
                        {
                            currentItem.GetComponent<WeaponBase>().currentAmmo = newItem.GetComponent<WeaponBase>().currentAmmo;
                            if (newItem.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                                currentItem.Use();
                        }

                        currentItem.gameObject.layer = 8;
                        Transform[] childs = currentItem.GetComponentsInChildren<Transform>() as Transform[];

                        foreach (Transform child in childs)
                            child.gameObject.layer = 8;

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
            if (inventory.itemToAdd)
            {
                if (currentItem == null)
                {
                    inventory.PlayAcceptAudio();
                    currentItem = inventory.itemToAdd;

                    currentItem.gameObject.layer = 8;
                    Transform[] childs = currentItem.GetComponentsInChildren<Transform>() as Transform[];

                    foreach (Transform child in childs)
                        child.gameObject.layer = 8;

                    currentItem.gameObject.SetActive(false);
                    currentItem.GetComponent<Collider>().enabled = false;

                    inventory.SearchShortcutFree(currentItem);

                    GameManager.instance.player.currentItem = null;
                    inventory.ClearItems();
                    GameManager.instance.ChangeSelected(null);
                    GameManager.instance.ChangeStateToGame();
                }
                else if (currentItem != null)
                {
                    if (inventory.itemToAdd.itemNameId == currentItem.itemNameId && currentItem.isStock)
                    {
                        if (inventory.itemToAdd.amount + currentItem.amount <= currentItem.amountLimit)
                        {
                            inventory.PlayAcceptAudio();
                            currentItem.amount += inventory.itemToAdd.amount;
                            Destroy(inventory.itemToAdd.gameObject);
                            inventory.ClearItems();
                            GameManager.instance.ChangeSelected(null);
                            GameManager.instance.ChangeStateToGame();
                        }
                        else
                        {
                            currentItem.amount += inventory.itemToAdd.amount;
                            inventory.itemToAdd.amount = currentItem.amount - currentItem.amountLimit;
                            currentItem.amount = currentItem.amountLimit;
                            inventory.PlayAcceptAudio();
                        }
                    }
                    else
                    {
                        if (CraftManager.instance.CheckRecipe(inventory.itemToAdd, currentItem))
                        {
                            GameObject newItem = Instantiate(CraftManager.instance.CheckRecipe(inventory.itemToAdd, currentItem));
                            ItemBase oldItem = currentItem;
                            inventory.PlayAcceptAudio();
                            newItem.gameObject.SetActive(false);
                            currentItem = newItem.GetComponent<ItemBase>();

                            if (inventory.itemToAdd.type == ItemType.Weapon)
                                currentItem.GetComponent<WeaponBase>().currentAmmo = inventory.itemToAdd.GetComponent<WeaponBase>().currentAmmo;
                            else if (oldItem.type == ItemType.Weapon)
                                currentItem.GetComponent<WeaponBase>().currentAmmo = oldItem.GetComponent<WeaponBase>().currentAmmo;

                            if (inventory.itemToAdd.type == ItemType.Weapon)
                            {
                                currentItem.GetComponent<WeaponBase>().currentAmmo = inventory.itemToAdd.GetComponent<WeaponBase>().currentAmmo;
                                if (inventory.itemToAdd.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                                    currentItem.Use();
                            }
                            else if (oldItem.type == ItemType.Weapon)
                            {
                                currentItem.GetComponent<WeaponBase>().currentAmmo = oldItem.GetComponent<WeaponBase>().currentAmmo;
                                if (oldItem.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                                    currentItem.Use();
                            }

                            Destroy(oldItem.gameObject);

                            currentItem.gameObject.layer = 8;
                            Transform[] childs = currentItem.GetComponentsInChildren<Transform>() as Transform[];

                            foreach (Transform child in childs)
                                child.gameObject.layer = 8;

                            Destroy(inventory.itemToAdd.gameObject);
                            inventory.SearchShortcutFree(currentItem);
                            inventory.ClearItems();
                            GameManager.instance.ChangeSelected(null);
                            GameManager.instance.ChangeStateToGame();
                        }
                        else
                            inventory.CannotPut();
                    }
                }
                GameManager.instance.player.WeaponsInBody();
            }
            else
            {
                if (!inventory.selectedSlot)
                {
                    if (currentItem != null)
                        inventory.SelectItem(currentItem);
                    else
                        inventory.ClearItems();
                }
                else
                {
                    if (currentItem)
                    {
                        if (inventory.selectedSlot != this && inventory.selectedSlot.currentItem.itemNameId == currentItem.itemNameId && currentItem.isStock)
                        {
                            if (inventory.selectedSlot.currentItem.amount + currentItem.amount <= currentItem.amountLimit)
                            {
                                inventory.PlayAcceptAudio();
                                currentItem.amount += inventory.selectedSlot.currentItem.amount;
                                Destroy(inventory.selectedSlot.currentItem.gameObject);
                                inventory.selectedSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                                inventory.ClearItems();
                                GameManager.instance.ChangeSelected(gameObject);
                                inventory.UpdateSlots();
                            }
                            else
                            {
                                currentItem.amount += inventory.selectedSlot.currentItem.amount;
                                inventory.selectedSlot.currentItem.amount = currentItem.amount - currentItem.amountLimit;
                                currentItem.amount = currentItem.amountLimit;
                                inventory.PlayAcceptAudio();
                            }
                        }
                        else
                        {
                            if (CraftManager.instance.CheckRecipe(inventory.selectedSlot.currentItem, currentItem))
                            {
                                GameObject newItem = Instantiate(CraftManager.instance.CheckRecipe(inventory.selectedSlot.currentItem, currentItem));
                                inventory.PlayAcceptAudio();
                                newItem.gameObject.SetActive(false);

                                if (inventory.selectedSlot.currentItem.type == ItemType.Weapon)
                                    newItem.GetComponent<WeaponBase>().currentAmmo = inventory.selectedSlot.currentItem.GetComponent<WeaponBase>().currentAmmo;
                                else if (currentItem.type == ItemType.Weapon)
                                    newItem.GetComponent<WeaponBase>().currentAmmo = currentItem.GetComponent<WeaponBase>().currentAmmo;

                                //Checa se o resultado é uma arma e se está equipada
                                if (inventory.selectedSlot.currentItem.type == ItemType.Weapon)
                                {
                                    newItem.GetComponent<WeaponBase>().currentAmmo = inventory.selectedSlot.currentItem.GetComponent<WeaponBase>().currentAmmo;
                                    if (inventory.selectedSlot.currentItem.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                                        newItem.GetComponent<ItemBase>().Use();
                                }
                                else if (currentItem.type == ItemType.Weapon)
                                {
                                    newItem.GetComponent<WeaponBase>().currentAmmo = currentItem.GetComponent<WeaponBase>().currentAmmo;
                                    if (currentItem.GetComponent<WeaponBase>() == GameManager.instance.player.currentWeapon)
                                        newItem.GetComponent<ItemBase>().Use();
                                }

                                Destroy(currentItem.gameObject);
                                currentItem = newItem.GetComponent<ItemBase>();
                                inventory.SearchShortcutFree(currentItem);

                                Destroy(inventory.selectedSlot.currentItem.gameObject);
                                inventory.selectedSlot.GetComponent<Image>().color = InventoryUI.instance.colors.normalColor;
                                inventory.ClearItems();
                                GameManager.instance.ChangeSelected(gameObject);
                                inventory.UpdateSlots();
                            }
                            else
                                inventory.CannotMix();
                        }
                    }
                    else
                        inventory.CannotMix();
                }
                GameManager.instance.player.WeaponsInBody();
            }
        }
    }
    ScrollUpdateY scroll;
    public void OnScroll(PointerEventData eventData)
    {
        scroll.MoveScroll();
    }
}