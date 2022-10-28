using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Audio;
using System;


public class Settings : MonoBehaviour
{
    public static Settings instance;

    //Graphics
    [Header("Graphics")]
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset lwrpAsset;
    int resolution, qualityLevel, shadowsLevel, vSyncLevel;
    int maxRes;
    bool isFullscreen;
    public bool hasPP = true;
    Resolution[] resolutions;
    List<int> options;
    public Text resolutionText, qualityText, shadowsText, vSyncText, fullscreenText, ppText;

    //Audio
    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider mainSlider, sfxSlider, musicSlider;

    //Gameplay
    [Header("Gameplay")]
    public bool tank;
    public Text tankText;

    public bool cursorOn;
    public Text cursorText;

    //Languages
    [Header("Language")]
    public int currentLanguage;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        StartSettings();
    }

    void Start()
    {
        //Audio
        mainSlider.value = PlayerPrefs.GetFloat("Main volume", 0.75f);
        mainMixer.SetFloat("Master", Mathf.Log10(mainSlider.value) * 20);

        sfxSlider.value = PlayerPrefs.GetFloat("Sfx volume", 0.75f);
        mainMixer.SetFloat("SFX", Mathf.Log10(sfxSlider.value) * 20);

        musicSlider.value = PlayerPrefs.GetFloat("Music volume", 0.75f);
        mainMixer.SetFloat("Music", Mathf.Log10(musicSlider.value) * 20);
    }

    void StartSettings()
    {
        //Resolution
        resolutions = Screen.resolutions;

        options = new List<int>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width >= 800 || resolutions[i].height >= 600)
            {
                options.Add(i);
            }
        }

        maxRes = options.Count - 1;

        resolution = (PlayerPrefs.HasKey("Resolution")) ? PlayerPrefs.GetInt("Resolution") : maxRes;
        resolution = Mathf.Clamp(resolution, 0, maxRes);

        isFullscreen = (PlayerPrefs.GetInt("Fullscreen") == 0) ? true : false;
        Screen.fullScreen = isFullscreen;
        fullscreenText.text = (isFullscreen) ? "ON" : "OFF";

        SetResolutionTo(resolution);

        //Graphics
        string[] names = QualitySettings.names;

        qualityLevel = (PlayerPrefs.HasKey("Quality")) ? PlayerPrefs.GetInt("Quality") : 1;
        qualityLevel = Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(qualityLevel);

        shadowsLevel = (PlayerPrefs.HasKey("Shadows")) ? PlayerPrefs.GetInt("Shadows") : 1;
        lwrpAsset.shadowDistance = (shadowsLevel == 0) ? 10 : 20;

        vSyncLevel = (PlayerPrefs.HasKey("VSync")) ? PlayerPrefs.GetInt("VSync") : 1;
        QualitySettings.vSyncCount = vSyncLevel;

        hasPP = (PlayerPrefs.GetInt("PP") == 0) ? false : true;

        qualityText.text = names[qualityLevel].ToString();
        shadowsText.text = (shadowsLevel == 0) ? "Normal" : "High";
        vSyncText.text = (vSyncLevel == 0) ? "OFF" : "ON";
        ppText.text = (!hasPP) ? "OFF" : "ON";

        //Gameplay
        tank = (PlayerPrefs.GetInt("Tank") == 0) ? true : false;
        tankText.text = (tank) ? "Classic" : "Alternative";

        cursorOn = (PlayerPrefs.GetInt("Cursor") == 0) ? true : false;
        cursorText.text = (cursorOn) ? "ON" : "OFF";

        //Language
        currentLanguage = PlayerPrefs.GetInt("Language");
    }

    public void Default()
    {

    }

    #region Graphics
    public void SetResolution(int index)
    {
        resolution += index;
        resolution = Mathf.Clamp(resolution, 0, maxRes);

        SetResolutionTo(resolution);
    }

    public void SetResolutionLoop(int index)
    {
        resolution += index;

        if (resolution > maxRes) resolution = 0;
        if (resolution < 0) resolution = maxRes;

        SetResolutionTo(resolution);
    }

    void SetResolutionTo(int index)
    {
        PlayMoveAudio();

        Resolution resolution = resolutions[options[index]];

        if(Screen.currentResolution.width != resolution.width ||
            Screen.currentResolution.height != resolution.height ||
            Screen.currentResolution.refreshRate != resolution.refreshRate)
            Screen.SetResolution(resolution.width, resolution.height, isFullscreen, resolution.refreshRate);

        resolutionText.text = resolution.ToString();

        PlayerPrefs.SetInt("Resolution", this.resolution);
    }

    public void SetFullscreen()
    {
        PlayMoveAudio();

        isFullscreen = !isFullscreen;
        int value = isFullscreen ? 0 : 1;
        PlayerPrefs.SetInt("Fullscreen", value);
        Screen.fullScreen = isFullscreen;
        fullscreenText.text = (isFullscreen) ? "ON" : "OFF";
    }

    public void SetQuality(int value)
    {
        PlayMoveAudio();

        qualityLevel += value;

        qualityLevel = Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length);

        PlayerPrefs.SetInt("Quality", qualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel);
        qualityLevel = QualitySettings.GetQualityLevel();

        qualityText.text = QualitySettings.names[qualityLevel].ToString();
    }

    public void SetQualityLoop(int value)
    {
        PlayMoveAudio();

        qualityLevel += value;

        if (qualityLevel > QualitySettings.names.Length - 1) qualityLevel = 0;
        if (qualityLevel < 0) qualityLevel = QualitySettings.names.Length;

        PlayerPrefs.SetInt("Quality", qualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel);
        qualityLevel = QualitySettings.GetQualityLevel();

        qualityText.text = QualitySettings.names[qualityLevel].ToString();
    }

    public void SetShadows()
    {
        PlayMoveAudio();

        shadowsLevel = (shadowsLevel == 0) ? 1 : 0;
        PlayerPrefs.SetInt("Shadows", shadowsLevel);
        lwrpAsset.shadowDistance = (shadowsLevel == 0) ? 10 : 20;

        shadowsText.text = (shadowsLevel == 0) ? "Normal" : "High";
    }

    public void SetVSync()
    {
        PlayMoveAudio();

        vSyncLevel = (vSyncLevel == 1) ? 0 : 1;
        QualitySettings.vSyncCount = vSyncLevel;
        PlayerPrefs.SetInt("VSync", vSyncLevel);
        vSyncText.text = (vSyncLevel == 0) ? "OFF" : "ON";
    }

    public void SetPP()
    {
        PlayMoveAudio();

        hasPP = !hasPP;
        if (FXController.instance)
            FXController.instance.UpdatePostProcessing(hasPP);
        PlayerPrefs.SetInt("PP", hasPP ? 1 : 0);
        ppText.text = (!hasPP) ? "OFF" : "ON";
    }

    #endregion

    #region Audio

    public void SetMainVolumeL(int value)
    {
        mainSlider.value += value * Time.unscaledDeltaTime;
    }

    public void SetMainVolume(float volume)
    {
        mainMixer.SetFloat("Master", Mathf.Log10(mainSlider.value) * 20);
        PlayerPrefs.SetFloat("Main volume", mainSlider.value);
    }

    public void SetSfxVolumeL(int value)
    {
        sfxSlider.value += value * Time.unscaledDeltaTime;
    }

    public void SetSfxVolume(float volume)
    {
        mainMixer.SetFloat("SFX", Mathf.Log10(sfxSlider.value) * 20);
        PlayerPrefs.SetFloat("Sfx volume", sfxSlider.value);
    }

    public void SetMusicVolumeL(int value)
    { 
        musicSlider.value += value * Time.unscaledDeltaTime;
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("Music", Mathf.Log10(musicSlider.value) * 20);
        PlayerPrefs.SetFloat("Music volume", musicSlider.value);
    }

    #endregion

    #region Gameplay
    public void SetTank()
    {
        PlayMoveAudio();

        int value = (tank) ? 1 : 0;
        PlayerPrefs.SetInt("Tank", value);
        tank = (PlayerPrefs.GetInt("Tank") == 0) ? true : false;
        tankText.text = (tank) ? "Classic" : "Alternative";
    }

    public void SetCursor()
    {
        PlayMoveAudio();

        int value = (cursorOn) ? 1 : 0;
        PlayerPrefs.SetInt("Cursor", value);
        cursorOn = (PlayerPrefs.GetInt("Cursor") == 0) ? true : false;
        cursorText.text = (cursorOn) ? "ON" : "OFF";
    }

    #endregion

    public void PlayCancelAudio()
    {
        if (InventoryUI.instance)
            InventoryUI.instance.PlayCancelAudio();
        else if (MainMenu.instance)
            MainMenu.instance.PlayCancelAudio();
    }

    public void PlayMoveAudio()
    {
        if (InventoryUI.instance)
            InventoryUI.instance.PlayMoveAudio();
        else if (MainMenu.instance)
            MainMenu.instance.PlayMoveAudio();
    }
}
