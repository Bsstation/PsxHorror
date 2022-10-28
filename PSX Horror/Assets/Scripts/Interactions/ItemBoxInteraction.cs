using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxInteraction : InteractibleBase
{
    public override void LockedReaction()
    {
        
    }

    public override void OnInteract()
    {
        if (interact)
            audioSource.PlayOneShot(interact);
        GameManager.instance.ChangeToItemBox();
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        locked = false;
    }
}