using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Analising : InteractibleBase
{
    public string[] analise;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        locked = false;
        audioSource.mute = true;
    }

    public override void LockedReaction()
    {
        
    }

    public override void OnInteract()
    {
        MessagesBehaviour.instance.SendMessageTxt(analise[Settings.instance.currentLanguage]);
    }
}
