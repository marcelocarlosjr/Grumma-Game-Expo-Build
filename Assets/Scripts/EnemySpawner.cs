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

    private void Update()
    {
        if (FindObjectOfType<NetworkCore>() && FindObjectOfType<NetworkCore>().IsServer)
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

    public IEnumerator SpawnTimer()
    {
        Timer = true;
        yield return new WaitForSeconds(RespawnTimer);
        LinkedEnemy = FindObjectOfType<NetworkCore>().NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
        Timer = false;

    }
}
