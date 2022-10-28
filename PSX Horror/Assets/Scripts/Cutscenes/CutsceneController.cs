using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneController : MonoBehaviour
{
    public CanvasGroup cutsceneSkipText;
    [HideInInspector]
    public float currentTimeAddSkip;
    public float speedToFade = 2;
    public bool showed;

    // Start is called before the first frame update
    void Start()
    {
        cutsceneSkipText = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        currentTimeAddSkip = 0;
    }

    // Update is called once per frame
    void Update()
    {
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

        if (Input.anyKeyDown)
        {
            currentTimeAddSkip = 5f;
        }
    }
}
