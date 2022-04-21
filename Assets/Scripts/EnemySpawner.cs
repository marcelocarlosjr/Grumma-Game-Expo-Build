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
            if (!CheckPlayer)
            {
                StartCoroutine(CheckPlayerTimer());
            }
        }
    }
    Coroutine Despawn;
    public IEnumerator CheckPlayerTimer()
    {
        CheckPlayer = true;
        foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if(Vector3.Distance(this.transform.position, pc.transform.position) < 15)
            {
                if (Despawn != null)
                {
                    StopCoroutine(Despawn);
                }
                if (LinkedEnemy == null)
                {
                    if (!Timer)
                    {
                        StartCoroutine(SpawnTimer());
                    }
                }
            }
            else
            {
                if (LinkedEnemy)
                {
                    Despawn = StartCoroutine(DespawnEnemy());
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

    public IEnumerator DespawnEnemy()
    {
        yield return new WaitForSeconds(10);
        FindObjectOfType<NetworkCore>().NetDestroyObject(LinkedEnemy.GetComponent<NetworkID>().NetId);
    }

    public IEnumerator SpawnTimer()
    {
        Timer = true;
        yield return new WaitForSeconds(RespawnTimer);
        LinkedEnemy = FindObjectOfType<NetworkCore>().NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
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
