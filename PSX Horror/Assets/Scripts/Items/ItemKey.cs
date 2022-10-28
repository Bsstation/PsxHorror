using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemKey : ItemBase
{
    public int keyIndex;

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
        if (player.interaction && keyIndex == player.interaction.keyIndex)
        {
            player.UnlockInteraction(itemName[Settings.instance.currentLanguage]);
            Destroy(gameObject);
            return;
        }
        InventoryUI.instance.PlayErrorAudio();
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
