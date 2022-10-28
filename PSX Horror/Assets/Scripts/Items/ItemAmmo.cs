using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAmmo : ItemBase
{
    public WeaponType ammoType;

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

    public override void OnAdd()
    {
        InventoryUI.instance.Update();
        InventoryUI.instance.UpdateSlots();
        InventoryUI.instance.itemToAdd = this;
        InventoryUI.instance.ShowItems();
        InventoryUI.instance.TakeItemMessage();
        InventoryUI.instance.AddInExaminePanel(itemNameId);
        GameManager.instance.gameStatus = GameStatus.Inventory;
        GameManager.instance.ChangeSelected(InventoryUI.instance.SearchAvailableSlot(this).gameObject);
    }

    public override void OnUse()
    {
        PlayerController player = GameManager.instance.player;

        if (player.currentWeapon && player.currentWeapon.weaponType == ammoType && player.currentWeapon.canReload && !player.reloading &&
            player.currentWeapon.CheckAmmo() > 0)
        {
            InventoryUI.instance.PlayAcceptAudio();
            player.StartReload();
        }
        else
        {
            InventoryUI.instance.PlayErrorAudio();
        }
    }

    public void Reload(int needAmmo)
    {
        amount -= needAmmo;
        if(amount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
