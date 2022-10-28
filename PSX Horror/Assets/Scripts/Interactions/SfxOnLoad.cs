using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxOnLoad : MonoBehaviour
{
    public AudioClip clip;

    private void OnLevelWasLoaded(int level)
    {
        Play();
    }

    public void Play()
    {
        StartCoroutine(Enumerator());
    }

    IEnumerator Enumerator()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        GetComponent<AudioSource>().PlayOneShot(clip);

        Destroy(gameObject, clip.length + 0.5f);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
