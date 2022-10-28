using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public struct SceneInteractibles
{
    public string lockName;
    public bool isLocked;
    public bool isInteractible;
}

[Serializable]
public struct SceneItem
{
    public string itemName;
    public Vector3 pos;
    public Vector3 rot;
    public int amount;
}

[Serializable]
public struct SceneEnemies
{
    public string enemyName;
    public float currentLife;
}

[Serializable]
public struct SceneSaves
{
    public SceneItem[] sceneItems;
    public SceneInteractibles[] sceneInteractibles;
    public SceneEnemies[] sceneEnemies;
    public bool sceneSaved;
}

[Serializable]
public struct SceneData
{
    public SceneSaves[] scenes;
}

public class SceneController : MonoBehaviour
{
    public static SceneController instance = null;

    [Header("Scenes")]
    public Transform player;

    public int pos;
    public GameObject[] spawnPos;

    [Header("SceneData")]
    public SceneData sceneData;

    [Header("StartItems")]
    public bool passItens = false;
    public int slotsStart = 4;
    public string[] itemsStart = new string[16];
    
    private void Awake()
    {
        transform.parent = null;

        if(instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
            Destroy(gameObject);

        if (!player && GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if(spawnPos == null)
        {
            spawnPos = GameObject.FindGameObjectsWithTag("SpawnPos");
            SortArray(spawnPos);
        }

        int count = SceneManager.sceneCountInBuildSettings;
        sceneData.scenes = new SceneSaves[count];
    }

    private void Start()
    {
        StartCoroutine(ItemsStart());
    }

    IEnumerator ItemsStart()
    {
        if (passItens)
        {
            InventoryUI inventory = InventoryUI.instance;
            int length = (inventory.slotsAvailable < itemsStart.Length) ? inventory.slotsAvailable : itemsStart.Length;

            yield return new WaitForEndOfFrame();

            if (slotsStart > inventory.slotsAvailable)
                inventory.slotsAvailable = slotsStart;
            inventory.ActiveAvailableSlots();

            for (int i = 0; i < length; i++)
            {
                ItemBase temp = GetItem(i);
                if (temp != null && !inventory.slots[i].currentItem)
                {
                    inventory.slots[i].currentItem = temp;
                    inventory.SearchShortcutFree(temp);

                    if (i == 0 && inventory.slots[i].currentItem.GetComponent<WeaponBase>())
                    {
                        yield return new WaitForEndOfFrame();
                        GameManager.instance.player.EquipWeapon(temp.GetComponent<WeaponBase>());
                    }
                }
                else
                    Destroy(temp);
            }
        }
    }

    void RenameToSave()
    {
        //Interactibles
        InteractibleBase[] interactibles = Resources.FindObjectsOfTypeAll(typeof(InteractibleBase)) as InteractibleBase[];
        int x = 0;
        for(int i = 0; i < interactibles.Length; i++)
        {
            if (!IsPrefabOriginal(interactibles[i].gameObject))
            {
                interactibles[i].gameObject.name = "Interactible (" + x + ")";
                x++;
            }
        }
        //Enemies
        EnemyBase[] enemyBases = Resources.FindObjectsOfTypeAll(typeof(EnemyBase)) as EnemyBase[];
        x = 0;
        for (int i = 0; i < enemyBases.Length; i++)
        {
            GameObject tempObject = ExtensionMethods.FindParentWithTag(enemyBases[i].gameObject, "Enemy");

            if (tempObject && !IsPrefabOriginal(enemyBases[i].gameObject))
            {
                tempObject.name = "Enemy (" + x + ")";
                x++;
            }
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (instance != this) return;

        if (!GameObject.FindGameObjectWithTag("Player")) return;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player)
        {
            spawnPos = GameObject.FindGameObjectsWithTag("SpawnPos");
            SortArray(spawnPos);

            player.GetComponent<PlayerController>().SetPosition(spawnPos[pos].transform);

            LoadSceneData();
        }
    }

    void SortArray(GameObject[] array)
    {
        if (array.Length <= 0) return;

        for (int x = 0; x < array.Length; x++)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                int value1 = int.Parse(array[i].name);
                int value2 = int.Parse(array[i + 1].name);
                if (value1 > value2)
                {
                    GameObject aux = array[i];
                    array[i] = array[i + 1];
                    array[i + 1] = aux;
                }
            }
        }
    }

