using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVhs : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer player;

    // Start is called before the first frame update
    public void Init()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        player.Prepare();

        while (!player.isPrepared)
        {
            yield return null;
        }

        rawImage.texture = player.texture;
        player.Play();
    }
}