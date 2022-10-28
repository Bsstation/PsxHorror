using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeDoor : InteractibleBase
{
    FadeInOut fade;
    public int doorIndex, sceneToLoad;
    public string sceneString;
    [HideInInspector]
    public int doorIndicatorIndex;

    // Start is called before the first frame update
    new private void Start()
    {
        fade = FindObjectOfType(typeof(FadeInOut)) as FadeInOut;
        base.Start();
    }

    public override void OnInteract()
    {
        fade.StopAllCoroutines();

        if (interact)
            audioSource.PlayOneShot(interact);

        MapController.instance.SetDoor(doorIndicatorIndex, DoorState.Unlocked);
        MapController.instance.SaveDoors();

        SceneController.instance.SaveSceneData();
        SceneController.instance.pos = doorIndex;

        InventoryUI.instance.SaveItems();
        GameManager.instance.player.Save();
        ItemBox.instance.SaveItems();

        if (DoesSceneExist(sceneString))
            StartCoroutine(fade.FadeAndLoadScene(FadeInOut.FadeDirection.In, sceneString));
        else
            StartCoroutine(fade.FadeAndLoadScene(FadeInOut.FadeDirection.In, sceneToLoad));

        var sfx = Resources.Load("FX/FadeSFX") as GameObject;
        GameObject tempSFX = Instantiate(sfx);
        tempSFX.transform.position = transform.position;

        if (MusicController.instance)
            MusicController.instance.Fade(MusicController.FadeMusic.OUT);
    }

    public static bool DoesSceneExist(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var lastSlash = scenePath.LastIndexOf("/");
            var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

            if (string.Compare(name, sceneName, true) == 0)
                return true;
        }

        return false;
    }

    public override void LockedReaction()
    {
        MapController.instance.SetDoor(doorIndicatorIndex, (keyIndex != 0) ? DoorState.Locked : DoorState.ForeverLocked);
    }
}
