using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public Image fadeOutUIImage;
    public float fadeSpeed = 0.8f;

    public bool fading;

    public enum FadeDirection
    {
        In, //Alpha = 1
        Out // Alpha = 0
    }

    private void Awake()
    {
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, 1);
        fadeOutUIImage.enabled = true;
    }

    private void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        if (GameManager.instance && GameManager.instance.gameStatus == GameStatus.Game)
            GameManager.instance.gameStatus = GameStatus.Fading;

        PlayVhs vhs = FindObjectOfType(typeof(PlayVhs)) as PlayVhs;

        if (vhs)
        {
            vhs.Init();

            while (!vhs.player.isPrepared)
            {
                yield return null;
            }
        }

        if (GameManager.instance && GameManager.instance.gameStatus == GameStatus.Fading)
            GameManager.instance.gameStatus = GameStatus.Game;

        StartCoroutine(Fade(FadeDirection.Out));
    }

    // Start is called before the first frame update
    public void NextScene()
    {
        int newScene = SceneManager.GetActiveScene().buildIndex + 1;

        StartCoroutine(FadeAndLoadScene(FadeDirection.In, newScene));
    }

    public void FadeAndLoad(int newScene)
    {
        StartCoroutine(FadeAndLoadScene(FadeDirection.In, newScene));
        if(MusicController.instance)
            MusicController.instance.Fade(MusicController.FadeMusic.OUT);
    }

    public IEnumerator Fade(FadeDirection fadeDirection)
    {
        fading = true;

        float alpha = (fadeDirection == FadeDirection.Out) ? 1 : 0;
        float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 0 : 1;

        if (fadeDirection == FadeDirection.Out)
        {
            fadeOutUIImage.enabled = true;
            while (alpha >= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;
            while (alpha <= fadeEndValue)
            {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
        }

        fading = false;
    }

    IEnumerator AsyncLoad(int scene)
    {
        FadeDoor[] fadeDoors = FindObjectsOfType(typeof(FadeDoor)) as FadeDoor[];

        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Loading;
        else if (transform.Find("Loading Screen"))
            transform.Find("Loading Screen").gameObject.SetActive(true);

        for (int i = 0; i < fadeDoors.Length; i++)
        {
            while (fadeDoors[i].GetComponent<AudioSource>().isPlaying)
            {
                yield return null;
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        yield return new WaitForSecondsRealtime(1.6f);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator AsyncLoad(string scene)
    {
        FadeDoor[] fadeDoors = FindObjectsOfType(typeof(FadeDoor)) as FadeDoor[];

        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Loading;
        else if (transform.Find("Loading Screen"))
            transform.Find("Loading Screen").gameObject.SetActive(true);

        for (int i = 0; i < fadeDoors.Length; i++)
        {
            while (fadeDoors[i].GetComponent<AudioSource>().isPlaying)
            {
                yield return null;
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        yield return new WaitForSecondsRealtime(0.3f);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    #region HELPERS
    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, int sceneToLoad)
    {
        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Fading;
        yield return Fade(fadeDirection);
        StartCoroutine(AsyncLoad(sceneToLoad));
    }

    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, string sceneToLoad)
    {
        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Fading;
        yield return Fade(fadeDirection);
        StartCoroutine(AsyncLoad(sceneToLoad));
    }

    private void SetColorImage(ref float alpha, FadeDirection fadeDirection)
    {
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, alpha);
        alpha += Time.fixedUnscaledDeltaTime * (1.0f / fadeSpeed) * ((fadeDirection == FadeDirection.Out) ? -1 : 1);
    }
    #endregion
}
