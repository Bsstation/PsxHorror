using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageSelect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteKey("Language");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectLanguage(int index)
    {
        PlayerPrefs.SetInt("Language", index);
        SceneManager.LoadScene(1);
    }
}
