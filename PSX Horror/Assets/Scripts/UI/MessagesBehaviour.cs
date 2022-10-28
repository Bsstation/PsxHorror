using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct MsgTxt
{
    [TextArea(0, 10)]
    public string[] msgs;
}

public class MessagesBehaviour : MonoBehaviour
{
    public static MessagesBehaviour instance;

    [Header("Universal Messages")]
    public MsgTxt itsLocked;
    public MsgTxt youUsedThe;
    //Stuck = emperrada
    public MsgTxt itsStuck;
    public MsgTxt itsLockedlockedOnTheOtherside;
    public MsgTxt youTakeMap;
    public MsgTxt youTakeFile;
    public MsgTxt youUnlocked;
    public MsgTxt youUpgradeInventory;
    public MsgTxt cannotDiscard;

    [Space]
    public Text messageText;

    public bool examing, typewriting;

    [TextArea(1, 10)]
    string tempMessage;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        messageText.enabled = examing;
    }

    public void ClearMessage()
    {
        if (examing && !typewriting)
        {
            examing = false;
        }
        else if (examing && typewriting)
        {
            StopAllCoroutines();
            messageText.text = tempMessage;
            typewriting = false;
        }
    }

    public void AddRich(string message, string pre, string pos)
    {
        tempMessage = tempMessage + pre + message + pos;
        StartCoroutine(AddRichtext(message, pre, pos));
        examing = true;
    }

    public void AddRich(string message)
    {
        tempMessage = tempMessage + message;
        StartCoroutine(AddRichtext(message, "", ""));
        examing = true;
    }

    public void SendMessageTxt(string message)
    {
        examing = true;
        StopAllCoroutines();
        tempMessage = message;
        StartCoroutine(ShowText(message));
    }

    IEnumerator AddRichtext(string message, string pre, string pos)
    {
        while (GameManager.instance.gameStatus != GameStatus.Game) yield return null;
        while (typewriting) yield return null;

        typewriting = true;
        for (int i = 0; i < message.Length; i++)
        {
            messageText.text = messageText.text + pre + message[i] + pos;

            yield return new WaitForSecondsRealtime(0.05f);
        }
        typewriting = false;
    }

    IEnumerator ShowText(string message)
    {
        while (GameManager.instance.gameStatus != GameStatus.Game) yield return null;

        messageText.text = "";

        typewriting = true;
        for (int i = 0; i < message.Length; i++)
        {
            messageText.text = messageText.text + message[i];

            yield return new WaitForSecondsRealtime(0.05f);
        }
        typewriting = false;
    }
}
