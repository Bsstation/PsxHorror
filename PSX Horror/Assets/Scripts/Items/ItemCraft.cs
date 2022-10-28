using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCraft : ItemBase
{
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
        return;
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
