using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesSpawner : MonoBehaviour
{
    GameManager gameManager;
    public GameObject enemy;
    public bool random;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager && gameManager.gameStatus == GameStatus.Game)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        RagdollEnemy ragdoll = Instantiate(enemy, transform.position, Quaternion.identity).GetComponentInChildren<RagdollEnemy>();

        if (random)
        {
            int range = Random.Range(0, 4);
            ragdoll.weapons.typeEnemy = (TypeEnemy)range;
        }

        gameManager.player.SearchEnemiesInScene();
    }
}