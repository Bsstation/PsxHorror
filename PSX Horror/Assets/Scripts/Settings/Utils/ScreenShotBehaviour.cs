using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScreenShotBehaviour : MonoBehaviour
{
    public static ScreenShotBehaviour instance;

    Camera myCamera;
    bool takeScreenshotOnNextFrame;

    public Sprite sprite;
    public string[] sceneName;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            myCamera = GetComponent<Camera>();

            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();
        if (takeScreenshotOnNextFrame)
        {
            RenderTexture render = myCamera.targetTexture;
            Texture2D result = new Texture2D(render.width, render.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, render.width, render.height);
            result.ReadPixels(rect, 0, 0);
            result.Apply();

            /*
            byte[] BitArray = result.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Screenshot.png", BitArray);
            */

            sprite = Sprite.Create(result, rect, Vector2.zero);

            RenderTexture.ReleaseTemporary(render);
            myCamera.targetTexture = null;

            takeScreenshotOnNextFrame = false;

            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    public void PrepareScreenshot(int width, int height)
    {
        UIController.instance.HideAll();
        myCamera.Render();
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenshotOnNextFrame = true;
        UIController.instance.ShowHud();

        StartCoroutine(Capture());
    }
}
