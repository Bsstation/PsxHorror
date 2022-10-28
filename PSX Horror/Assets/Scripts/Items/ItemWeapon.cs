using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : ItemBase
{
    public override void OnAdd()
    {
        InventoryUI inventory = InventoryUI.instance;
        GameManager gameManager = GameManager.instance;

        inventory.Update();
        inventory.UpdateSlots();
        inventory.itemToAdd = this;
        inventory.ShowItems();
        inventory.TakeItemMessage();
        inventory.AddInExaminePanel(itemNameId);

        gameManager.gameStatus = GameStatus.Inventory;
        gameManager.ChangeSelected(InventoryUI.instance.SearchAvailableSlot(this).gameObject);
    }

    public override void OnUse()
    {
        PlayerController player = GameManager.instance.player;

        InventoryUI.instance.PlayEquipAudio();

        WeaponBase wp = GetComponent<WeaponBase>();

        player.ChangeWeapon(wp);

        player.WeaponsInBody();
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
