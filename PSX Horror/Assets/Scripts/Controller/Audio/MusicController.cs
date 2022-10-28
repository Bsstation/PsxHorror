using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public enum MusicState { Action, Exploring }
    public enum FadeMusic { IN, OUT }

    public static MusicController instance;

    AudioSource audioSource;

    public float fadeSpeed = 0.8f;

    [Space]

    [Range(0, 1f)]
    public float minValue = 0;
    [Range(0, 1f)]
    public float maxValue = 0.5f;

    [Space]
    public AudioClip normalTheme, actionTheme;
    MusicState currentMusicState;

    EnemyBase[] enemies;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = normalTheme;
        audioSource.Play();

        Fade(FadeMusic.IN);
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckMusicState() == true && currentMusicState == MusicState.Exploring)
        {
            ChangeState(MusicState.Action);
        }

        if (CheckMusicState() == false && currentMusicState == MusicState.Action)
        {
            ChangeState(MusicState.Exploring);
        }
    }

    bool CheckMusicState()
    {
        if (enemies.Length > 0)
        {
            foreach (EnemyBase en in enemies)
            {
                if (en && en.gameObject.activeInHierarchy && !en.dead && en.chasing)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void ChangeState(MusicState musicState)
    {
        if(musicState == MusicState.Action)
        {
            currentMusicState = MusicState.Action;
            audioSource.Stop();
            audioSource.clip = actionTheme;
            audioSource.Play();
        }
        else
        {
            currentMusicState = MusicState.Exploring;
            audioSource.Stop();
            audioSource.clip = normalTheme;
            audioSource.Play();
        }
    }

    void FindEnemies()
    {
        enemies = FindObjectsOfType(typeof(EnemyBase)) as EnemyBase[];
    }

    public void Fade(FadeMusic fade)
    {
        StopAllCoroutines();

        switch (fade)
        {
            case FadeMusic.IN:
                StartCoroutine(FadeIN());
                break;
            case FadeMusic.OUT:
                StartCoroutine(FadeOut());
                break;
        }
    }

    IEnumerator FadeIN()
    {
        float volumeStart = minValue;
        float volumeEnd = maxValue;

        FindEnemies();

        audioSource.volume = volumeStart;

        while (volumeStart <= volumeEnd)
        {
            SetVolume(ref volumeStart, FadeMusic.IN, fadeSpeed * 3f);
            yield return null;
        }

        audioSource.volume = Mathf.Clamp(audioSource.volume, minValue, maxValue);
    }

    IEnumerator FadeOut()
    {
        float volumeStart = maxValue;
        float volumeEnd = minValue;

        audioSource.volume = volumeStart;

        while (volumeStart >= volumeEnd)
        {
            SetVolume(ref volumeStart, FadeMusic.OUT, fadeSpeed / 2);
            yield return null;
        }

        audioSource.volume = Mathf.Clamp(audioSource.volume, minValue, maxValue);
    }

    void SetVolume(ref float volume, FadeMusic fade, float speed)
    {
        volume += Time.unscaledDeltaTime * (maxValue / speed) * ((fade == FadeMusic.OUT) ? -1 : 1);
        audioSource.volume = volume;
    }
}
