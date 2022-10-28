using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [System.Serializable]
    public class EventButtons
    {
        public GameObject resumeButton, backButton, loadGameInDeathPanel, mapButton, firstSlot, yesSave;
    }

    public EventButtons buttons;
    public PlayerController player;
    public GameObject pauseMenu, inventory,  Hud, deathMenu, itemBoxMenu, cutscenePanel;

    
    [Header("Player States")]
    public Color fine, caution, danger;
    public Image damageScreen;
    public Text hpCounter;
    public Image bleedIcon;
    public Image heartbeat;

    [Space]
    public CanvasGroup cutsceneSkipText;
    //[HideInInspector]
    public float currentTimeAddSkip;
    public float speedToFade = 2;
    public bool showed;
    Cutscene cutscene;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        damageScreen.enabled = false;

        player = FindObjectOfType(typeof(PlayerController)) as PlayerController;
        cutsceneSkipText = cutscenePanel.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((player.currentLife / player.maxLife) > 0.5f)
        {
            heartbeat.color = hpCounter.color = fine;
        }
        else if ((player.currentLife / player.maxLife) <= 0.5f && (player.currentLife / player.maxLife) > 0.25f)
        {
            heartbeat.color = hpCounter.color = caution;
        }
        else if ((player.currentLife / player.maxLife) <= 0.25f)
        {
            heartbeat.color = hpCounter.color = danger;
        }

        if (currentTimeAddSkip > 0)
        {
            currentTimeAddSkip -= Time.unscaledDeltaTime;
            if (cutsceneSkipText.alpha < 1)
                cutsceneSkipText.alpha += speedToFade * Time.unscaledDeltaTime;
            showed = true;
        }
        else
        {
            if (cutsceneSkipText.alpha > 0)
                cutsceneSkipText.alpha -= speedToFade * Time.unscaledDeltaTime;
            showed = false;
        }

        hpCounter.text = player.currentLife.ToString();
        bleedIcon.enabled = (player.bleeding);

        if (cutscene && GameManager.instance.gameStatus == GameStatus.Cutscene && Input.anyKeyDown)
            currentTimeAddSkip = 5f;
    }

    public void PassCutscene(Cutscene playable)
    {
        StartCoroutine(Cut(playable));
    }

    IEnumerator Cut(Cutscene playable)
    {
        yield return new WaitForEndOfFrame();
        cutscene = playable;
    }

    public void HitScreen()
    {
        StartCoroutine(StartHit());
    }

    public IEnumerator StartHit()
    {
        damageScreen.enabled = true;
        yield return new WaitForSeconds(3.5f * Time.deltaTime);
        damageScreen.enabled = false;
    }

    public void ShowPause()
    {
        pauseMenu.SetActive(true);
        inventory.SetActive(false);
        Hud.SetActive(false);
        deathMenu.SetActive(false);
        itemBoxMenu.SetActive(false);
        cutscenePanel.SetActive(false);
    }

    public void ShowCutscene()
    {
        pauseMenu.SetActive(false);
        inventory.SetActive(false);
        Hud.SetActive(false);
        deathMenu.SetActive(false);
        itemBoxMenu.SetActive(false);
        cutscenePanel.SetActive(true);
    }

    public void OpenInventory()
    {
        pauseMenu.SetActive(false);
        inventory.SetActive(true);
        Hud.SetActive(false);
        deathMenu.SetActive(false);
        itemBoxMenu.SetActive(false);
        cutscenePanel.SetActive(false);
    }

    public void ShowItemBoxMenu()
    {
        pauseMenu.SetActive(false);
        inventory.SetActive(false);
        Hud.SetActive(false);
        deathMenu.SetActive(false);
        itemBoxMenu.SetActive(true);
        cutscenePanel.SetActive(false);
    }

    public void ShowHud()
    {
        pauseMenu.SetActive(false);
        inventory.SetActive(false);
        Hud.SetActive(true);
        deathMenu.SetActive(false);
        itemBoxMenu.SetActive(false);
        cutscenePanel.SetActive(false);
    }

    public void ShowDeathMenu()
    {
        pauseMenu.SetActive(false);
        inventory.SetActive(false);
        Hud.SetActive(false);
        deathMenu.SetActive(true);
        itemBoxMenu.SetActive(false);
        cutscenePanel.SetActive(false);
    }

    public void HideAll()
    {
        pauseMenu.SetActive(false);
        inventory.SetActive(false);
        Hud.SetActive(false);
        deathMenu.SetActive(false);
        itemBoxMenu.SetActive(false);
        cutscenePanel.SetActive(false);
    }
}
