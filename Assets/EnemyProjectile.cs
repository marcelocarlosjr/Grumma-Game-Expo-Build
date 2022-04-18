using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class EnemyProjectile : NetworkComponent
{
    float Damage;
    float Speed;
    public Rigidbody2D MyRig;

    Vector3 position;
    public float PositionMod;
    Vector3 direction;
    public float radius;
    public float distance;

    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            MyRig.velocity = transform.up * Speed;

            position = transform.position + (transform.up * PositionMod);
            direction = this.transform.up;
            radius = 0.1875f;
            distance = 0.6875f;
            DectectCollisionCircleCast(position, radius, direction, distance);
        }
    }

    public void SetData(float _speed, float _damage)
    {
        Speed = _speed;
        Damage = _damage;
    }

    public void DectectCollisionCircleCast(Vector2 position1, float radius1, Vector2 direction1, float distance1)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position1, radius1, direction1, (distance1 - radius1));
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.gameObject.GetComponent<PlayerController>())
            {
                if (collision.collider.GetComponent<NetworkID>().Owner != this.gameObject.GetComponent<NetworkID>().Owner)
                {
                    collision.collider.gameObject.GetComponent<PlayerController>().TakeDamage(Damage);
                    MyCore.NetDestroyObject(this.NetId);
                }
            }
            if (collision.collider.gameObject.GetComponent<EnemyAI>())
            {
                if (collision.collider.GetComponent<NetworkID>().Owner != this.gameObject.GetComponent<NetworkID>().Owner)
                {
                    collision.collider.gameObject.GetComponent<EnemyAI>().TakeDamage(this.Owner, Damage);
                    foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
                    {
                        if (pc.Owner == this.Owner)
                        {
                            pc.GetLastEnemy(collision.collider.GetComponent<NetworkID>().NetId);
                        }
                    }
                    MyCore.NetDestroyObject(this.NetId);
                }
            }
        }
    }
}
