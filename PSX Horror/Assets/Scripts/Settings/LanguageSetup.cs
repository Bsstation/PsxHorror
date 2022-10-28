using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LanguageSetup : MonoBehaviour
{
    [System.Serializable]
    public class Languages
    {
        public List<string> languageTexts;
    }

    public List<Languages> textArc;

    public List<Text> texts;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup()
    {
        int currentLanguage = PlayerPrefs.GetInt("Language");
        for (int i = 0; i < texts.Capacity; i++)
        {
            if(texts[i])
                texts[i].text = textArc[i].languageTexts[currentLanguage];
        }
    }
}