    public void SaveSceneData()
    {
        if (instance != this) return;

        //Renomeia para evitar confusão
        RenameToSave();

        int x = SceneManager.GetActiveScene().buildIndex;

        //Items
        ItemBase[] items = Resources.FindObjectsOfTypeAll(typeof(ItemBase)) as ItemBase[];
        sceneData.scenes[x].sceneItems = new SceneItem[items.Length];
        int count = 0;

        for (int i = 0; i < items.Length; i++)
        {
            GameObject tempObject = ExtensionMethods.FindParentWithTag(items[i].gameObject, "Player");

            if (!IsPrefabOriginal(items[i].gameObject) && items[i].gameObject.activeSelf && !tempObject)
            {
                sceneData.scenes[x].sceneItems[count].itemName = items[i].GetComponent<ItemBase>().itemNameId;
                sceneData.scenes[x].sceneItems[count].pos = items[i].transform.position;
                sceneData.scenes[x].sceneItems[count].rot = items[i].transform.eulerAngles;
                sceneData.scenes[x].sceneItems[count].amount = (items[i].GetComponent<WeaponBase>()) ? items[i].GetComponent<WeaponBase>().currentAmmo :
                     items[i].amount;
                count++;
            }
        }
        Array.Resize(ref sceneData.scenes[x].sceneItems, count);

        //Interactions
        InteractibleBase[] interactibles = Resources.FindObjectsOfTypeAll(typeof(InteractibleBase)) as InteractibleBase[];
        sceneData.scenes[x].sceneInteractibles = new SceneInteractibles[interactibles.Length];
        count = 0;

        for (int i = 0; i < interactibles.Length; i++)
        {
            if (!IsPrefabOriginal(interactibles[i].gameObject))
            {
                sceneData.scenes[x].sceneInteractibles[i].lockName = interactibles[i].name;
                if (interactibles[i])
                {
                    sceneData.scenes[x].sceneInteractibles[count].isInteractible = interactibles[i].GetComponent<InteractibleBase>().canInteract;
                    sceneData.scenes[x].sceneInteractibles[count].isLocked = interactibles[i].GetComponent<InteractibleBase>().locked;
                }
                count++;
            }
        }
        Array.Resize(ref sceneData.scenes[x].sceneInteractibles, count);

        //Enemies
        EnemyBase[] enemyBases = Resources.FindObjectsOfTypeAll(typeof(EnemyBase)) as EnemyBase[];
        sceneData.scenes[x].sceneEnemies = new SceneEnemies[enemyBases.Length];
        count = 0;

        for (int i = 0; i < enemyBases.Length; i++)
        {
            GameObject tempObject = ExtensionMethods.FindParentWithTag(enemyBases[i].gameObject, "Enemy");

            if (tempObject && !IsPrefabOriginal(tempObject.gameObject) && !enemyBases[i].dead && tempObject.activeSelf)
            {
                sceneData.scenes[x].sceneEnemies[count].enemyName = tempObject.name;
                sceneData.scenes[x].sceneEnemies[count].currentLife = enemyBases[i].currentLife;
                count++;
            }
        }
        Array.Resize(ref sceneData.scenes[x].sceneEnemies, count);

        //Saved
        sceneData.scenes[x].sceneSaved = true;
    }

