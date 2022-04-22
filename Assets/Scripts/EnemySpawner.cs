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
            if (!LinkedEnemy)
            {
                if (!CheckingForPlayer && !Spawning)
                {
                    StartCoroutine(CheckPlayerProximity());
                }
            }

            if (LinkedEnemy)
            {
                if (!CheckingForPlayersD && !Despawning)
                {
                    StartCoroutine(CheckPlayerProximityD());
                }
            }
        }
    }
    bool Spawning;
    public IEnumerator SpawnEnemy()
    {
        Spawning = true;
        FindObjectOfType<NetworkCore>().NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(RespawnTimer);
        Spawning = false;
    }
    bool CheckingForPlayer;
    public IEnumerator CheckPlayerProximity()
    {
        CheckingForPlayer = true;
        foreach(PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if(Vector3.Distance(this.transform.position, pc.transform.position) < 15)
            {
                if (!Spawning)
                {
                    StartCoroutine(SpawnEnemy());
                }
            }
        }
        yield return new WaitForSeconds(2);
        CheckingForPlayer = false;
    }
    bool Despawning;
    bool CheckingForPlayersD;
    public IEnumerator DespawnEnemy()
    {
        Despawning = true;
        yield return new WaitForSeconds(RespawnTimer);
        FindObjectOfType<NetworkCore>().NetDestroyObject(LinkedEnemy.GetComponent<NetworkID>().NetId);
        Despawning = false;
    }
    public IEnumerator CheckPlayerProximityD()
    {
        CheckingForPlayersD = true;
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if (Vector3.Distance(LinkedEnemy.transform.position, pc.transform.position) > 15)
            {
                if (!Despawning)
                {
                    StartCoroutine(SpawnEnemy());
                }
            }
        }
        yield return new WaitForSeconds(2);
        CheckingForPlayersD = false;
    }


    private void Start()
    {
        StartCoroutine(GetIsServer());
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
