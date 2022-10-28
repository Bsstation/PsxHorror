using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public enum SaveMode { Save, Load }

public class SaveSlotBehaviour : MonoBehaviour
{
    public int id;
    private string filePath;
    public bool isEmpty;

    public SaveMode mode;

    [Space]
    RandomImage randomImage;
    public Text emptySlotText;
    public Text info;
    public GameObject noise;

    private void Awake()
    {
        randomImage = GetComponent<RandomImage>();

        filePath = Application.dataPath + "/Saves/Save Game " + id + ".dat";

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(Application.dataPath + "/Saves");

        UpdateSlot();
    }

    void UpdateSlot()
    {
        if (!File.Exists(filePath))
        {
            info.enabled = false;
            isEmpty = true;
            randomImage.enabled = true;
            emptySlotText.enabled = true;
            noise.SetActive(false);
        }
        else
        {
            info.enabled = true;
            isEmpty = false;
            randomImage.enabled = false;
            emptySlotText.enabled = false;
            noise.SetActive(true);

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);

            PlayerData data = (PlayerData)bf.Deserialize(file);

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(data.screenshot);

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            GetComponent<Image>().sprite = Sprite.Create(texture, rect, Vector2.zero);

            Difficulty difficulty = (Difficulty)data.difficulty;

            info.text = data.sceneName[Settings.instance.currentLanguage] + Environment.NewLine +
                difficulty.ToString() + Environment.NewLine + data.playTimeString + Environment.NewLine + "Saves:"  + data.gamesSaved;

            file.Close();
        }
    }

    public void Interact()
    {
        if (mode == SaveMode.Save)
            Save();
        else if (mode == SaveMode.Load)
            Load();
    }

    void Save()
    {
        //Inicio do save////////////////////////////////////////////////////////////////////////
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filePath);

        PlayerData data = new PlayerData();

        //carregando valores//////////////////////////////////////////////////////////////////

        PlayerController player = GameManager.instance.player;
        InventoryUI inventory = InventoryUI.instance;
        ItemBox itemBox = ItemBox.instance;
        SceneController sC = SceneController.instance;

        PlayerStates.instance.saved++;

        //scene
        sC.SaveSceneData();
        data.sceneIndex = SceneManager.GetActiveScene().buildIndex;
        data.spawnPos = 1;

        data.playTime = PlayerStates.instance.timer;

        int hours = (int)(data.playTime / 3600) % 24;
        int minutes = (int)(data.playTime / 60) % 60;
        int seconds = (int)(data.playTime % 60);

        data.playTimeString = string.Format("{0:0}:{1:00}:{2:00}", hours, minutes, seconds);
        data.gamesSaved = PlayerStates.instance.saved;
        data.difficulty = (int)PlayerStates.instance.difficulty;

        #region Player
        data.currentLife = player.currentLife;
        data.flashlightOn = player.flashlightEnable;

        data.haveWeapon = (player.currentWeapon);
        if (player.currentWeapon)
        {
            for (int i = 0; i < InventoryUI.instance.slotsAvailable; i++)
            {
                if (InventoryUI.instance.slots[i] && InventoryUI.instance.slots[i].currentItem &&
                    InventoryUI.instance.slots[i].currentItem.GetComponent<WeaponBase>())
                {
                    if (InventoryUI.instance.slots[i].currentItem.GetComponent<WeaponBase>() == player.currentWeapon)
                    {
                        data.currentWeaponSlot = i;
                        break;
                    }
                }
            }
        }
        #endregion

        #region Inventory
        data.slotsAvailable = inventory.slotsAvailable;
        for (int i = 0; i < inventory.slotsAvailable; i++)
        {
            if (inventory.slots[i])
            {
                if (inventory.slots[i].currentItem)
                {
                    data.slots[i].haveItem = true;
                    data.slots[i].itemName = inventory.slots[i].currentItem.itemNameId;
                    if(inventory.slots[i].currentItem.isStock)
                    {
                        data.slots[i].amount = inventory.slots[i].currentItem.amount;
                    }
                    else if(inventory.slots[i].currentItem.type == ItemType.Weapon)
                    {
                        data.slots[i].amount = inventory.slots[i].currentItem.GetComponent<WeaponBase>().currentAmmo;
                    }
                    else
                    {
                        data.slots[i].amount = -1;
                    }
                }
                else
                {
                    data.slots[i].haveItem = false;
                }
            }
        }

        for(int i = 0; i < inventory.shortcuts.Length; i++)
        {
            if (inventory.shortcuts[i] && inventory.shortcuts[i].currentItem)
            {
                data.shortcuts[i] = inventory.GetSlotNumber(inventory.shortcuts[i].currentItem);
            }
            else
                data.shortcuts[i] = -1;
        }

        data.filesCount = inventory.currentFiles.files.Count;
        data.files = new PlayerData.FileData[data.filesCount];

        for(int i = 0; i < data.filesCount; i++)
        {
            if (inventory.currentFiles.files[i])
            {
                data.files[i].files = inventory.currentFiles.files[i].itemNameId;
                data.files[i].hasReaded = inventory.currentFiles.files[i].hasReaded;
            }
        }

        data.mapsCount = inventory.maps.Length;
        data.maps = new bool[inventory.maps.Length];
        for(int i = 0; i < inventory.maps.Length; i++)
        {
            if (inventory.maps[i])
                data.maps[i] = true;
            else
                data.maps[i] = false;
        }

        #endregion

        #region ItemBox

        data.itemBoxCount = itemBox.itemsInData.Count;
        data.boxSlots = new PlayerData.SlotBoxData[itemBox.itemsInData.Count];

        for (int i = 0; i < itemBox.itemsInData.Count; i++)
        {
            if (itemBox.itemsInData[i].currentItem)
            {
                data.boxSlots[i].itemName = itemBox.itemsInData[i].currentItem.itemNameId;

                if (itemBox.itemsInData[i].currentItem.isStock)
                {
                    data.boxSlots[i].amount = itemBox.itemsInData[i].currentItem.amount;
                }
                else if (itemBox.itemsInData[i].currentItem.type == ItemType.Weapon)
                {
                    data.boxSlots[i].amount = itemBox.itemsInData[i].currentItem.GetComponent<WeaponBase>().currentAmmo;
                }
                else
                {
                    data.boxSlots[i].amount = 0;
                }
            }
        }

        #endregion

        #region SceneController

        data.sceneSave = new PlayerData.SceneSave[SceneManager.sceneCountInBuildSettings];

        for(int i = 0; i < data.sceneSave.Length; i++)
        {
            //items
            if (sC.sceneData.scenes[i].sceneItems != null)
            {
                int count = sC.sceneData.scenes[i].sceneItems.Length;
                data.sceneSave[i].sceneItems = new PlayerData.ItemScene[count];

                for (int x = 0; x < count; x++)
                {
                    if (sC.sceneData.scenes[i].sceneItems[x].itemName != "")
                    {
                        data.sceneSave[i].sceneItems[x].itemName = sC.sceneData.scenes[i].sceneItems[x].itemName;
                        data.sceneSave[i].sceneItems[x].amount = sC.sceneData.scenes[i].sceneItems[x].amount;

                        data.sceneSave[i].sceneItems[x].pos.x = sC.sceneData.scenes[i].sceneItems[x].pos.x;
                        data.sceneSave[i].sceneItems[x].pos.y = sC.sceneData.scenes[i].sceneItems[x].pos.y;
                        data.sceneSave[i].sceneItems[x].pos.z = sC.sceneData.scenes[i].sceneItems[x].pos.z;

                        data.sceneSave[i].sceneItems[x].rot.x = sC.sceneData.scenes[i].sceneItems[x].rot.x;
                        data.sceneSave[i].sceneItems[x].rot.y = sC.sceneData.scenes[i].sceneItems[x].rot.y;
                        data.sceneSave[i].sceneItems[x].rot.z = sC.sceneData.scenes[i].sceneItems[x].rot.z;

                    }
                }
            }

            //interactibles
            data.sceneSave[i].sceneInteractibles = sC.sceneData.scenes[i].sceneInteractibles;
            //enemies
            data.sceneSave[i].sceneEnemies = sC.sceneData.scenes[i].sceneEnemies;
            //finish
            data.sceneSave[i].sceneSaved = sC.sceneData.scenes[i].sceneSaved;
        }

        #endregion

        #region ScreenShot

        string shotPath = Application.persistentDataPath + "/Saves/Shot " + id + ".dat";

        byte[] BitArray = ScreenShotBehaviour.instance.sprite.texture.EncodeToPNG();
        data.screenshot = BitArray;
        data.sceneName = ScreenShotBehaviour.instance.sceneName;

        #endregion

        #region Maps Settings

        MapController.instance.SaveDoors();

        MapData[] indicators = FindObjectsOfType<MapData>();

        data.mapIdCount = indicators.Length;
        data.mapDatas = new PlayerData.MapsInd[indicators.Length];

        for (int i = 0; i < indicators.Length; i++)
        {
            data.mapDatas[i].mapId = indicators[i].mapId;
            data.mapDatas[i].doorStates = new int[indicators[i].doors.Length];

            for (int x = 0; x < indicators[i].doors.Length; x++)
            {
                data.mapDatas[i].doorStates[x] = (int)indicators[i].doors[x].doorState;
            }
        }

        #endregion
        //Final do save//////////////////////////////////////////////////////////////////
        bf.Serialize(file, data);

        file.Close();

        UpdateSlot();
    }

    void Load()
    {
        //se o arquivo existe
        if (File.Exists(filePath))
        {
            //inicio do load////////////////////////////////////////////////////////////////////
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);

            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            //carregando valores/////////////////////////////////////////////////////////////////

            //scene
            GameObject obj = new GameObject();
            SceneController sceneController = obj.AddComponent<SceneController>();
            sceneController.pos = data.spawnPos;

            PlayerPrefs.SetFloat("Timer", data.playTime);
            PlayerPrefs.SetInt("Saved", data.gamesSaved);
            PlayerPrefs.SetInt("Difficulty", data.difficulty);

            #region Player
            PlayerPrefs.SetFloat("Player Life", data.currentLife);
            int value = (data.flashlightOn) ? 1 : 0;
            PlayerPrefs.SetInt("Flash On", value);

            if (data.haveWeapon)
            {
                PlayerPrefs.SetInt("Current Weapon", data.currentWeaponSlot);
            }
            else
            {
                PlayerPrefs.DeleteKey("Current Weapon");
            }

            #endregion

            #region Inventory
            PlayerPrefs.SetInt("SlotsAvailable", data.slotsAvailable);

            for (int i = 0; i < data.slotsAvailable; i++)
            {
                if (data.slots[i].haveItem)
                {
                    PlayerPrefs.SetString("Slot Item Temp " + i, data.slots[i].itemName);

                    if (data.slots[i].amount >= 0)
                        PlayerPrefs.SetInt("Slot Amount Temp " + i, data.slots[i].amount);
                    else
                        PlayerPrefs.SetInt("Slot Amount Temp " + i, 1);
                }
                else
                {
                    PlayerPrefs.DeleteKey("Slot Item Temp " + i);
                    PlayerPrefs.DeleteKey("Slot Amount Temp " + i);
                }
            }

            for(int i = 0; i < data.shortcuts.Length; i++)
            {
                if (data.shortcuts[i] >= 0)
                    PlayerPrefs.SetInt("Shortcut" + i, data.shortcuts[i]);
                else
                    PlayerPrefs.DeleteKey("Shortcut" + i);
            }

            PlayerPrefs.SetInt("Files Count", data.filesCount);

            for (int i = 0; i < data.filesCount; i++)
            {
                if (data.files[i].files != "")
                {
                    PlayerPrefs.SetString("Slot File Temp " + i, data.files[i].files);

                    if(data.files[i].hasReaded)
                        PlayerPrefs.SetString("Slot Readed Temp " + i, data.files[i].files);
                    else
                        PlayerPrefs.DeleteKey("Slot Readed Temp " + i);
                }
                else
                {
                    PlayerPrefs.DeleteKey("Slot File Temp " + i);
                    PlayerPrefs.DeleteKey("Slot Readed Temp " + i);
                }
            }

            PlayerPrefs.SetInt("Map Amount", data.mapsCount);

            for (int i = 0; i < data.mapsCount; i++)
            {
                if (data.maps[i] == true)
                {
                    PlayerPrefs.SetInt("Map " + i, 1);
                }
                else
                {
                    PlayerPrefs.SetInt("Map " + i, 0);
                }
            }

            #endregion

            #region ItemBox

            int itemsCount = data.itemBoxCount;
            PlayerPrefs.SetInt("ItemsCount", itemsCount);

            for (int i = 0; i < itemsCount; i++)
            {
                if(data.boxSlots[i].itemName != "")
                {
                    PlayerPrefs.SetString("Slot ItemBox Temp " + i, data.boxSlots[i].itemName);
                    if (data.boxSlots[i].amount > 0)
                        PlayerPrefs.SetInt("Slot ItemBox Amount Temp " + i, data.boxSlots[i].amount);
                    else
                        PlayerPrefs.SetInt("Slot ItemBox Amount Temp " + i, 1);
                }
                else
                {
                    PlayerPrefs.DeleteKey("Slot ItemBox Temp " + i);
                    PlayerPrefs.DeleteKey("Slot ItemBox Amount Temp " + i);
                }
            }

            #endregion

            #region SceneController

            for(int i = 0; i < sceneController.sceneData.scenes.Length; i++)
            {
                //items
                if (data.sceneSave[i].sceneItems != null)
                {
                    sceneController.sceneData.scenes[i].sceneItems = new SceneItem[data.sceneSave[i].sceneItems.Length];

                    for (int x = 0; x < data.sceneSave[i].sceneItems.Length; x++)
                    {
                        if (data.sceneSave[i].sceneItems[x].itemName != "")
                        {
                            sceneController.sceneData.scenes[i].sceneItems[x].itemName = data.sceneSave[i].sceneItems[x].itemName;
                            sceneController.sceneData.scenes[i].sceneItems[x].amount = data.sceneSave[i].sceneItems[x].amount;

                            sceneController.sceneData.scenes[i].sceneItems[x].pos.x = data.sceneSave[i].sceneItems[x].pos.x;
                            sceneController.sceneData.scenes[i].sceneItems[x].pos.y = data.sceneSave[i].sceneItems[x].pos.y;
                            sceneController.sceneData.scenes[i].sceneItems[x].pos.z = data.sceneSave[i].sceneItems[x].pos.z;

                            sceneController.sceneData.scenes[i].sceneItems[x].rot.x = data.sceneSave[i].sceneItems[x].rot.x;
                            sceneController.sceneData.scenes[i].sceneItems[x].rot.y = data.sceneSave[i].sceneItems[x].rot.y;
                            sceneController.sceneData.scenes[i].sceneItems[x].rot.z = data.sceneSave[i].sceneItems[x].rot.z;

                        }
                    }
                }

                //interactibles
                sceneController.sceneData.scenes[i].sceneInteractibles = data.sceneSave[i].sceneInteractibles;

                //enemies
                sceneController.sceneData.scenes[i].sceneEnemies = data.sceneSave[i].sceneEnemies;

                //finish
                sceneController.sceneData.scenes[i].sceneSaved = data.sceneSave[i].sceneSaved;
            }

            #endregion

            #region Maps Settings

            if (data.mapDatas != null)
            {
                for (int i = 0; i < data.mapIdCount; i++)
                {
                    GameObject dataObj = new GameObject();
                    MapData mapData = dataObj.AddComponent<MapData>();
                    mapData.mapId = data.mapDatas[i].mapId;
                    mapData.doors = new DoorIndicator[data.mapDatas[i].doorStates.Length];

                    for (int x = 0; x < data.mapDatas[i].doorStates.Length; x++)
                    {
                        mapData.doors[x].doorState = (DoorState)(data.mapDatas[i].doorStates[x]);
                    }
                }
            }

            #endregion

            //finish
            FinishLoad(data.sceneIndex);
            print("loaded!");
        }
    }

    void FinishLoad(int index)
    {
        FadeInOut fade = FindObjectOfType(typeof(FadeInOut)) as FadeInOut;
        fade.FadeAndLoad(index);
    }
}