    void LoadSceneData()
    {
        int x = SceneManager.GetActiveScene().buildIndex;
        //Renomeia para não causar confusão
        RenameToSave();

        if (!sceneData.scenes[x].sceneSaved) return;

        //Items
        ItemBase[] items = Resources.FindObjectsOfTypeAll(typeof(ItemBase)) as ItemBase[];

        for (int i = 0; i < items.Length; i++)
        {
            GameObject tempObject = ExtensionMethods.FindParentWithTag(items[i].gameObject, "Player");

            if (!IsPrefabOriginal(items[i].gameObject) && !tempObject && items[i].gameObject.activeSelf)
                Destroy(items[i].gameObject);
        }

        Transform itemBag = new GameObject().transform;

        for (int i = 0; i < sceneData.scenes[x].sceneItems.Length; i++)
        {
            GameObject temp = null;
            if (Resources.Load("Items/" + sceneData.scenes[x].sceneItems[i].itemName))
                temp = Resources.Load("Items/" + sceneData.scenes[x].sceneItems[i].itemName) as GameObject;

            if (Resources.Load("Files/" + sceneData.scenes[x].sceneItems[i].itemName))
                temp = Resources.Load("Files/" + sceneData.scenes[x].sceneItems[i].itemName) as GameObject;
            if (Resources.Load("Maps/" + sceneData.scenes[x].sceneItems[i].itemName))
                temp = Resources.Load("Maps/" + sceneData.scenes[x].sceneItems[i].itemName) as GameObject;

            if (!temp) return;

            GameObject newItem = Instantiate(temp, sceneData.scenes[x].sceneItems[i].pos, Quaternion.Euler(sceneData.scenes[x].sceneItems[i].rot)) as GameObject;
            if (newItem.GetComponent<ItemAmmo>())
                newItem.GetComponent<ItemAmmo>().amount = sceneData.scenes[x].sceneItems[i].amount;

            if (newItem.GetComponent<WeaponBase>())
                newItem.GetComponent<WeaponBase>().currentAmmo = sceneData.scenes[x].sceneItems[i].amount;

            newItem.transform.parent = itemBag;
        }

        //interactions
        InteractibleBase[] interactibles = Resources.FindObjectsOfTypeAll(typeof(InteractibleBase)) as InteractibleBase[];

        for (int i = 0; i < sceneData.scenes[x].sceneInteractibles.Length; i++)
        {
            InteractibleBase interactible = ReturnInArray(sceneData.scenes[x].sceneInteractibles[i].lockName, interactibles);

            if (interactible)
            {
                interactible.locked = sceneData.scenes[x].sceneInteractibles[i].isLocked;
                interactible.canInteract = sceneData.scenes[x].sceneInteractibles[i].isInteractible;
            }
        }

        //Enemies
        EnemyBase[] enemies = Resources.FindObjectsOfTypeAll(typeof(EnemyBase)) as EnemyBase[];
        List<GameObject> tempEnemies = new List<GameObject>();

        foreach(EnemyBase e in enemies)
        {
            if (!IsPrefabOriginal(e.transform.parent.gameObject))
            {
                GameObject tempObject = ExtensionMethods.FindParentWithTag(e.gameObject, "Enemy");
                if (tempObject && !tempEnemies.Contains(tempObject))
                    tempEnemies.Add(tempObject);
            }
        }

        if (tempEnemies.Count > 0)
        {
            if (sceneData.scenes[x].sceneEnemies.Length > 0)
            {
                foreach (GameObject en in tempEnemies)
                {
                    for (int i = 0; i < sceneData.scenes[x].sceneEnemies.Length; i++)
                    {
                        if (en.name == sceneData.scenes[x].sceneEnemies[i].enemyName)
                        {
                            en.GetComponentInChildren<EnemyBase>().currentLife = sceneData.scenes[x].sceneEnemies[i].currentLife;
                            break;
                        }

                        if (i == sceneData.scenes[x].sceneEnemies.Length - 1)
                        {
                            en.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject en in tempEnemies)
                    Destroy(en.gameObject);
            }
        }
    }

    bool IsPrefabOriginal(GameObject go)
    {
        return !go.scene.IsValid();
    }

    InteractibleBase ReturnInArray(string name, InteractibleBase[] array)
    {
        for(int i = 0; i < array.Length; i++)
        {
            if(!IsPrefabOriginal(array[i].gameObject) && array[i].gameObject.name == name)
                return array[i];
        }
        return null;
    }

    public ItemBase GetItem(int itemIndex)
    {
        if (Resources.Load("Items/" + itemsStart[itemIndex]))
        {
            GameObject temp = Resources.Load("Items/" + itemsStart[itemIndex]) as GameObject;
            GameObject weapon = Instantiate(temp) as GameObject;
            if (weapon.GetComponent<WeaponBase>() && weapon.GetComponent<WeaponBase>().weaponType != WeaponType.Melee)
                weapon.GetComponent<WeaponBase>().currentAmmo = 0;
            itemsStart[itemIndex] = "";

            weapon.layer = 8;
            Transform[] childs = weapon.GetComponentsInChildren<Transform>() as Transform[];
            foreach (Transform child in childs)
                child.gameObject.layer = 8;

            weapon.SetActive(false);
            return weapon.GetComponent<ItemBase>();
        }
        return null;
    }
}