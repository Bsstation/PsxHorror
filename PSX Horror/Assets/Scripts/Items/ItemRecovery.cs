using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRecovery : ItemBase
{
    [Range(0,1)]
    public float recovery;
    public bool stopBleeding;

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
        float lifeToRecover = player.maxLife * recovery;
        InventoryUI.instance.PlayRecoveryAudio();

        player.Recovery(lifeToRecover);

        if (stopBleeding)
            player.bleeding = false;

        Destroy(gameObject);
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
