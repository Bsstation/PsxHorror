using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class InteractibleBase : MonoBehaviour
{
    public int keyIndex;
    public bool locked;
    public Transform angleRef;

    public bool canInteract;

    [HideInInspector]
    public AudioSource audioSource;
    public AudioClip unlock, lockedAudio, interact;

    // Start is called before the first frame update
    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (!locked)
            OnInteract();
        else
        {
            if (keyIndex > 0)
            {
                InventoryUI.instance.UpdateSlots();
                InventoryUI.instance.ShowItems();
                GameManager.instance.gameStatus = GameStatus.Inventory;
                MessagesBehaviour.instance.SendMessageTxt(MessagesBehaviour.instance.itsLocked.msgs[Settings.instance.currentLanguage]);
                GameManager.instance.ChangeSelected(InventoryUI.instance.slots[0].gameObject);
                audioSource.PlayOneShot(lockedAudio);
                LockedReaction();
            }
            else if (keyIndex == 0)
            {
                MessagesBehaviour.instance.SendMessageTxt(MessagesBehaviour.instance.itsStuck.msgs[Settings.instance.currentLanguage]);
                audioSource.PlayOneShot(lockedAudio);
                LockedReaction();
            }
            else
            {
                MessagesBehaviour.instance.SendMessageTxt(MessagesBehaviour.instance.itsLockedlockedOnTheOtherside.msgs[Settings.instance.currentLanguage]);
                audioSource.PlayOneShot(lockedAudio);
                LockedReaction();
            }
        }
    }

    public void Unlock()
    {
        if (locked)
            locked = false;

        if (GetComponent<FadeDoor>())
            MapController.instance.SetDoor(GetComponent<FadeDoor>().doorIndicatorIndex, DoorState.Unlocked);

        audioSource.PlayOneShot(unlock);

        if(GetComponent<TeleportingDoor>())
            MapController.instance.SetDoor(GetComponent<TeleportingDoor>().doorIndicatorIndex, DoorState.Unlocked);
        else if (GetComponent<FadeDoor>())
            MapController.instance.SetDoor(GetComponent<FadeDoor>().doorIndicatorIndex, DoorState.Unlocked);
    }

    public abstract void OnInteract();
    public abstract void LockedReaction();
}
