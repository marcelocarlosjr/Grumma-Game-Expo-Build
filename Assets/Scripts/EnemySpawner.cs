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

    Transform nearestPlayer;

   

    private void Update()
    {
        if (IsServer)
        {
            if (!detecting)
            {
                StartCoroutine(EnemyFindClosestPlayer());
            }
        }
    }
    bool detecting;
    public IEnumerator EnemyFindClosestPlayer()
    {
        detecting = true;
        if (LinkedEnemy)
        {
            if (!FindObjectOfType<PlayerController>())
            {
                FindObjectOfType<NetworkCore>().NetDestroyObject(LinkedEnemy.GetComponent<NetworkID>().NetId);
                yield return new WaitForSeconds(2);
                detecting = false;
                yield break;
            }
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                float minimumDistance = Mathf.Infinity;
                float distance = Vector3.Distance(LinkedEnemy.transform.position, pc.transform.position);
                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                    nearestPlayer = pc.transform;
                }
            }
            if (Vector3.Distance(nearestPlayer.position, LinkedEnemy.transform.position) > 15)
            {
                if (!Despawning)
                {
                    StartCoroutine(Despawn());
                }
            }
        }
        if (!LinkedEnemy)
        {
            if (!FindObjectOfType<PlayerController>())
            {
                yield return new WaitForSeconds(2);
                detecting = false;
                yield break;
            }
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                float minimumDistance = Mathf.Infinity;
                float distance = Vector3.Distance(this.transform.position, pc.transform.position);
                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                    nearestPlayer = pc.transform;
                }
            }
            if (Vector3.Distance(nearestPlayer.position, this.transform.position) < 15)
            {
                if (!Spawning)
                {
                    StartCoroutine(Spawn());
                }
            }
        }
        yield return new WaitForSeconds(2);
        detecting = false;
    }

    bool Despawning;
    bool Spawning;
    public IEnumerator Despawn()
    {
        Despawning = true;
        yield return new WaitForSeconds(7);
        if((Vector3.Distance(nearestPlayer.position, LinkedEnemy.transform.position) > 15))
        {
            FindObjectOfType<NetworkCore>().NetDestroyObject(LinkedEnemy.GetComponent<NetworkID>().NetId);
        }
        Despawning = false;
    }

    public IEnumerator Spawn()
    {
        Spawning = true;
        LinkedEnemy = FindObjectOfType<NetworkCore>().NetCreateObject(SpawnPrefab, -1, this.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(RespawnTimer);
        Spawning = false;
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
