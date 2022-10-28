using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportingDoor : InteractibleBase
{
    FadeInOut fade;

    public Transform newPos;

    [Space]
    public GameObject newScene;
    //Caso tenha uma porta trancada pelo outro lado
    public TeleportingDoor doorToUnlock;
    GameObject currentScene;
    [HideInInspector]
    public int doorIndicatorIndex;

    // Start is called before the first frame update
    new private void Start()
    {
        fade = FindObjectOfType(typeof(FadeInOut)) as FadeInOut;
        base.Start();
        GetCurrentScene();
    }

    void GetCurrentScene()
    {
        GameObject scene = ExtensionMethods.FindParentWithTag(gameObject, "Scene");
        currentScene = scene;
    }

    public override void LockedReaction()
    {
        MapController.instance.SetDoor(doorIndicatorIndex, (keyIndex != 0) ? DoorState.Locked : DoorState.ForeverLocked);
    }

    public override void OnInteract()
    {
        MapController.instance.SetDoor(doorIndicatorIndex, DoorState.Unlocked);
        if (doorToUnlock && doorToUnlock.locked)
        {
            doorToUnlock.locked = false;
            if (unlock)
                audioSource.PlayOneShot(unlock);
            MessagesBehaviour.instance.SendMessageTxt(
                MessagesBehaviour.instance.youUnlocked.msgs[Settings.instance.currentLanguage]);
        }
        else
            StartCoroutine(Teleport());
    }

    IEnumerator Teleport()
    {
        fade.StopAllCoroutines();

        if (interact)
            audioSource.PlayOneShot(interact);

        fade.StartCoroutine(fade.Fade(FadeInOut.FadeDirection.In));

        if (MusicController.instance)
            MusicController.instance.Fade(MusicController.FadeMusic.OUT);

        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Fading;

        yield return new WaitForSecondsRealtime(1.6f);

        while (fade.fading)
            yield return null;

        GameManager.instance.player.SetPosition(newPos);

        newScene.SetActive(true);
        //cena nova aberta
        fade.StartCoroutine(fade.Fade(FadeInOut.FadeDirection.Out));

        if (GameManager.instance)
            GameManager.instance.gameStatus = GameStatus.Game;

        var sfx = Resources.Load("FX/FadeSFX") as GameObject;
        GameObject tempSFX = Instantiate(sfx);
        tempSFX.transform.position = transform.position;
        tempSFX.GetComponent<SfxOnLoad>().Play();

        currentScene.SetActive(false);

        if (MusicController.instance)
            MusicController.instance.Fade(MusicController.FadeMusic.IN);

        CamPos cam = FindObjectOfType(typeof(CamPos)) as CamPos;
        if(cam)
            cam.SetInstance();
    }
}