[Serializable]
public class PlayerData
{
    //info
    public string[] sceneName;
    public float playTime;
    public string playTimeString;
    public int gamesSaved;
    public int difficulty;

    //scene
    public int sceneIndex;
    public int spawnPos;

    //player data
    public float currentLife;
    public bool flashlightOn;
    public bool haveWeapon;
    public int currentWeaponSlot;

    //inventory data
    [Serializable]
    public struct SlotData
    {
        public bool haveItem;
        public string itemName;
        public int amount;
    }
    public int slotsAvailable;
    public SlotData[] slots = new SlotData[16];

    public int[] shortcuts = new int[4]; 

    [Serializable]
    public struct FileData
    {
        public string files;
        public bool hasReaded;
    }
    public int filesCount;
    public FileData[] files;

    public int mapsCount;
    public bool[] maps;

    //maps settings

    [Serializable]
    public struct MapsInd
    {
        public int mapId;

        public int[] doorStates;
    }

    public int mapIdCount;
    public MapsInd[] mapDatas;

    //item box data
    [Serializable]
    public struct SlotBoxData
    {
        public string itemName;
        public int amount;
    }
    public int itemBoxCount;
    public SlotBoxData[] boxSlots;

    //all in scene
    [Serializable]
    public struct ItemScene
    {
        public string itemName;
        public Vector3Serialization pos;
        public Vector3Serialization rot;
        public int amount;
    }

    [Serializable]
    public struct SceneSave
    {
        public ItemScene[] sceneItems;
        public SceneInteractibles[] sceneInteractibles;
        public SceneEnemies[] sceneEnemies;
        public bool sceneSaved;
    }

    //screenshot
    public byte[] screenshot;

    public SceneSave[] sceneSave;
}

[Serializable]
public struct Vector3Serialization
{
    public float x, y, z;
}
