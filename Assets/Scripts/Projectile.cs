using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Projectile : NetworkComponent
{
    protected Rigidbody2D MyRig;

    public float Damage;
    public float Speed;
    public float Timer;

    protected virtual void Start()
    {
        if (IsServer)
        {
            MyRig = GetComponent<Rigidbody2D>();
            Invoke("DestroyTimer", Timer);
        }
    }
    protected virtual void Update()
    {
        if (IsServer)
        {
            MyRig.velocity = transform.up * Speed;
        }
    }

    public void DectectCollisionCircleCast(Vector2 position, float radius, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position, radius, direction, distance);
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.GetComponent<NetworkID>().Owner != this.gameObject.GetComponent<NetworkID>().Owner)
            {
                if (collision.collider.gameObject.GetComponent<PlayerController>())
                {
                    collision.collider.gameObject.GetComponent<PlayerController>().TakeDamage(Damage);
                    MyCore.NetDestroyObject(this.NetId);
                }
            }
        }
    }

    public void DestroyTimer()
    {
        MyCore.NetDestroyObject(this.NetId);
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            yield return new WaitForSeconds(1);
        }
    }

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }
}
