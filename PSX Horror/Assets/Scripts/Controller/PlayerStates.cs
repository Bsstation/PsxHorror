using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    public static PlayerStates instance;
    public float timer;
    public int saved;
    public Difficulty difficulty;

    // Start is called before the first frame update
    void Awake()
    {
        transform.parent = null;
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        timer = (PlayerPrefs.HasKey("Timer")) ? PlayerPrefs.GetFloat("Timer") : 0;
        PlayerPrefs.DeleteKey("Timer");
        saved = (PlayerPrefs.HasKey("Saved")) ? PlayerPrefs.GetInt("Saved") : 0;
        PlayerPrefs.DeleteKey("Saved");
        difficulty = PlayerPrefs.HasKey("Difficulty") ? (Difficulty)PlayerPrefs.GetInt("Difficulty") : (Difficulty)1;
        PlayerPrefs.DeleteKey("Difficulty");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance) {
            if(GameManager.instance.gameStatus == GameStatus.Game || GameManager.instance.gameStatus == GameStatus.Inventory ||
                GameManager.instance.gameStatus == GameStatus.ItemBox || GameManager.instance.gameStatus == GameStatus.SaveGame)
            {
                timer += Time.unscaledDeltaTime;
            }
        }
    }
}
