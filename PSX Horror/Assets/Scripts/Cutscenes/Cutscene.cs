using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

public class Cutscene : MonoBehaviour {

    public PlayableDirector cutscene;
    public bool init;
    public float time, duration;
    public UnityEvent OnStart, OnStop;
    public bool skippable;
    bool hasSkiped;

    CutsceneController cutsceneController;

	// Use this for initialization
	void Start () {
        if(!cutscene)
            cutscene = GetComponent<PlayableDirector>();

        cutscene.enabled = false;

        if (cutscene.playOnAwake)
        {
            Play();
        }

        if (!GameManager.instance && FindObjectOfType(typeof(CutsceneController)))
            cutsceneController = FindObjectOfType(typeof(CutsceneController)) as CutsceneController;
    }
	
	// Update is called once per frame
	void Update () {
        time = (float)cutscene.time;
        duration = (float)cutscene.duration;

        if (!hasSkiped && (Input.GetKeyDown(KeyCode.Escape) ||
            InputManager.instance.GetJoyButtonDown("B")) 
            && init && time < duration && skippable && ((cutsceneController && cutsceneController.showed) || (GameManager.instance && UIController.instance.showed)))
        {
            hasSkiped = true;
            cutscene.time = (double)(duration - 0.05f);
            CancelInvoke("Stop");
            Invoke("Stop", 0.05f);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            GetComponent<Collider>().enabled = false;
            Play();
        }
    }

    public void Play()
    {
        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Cutscene;
        cutscene.enabled = true;

        if(skippable && UIController.instance)
            UIController.instance.PassCutscene(this);

        hasSkiped = false;
        init = true;
        OnStart.Invoke();
        cutscene.Play();
        Invoke("Stop", (float)cutscene.duration);
    }

    public void Stop()
    {
        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Game;

        if (skippable && UIController.instance)
            UIController.instance.PassCutscene(null);

        OnStop.Invoke();
        cutscene.Stop();
    }
}
