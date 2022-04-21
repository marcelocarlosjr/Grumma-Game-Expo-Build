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

    private void Update()
    {
        if (IsServer)
        {
            if(LinkedEnemy == null)
            {
                if (!Timer)
                {
                    StartCoroutine(SpawnTimer());
                }
            }
        }
    }

    private void Start()
    {
        StartCoroutine(GetIsServer());
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
