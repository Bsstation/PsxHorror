using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    public int mapId;

    public DoorIndicator[] doors;

    public MapData[] mapDatas;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);     
    }

    private void Start()
    {
        mapDatas = FindObjectsOfType<MapData>();

        foreach (MapData map in mapDatas)
        {
            if (map.gameObject != gameObject && map.mapId == mapId)
            {
                Destroy(map.gameObject);
            }
        }
    }
}
