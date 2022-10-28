using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    public static CraftManager instance;

    [System.Serializable]
    public class Recipe
    {
        public string result;

        public string item1;
        public string item2;
    }

    public List<Recipe> recipes = new List<Recipe>();

    private void Awake()
    {
        instance = this;
    }

    public GameObject CheckRecipe(ItemBase item1, ItemBase item2)
    {
        for(int i = 0; i < recipes.Count; i++)
        {
            if(item1.itemNameId == recipes[i].item1)
            {
                if(item2.itemNameId == recipes[i].item2)
                {
                    var result = Resources.Load("Items/" + recipes[i].result) as GameObject;
                    return result;
                }
            }
            else if(item1.itemNameId == recipes[i].item2)
            {
                if(item2.itemNameId == recipes[i].item1)
                {
                    var result = Resources.Load("Items/" + recipes[i].result) as GameObject;
                    return result;
                }
            }
        }
        return null;
    }
}
