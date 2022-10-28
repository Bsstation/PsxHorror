using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Ammo, Recovery, Key, File, Map, Weapon, Craft
}

public abstract class ItemBase : MonoBehaviour
{
    public string itemNameId;

    public string[] itemName = new string[1];

    public ItemType type;
    public Sprite icon;
    public bool isStock;
    public int amount;
    public int amountLimit;

    public bool canDiscard;

    [Space]
    [Range(0, 1.2f)]
    public float examineDistance = 0.45f;

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        PlayerController player = GameManager.instance.player;
    }

    public void Add()
    {
        InventoryUI.instance.ClearItems();

        OnAdd();
    }

    public void Use()
    {
        OnUse();
    }

    public abstract void OnAdd();
    public abstract void OnUse();
}
