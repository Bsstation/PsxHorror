using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState : int { Unknown = 0, ForeverLocked = 1, Locked = 2, Unlocked = 3 }

[System.Serializable]
public struct DoorIndicator
{
    public DoorState doorState;
}

public class MapController : MonoBehaviour
{
    public static MapController instance;

    public GameObject map;
    public bool haveMap;
    public int mapId;

    public string[] areaName = new string[2];

    public Transform doorGrid;

    public Vector2 clamp;
    public float panoramicZoom = 15;

    public DoorIndicator[] doors;

    public Sprite lockedSprite, unlockedSprite, unknownSprite ,lockedForeverSprite;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    public void Start()
    {
        if(!haveMap && InventoryUI.instance.maps.Length > mapId)
            haveMap = InventoryUI.instance.maps[mapId];

        DoorToIndex();

        LoadDoors();

        InitDoors();
    }

    public void DoorToIndex()
    {
        InteractibleBase[] interactibles = Resources.FindObjectsOfTypeAll(typeof(InteractibleBase)) as InteractibleBase[];
        List<FadeDoor> fadeDoors = new List<FadeDoor>();
        List<TeleportingDoor> teleportingDoors = new List<TeleportingDoor>();

        foreach (InteractibleBase i in interactibles)
        {
            if (i.gameObject.scene.IsValid())
            {
                if (i.GetComponent<TeleportingDoor>() && !teleportingDoors.Contains(i.GetComponent<TeleportingDoor>()))
                    teleportingDoors.Add(i.GetComponent<TeleportingDoor>());
                else if (i.GetComponent<FadeDoor>() && !fadeDoors.Contains(i.GetComponent<FadeDoor>()))
                    fadeDoors.Add(i.GetComponent<FadeDoor>());
            }
        }

        for(int i = 0; i < fadeDoors.Count; i++)
             fadeDoors[i].doorIndicatorIndex = ClosestDoor(fadeDoors[i].transform);

        for (int i = 0; i < teleportingDoors.Count; i++)
            teleportingDoors[i].doorIndicatorIndex = ClosestDoor(teleportingDoors[i].transform);
}

    public int ClosestDoor(Transform checker)
    {
        List<float> distances = new List<float>();

        distances.Clear();

        foreach (Transform i in doorGrid)
        {
            float dist = Vector3.Distance(checker.position, i.position);
            distances.Add(dist);
        }

        distances.Sort();

        for (int i = 0; i < doorGrid.childCount; i++)
        {
            if (distances[0] == Vector3.Distance(checker.position, doorGrid.GetChild(i).position))
                return i;
        }
        return -1;
    }

    public void InitDoors()
    {
        for(int i = 0; i < doorGrid.childCount; i++)
        {
            switch (doors[i].doorState)
            {
                case DoorState.Unknown:
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().sprite = unknownSprite;
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                    break;

                case DoorState.ForeverLocked:
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().sprite = lockedForeverSprite;
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    break;

                case DoorState.Locked:
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().sprite = lockedSprite;
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    break;

                case DoorState.Unlocked:
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().sprite = unlockedSprite;
                    doorGrid.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    break;
            }
        }
    }

    public void SetDoor(int index, DoorState state)
    {
        doors[index].doorState = state;
        InitDoors();
    }

    public void SaveDoors()
    {
        MapData data = new GameObject().AddComponent<MapData>();
        data.gameObject.name = "MapData" + mapId;
        data.mapId = mapId;
        data.doors = doors;
    }

    public void LoadDoors()
    {
        MapData[] datas = FindObjectsOfType<MapData>();

        foreach(MapData data in datas) {
            if (data.mapId == mapId)
            {
                System.Array.Resize(ref doors, data.doors.Length);

                for (int i = 0; i < doors.Length; i++)
                {
                    doors[i].doorState = data.doors[i].doorState;
                }

                Destroy(data.gameObject);

                break;
            }
        }
    }
}