using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class EnemySpawner : MonoBehaviour
{
    public int SpawnPrefab;
    public float RespawnTimer;

    public GameObject LinkedEnemy;

    bool Timer;

    bool IsServer;

    bool CheckPlayer;


    private void Update()
    {
        if (IsServer)
        {
            if (!CheckPlayer && !LinkedEnemy)
            {
                StartCoroutine(CheckPlayerTimer());
            }

            if (!Despawning && LinkedEnemy)
            {
                DespawnEnemy();
            }
        }
    }
    bool Despawn;
    public IEnumerator CheckPlayerTimer()
    {
        CheckPlayer = true;
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (Vector3.Distance(this.transform.position, pc.transform.position) < 15)
            {
                if (!Timer)
                {
                    SpawnTimer();
                }
            }
        }
        yield return new WaitForSeconds(2);
        CheckPlayer = false;
    }

    private void Start()
    {
        StartCoroutine(GetIsServer());
    }

    bool Despawning;

    public IEnumerator DespawnEnemy()
    {
        Despawning = true;
        yield return new WaitForSeconds(10);
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (Vector3.Distance(LinkedEnemy.transform.position, pc.transform.position) > 15)
            {
                FindObjectOfType<NetworkCore>().NetDestroyObject(LinkedEnemy.GetComponent<NetworkID>().NetId);
            }
        }
        Despawning = false;
    }

    public IEnumerator SpawnTimer()
    {
        Timer = true;
        LinkedEnemy = FindObjectOfType<NetworkCore>().NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(RespawnTimer);
        Timer = false;
    }

    public IEnumerator GetIsServer()
    {
        yield return new WaitUntil(() => FindObjectOfType<NetworkCore>().IsConnected);
        if (FindObjectOfType<NetworkCore>().IsServer)
        {
            IsServer = true;
        }
        else
        {
            IsServer = false;
        }
    }
}
