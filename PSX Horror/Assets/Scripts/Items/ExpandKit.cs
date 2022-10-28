using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandKit : ItemBase 
{
    public override void OnAdd()
    {
        InventoryUI inventory = InventoryUI.instance;

        inventory.upgrading = true;
        inventory.Update();
        inventory.UpdateSlots();
        inventory.itemToAdd = this;
        inventory.ShowItems();
        inventory.AddInExaminePanel(itemNameId);

        GameManager.instance.gameStatus = GameStatus.Inventory;

        inventory.itemsScroll.SnapTo(inventory.slots[inventory.slotsAvailable - 1].gameObject);

        StartCoroutine(UpgradeInventory());
    }

    IEnumerator UpgradeInventory()
    {
        InventoryUI inventory = InventoryUI.instance;

        inventory.slotsAvailable += 2;
        yield return new WaitForSecondsRealtime(2);
        inventory.ActiveAvailableSlots();

        yield return new WaitForEndOfFrame();
        inventory.itemsScroll.SnapTo(inventory.slots[inventory.slotsAvailable - 1].gameObject);

        inventory.PlayAcceptAudio();
        yield return new WaitForSecondsRealtime(1);

        inventory.PlayCancelAudio();
        GameManager.instance.gameStatus = GameStatus.Game;
        inventory.upgrading = false;
        inventory.ClearExamineItem();
        Destroy(gameObject);
    }

    public override void OnUse()
    {
        
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
