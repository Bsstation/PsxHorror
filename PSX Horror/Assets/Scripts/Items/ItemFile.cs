using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFile : ItemBase
{
    [System.Serializable]
    public class FileLang
    {
        public string fileTitle;
        [TextArea(1, 20)]
        public List<string> text;
    }

    public List<FileLang> fileLangs;
    public bool hasReaded;

    public Sprite backgroundImage;

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
        var tempItem = Resources.Load("Files/" + itemNameId) as GameObject;

        GameObject item = Instantiate(tempItem);

        InventoryUI.instance.currentFiles.files.Add(item.GetComponent<ItemFile>());
        InventoryUI.instance.RefreshFiles();
        InventoryUI.instance.PlayAcceptAudio();

        MessagesBehaviour.instance.SendMessageTxt(MessagesBehaviour.instance.youTakeMap.msgs[Settings.instance.currentLanguage] + " ");
        MessagesBehaviour.instance.AddRich(itemName[Settings.instance.currentLanguage], "<color=lime>", "</color>");
        MessagesBehaviour.instance.AddRich(".", "", "");
        item.SetActive(false);
        item.GetComponent<ItemFile>().hasReaded = false;

        //InventoryUI.instance.ShowNewFile(item.GetComponent<ItemFile>());
        Destroy(gameObject);
    }

    public override void OnUse()
    {
        
    }
}
