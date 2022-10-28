using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : InteractibleBase
{
    public string[] sceneName;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        locked = false;
    }

    public override void LockedReaction()
    {
        
    }

    public override void OnInteract()
    {
        if (interact)
            audioSource.PlayOneShot(interact);
        if (!ScreenShotBehaviour.instance)
        {
            var cam = Resources.Load("Init/ScreenshotCam") as GameObject;
            ScreenShotBehaviour screenShot = Instantiate(cam).GetComponent<ScreenShotBehaviour>();
        }

        ScreenShotBehaviour.instance.transform.position = Camera.main.transform.position;
        ScreenShotBehaviour.instance.transform.rotation = Camera.main.transform.rotation;

        ScreenShotBehaviour.instance.PrepareScreenshot(Screen.width, Screen.height);
        ScreenShotBehaviour.instance.sceneName = sceneName;

        GameManager.instance.gameStatus = GameStatus.SaveGame;
        GameManager.instance.ChangeSelected(UIController.instance.buttons.firstSlot);
    }
}
