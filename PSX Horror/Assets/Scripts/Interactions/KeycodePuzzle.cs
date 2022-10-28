using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class KeycodePuzzle : InteractibleBase
{
    public UnityEvent whenSolve;
    public string password;
    public string currentPass;
    public GameObject puzzleScreen;
    public GameObject firstButtonSelected;

    public Text currentPassText;

    public Transform crank;
    public Vector3 crankRot;

    new void Start()
    {
        base.Start();

        puzzleScreen.transform.parent = GameManager.instance.puzzles.transform;

        puzzleScreen.GetComponent<RectTransform>().offsetMin = Vector3.zero;
        puzzleScreen.GetComponent<RectTransform>().offsetMax = Vector3.zero;
        puzzleScreen.GetComponent<RectTransform>().localRotation = Quaternion.Euler(Vector3.zero);
        puzzleScreen.GetComponent<RectTransform>().localScale = Vector3.one;

        puzzleScreen.SetActive(false);

        if (!canInteract)
        {
            if (crank)
                crank.localRotation = Quaternion.Euler(crankRot);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentPassText.text = currentPass;
    }

    public override void OnInteract()
    {
        if (interact)
            audioSource.PlayOneShot(interact);
        GameManager.instance.gameStatus = GameStatus.Puzzle;
        puzzleScreen.SetActive(true);
        GameManager.instance.ChangeSelected(firstButtonSelected);
    }

    public override void LockedReaction()
    {
        
    }

    public void InsertNumber(int pass)
    {
        currentPass = currentPass + pass;
        InventoryUI.instance.PlayMoveAudio();
    }

    public void Submit()
    {
        if(currentPass == password)
        {
            puzzleScreen.SetActive(false);
            canInteract = false;
            whenSolve.Invoke();
            //GameManager.instance.gameStatus = GameStatus.Game;
            InventoryUI.instance.PlayAcceptAudio();
        }
        else
        {
            print("errado");
            InventoryUI.instance.PlayErrorAudio();
        }
    }

    public void Clear()
    {
        currentPass = "";
        InventoryUI.instance.PlayMoveAudio();
    }
}
