using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;

public class EnemyAI : NetworkComponent
{
    public NavMeshAgent MyAgent;
    public Rigidbody2D MyRig;

    float rotationAngle;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        if (IsClient)
        {
            MyAgent.enabled = false;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Start()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyAgent.updateRotation = false;
        MyAgent.updateUpAxis = false;

        MyRig = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsServer)
        {
            if(FindObjectsOfType<PlayerController>() != null)
            {
                foreach (PlayerController p in FindObjectsOfType<PlayerController>())
                {
                    if (Vector2.Distance(this.transform.position, p.transform.position) < 6)
                    {
                        Vector2 position = transform.position + (transform.up * 1.1875f);
                        Vector2 direction = this.transform.up;
                        float radius = 0.4375f;
                        float distance = 0.8f;

                        DectectCollisionCircleCast(position, radius, direction, distance);

                        MyAgent.SetDestination(p.gameObject.transform.position);
                        Vector2 relative = (p.transform.position - this.transform.position);

                        rotationAngle = (Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg) - 90;
                    }

                }
            }
            MyRig.velocity = MyAgent.velocity;
            MyRig.rotation = Mathf.LerpAngle(MyRig.rotation, rotationAngle, 2);
        }
    }

     public void DectectCollisionCircleCast(Vector2 position1, float radius1, Vector2 direction1, float distance1)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position1, radius1, direction1, (distance1 - radius1));
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.gameObject.GetComponent<PlayerController>())
            {
                MyAgent.isStopped = true;
            }
            else
            {
                MyAgent.isStopped = false;
            }
            
        }
    }
}
