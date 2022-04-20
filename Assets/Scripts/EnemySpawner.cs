using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class EnemySpawner : NetworkComponent
{
    public int SpawnPrefab;
    public float RespawnTimer;

    public GameObject LinkedEnemy;

    bool Timer;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            LinkedEnemy = MyCore.NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }

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

    public IEnumerator SpawnTimer()
    {
        Timer = true;
        yield return new WaitForSeconds(RespawnTimer);
        LinkedEnemy = MyCore.NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
        Timer = false;

    }
}
