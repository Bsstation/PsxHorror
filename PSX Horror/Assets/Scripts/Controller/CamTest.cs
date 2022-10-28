using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamTest : MonoBehaviour
{
    GameManager gameManager;
    public Transform player;
    CinemachineFreeLook freeLook;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
        freeLook = GetComponent<CinemachineFreeLook>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        freeLook.m_Follow = freeLook.m_LookAt = player;
    }

    // Update is called once per frame
    void Update()
    {
        freeLook.m_XAxis.m_MaxSpeed = (gameManager.gameStatus == GameStatus.Game) ? 300 : 0;
        freeLook.m_YAxis.m_MaxSpeed = (gameManager.gameStatus == GameStatus.Game) ? 2 : 0;
    }
}
