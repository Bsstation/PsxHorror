using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMap : ItemBase
{
    public int mapId;


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
        MapController.instance.haveMap = true;
        if (InventoryUI.instance.maps.Length < mapId + 1)
            System.Array.Resize(ref InventoryUI.instance.maps, mapId + 1);

        InventoryUI.instance.maps[mapId] = true;
        InventoryUI.instance.mapPanel.Update();
        InventoryUI.instance.PlayAcceptAudio();

        MessagesBehaviour.instance.SendMessageTxt(MessagesBehaviour.instance.youTakeMap.msgs[Settings.instance.currentLanguage] + " ");
        MessagesBehaviour.instance.AddRich(itemName[Settings.instance.currentLanguage], "<color=lime>", "</color>");
        MessagesBehaviour.instance.AddRich(".");

        Destroy(gameObject);
        return;
    }

    public override void OnUse()
    {
        
    }
}
