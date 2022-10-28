using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Shotgun Values")]
    public float projectils;
    public float spreadFactor;

    public override void OnFire()
    {
        for (int i = 0; i < projectils; i++)
        {
            Vector3 direction = transform.forward;

            Vector3 finalDirection = direction + new Vector3(Random.Range(-spreadFactor, spreadFactor), Random.Range(-spreadFactor, spreadFactor), Random.Range(-spreadFactor, spreadFactor));
            PiercingShot(damage / projectils, finalDirection);
        }
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

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}
