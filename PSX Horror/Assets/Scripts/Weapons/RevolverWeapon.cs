using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverWeapon : WeaponBase
{
    InventoryUI inventory;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        inventory = InventoryUI.instance;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        haveLoader = CheckForSpeedLoader();
    }

    bool CheckForSpeedLoader()
    {
        foreach (SlotItemBehaviour slot in inventory.slots)
            if (slot && slot.currentItem && slot.currentItem.itemNameId == "Wp Speed Loader")
                return true;
        return false;
    }

    public override void OnFire()
    {
        Vector3 direction = transform.forward;

        Shot(damage, direction);
    }

    public override void OnReload()
    {
        int needAmmo = capacityAmmo - currentAmmo;
        if (!haveLoader)
            needAmmo = 1;

        SlotItemBehaviour[] slots = InventoryUI.instance.slots;

        for (int i = 0; i < InventoryUI.instance.slotsAvailable; i++)
        {
            if (slots[i].currentItem && slots[i].currentItem.type == ItemType.Ammo &&
                slots[i].currentItem.GetComponent<ItemAmmo>().ammoType == weaponType)
            {
                if (slots[i].currentItem.amount >= needAmmo)
                {
                    slots[i].currentItem.GetComponent<ItemAmmo>().Reload(needAmmo);
                    currentAmmo += needAmmo;
                    break;
                }
                else if (slots[i].currentItem.amount < needAmmo)
                {
                    needAmmo = slots[i].currentItem.amount;
                    slots[i].currentItem.GetComponent<ItemAmmo>().Reload(needAmmo);
                    currentAmmo += needAmmo;
                    needAmmo = capacityAmmo - currentAmmo;
                    i = -1;
                    if (CheckAmmo() == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
