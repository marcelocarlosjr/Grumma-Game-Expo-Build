using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Projectile : NetworkComponent
{
    protected Rigidbody2D MyRig;
    public Animator MyAnim;

    public Vector2 position;
    public Vector2 direction;
    public float radius;
    public float distance;

    public float Damage;
    public float Speed;
    public float Timer;

    protected virtual void Start()
    {
        if (IsServer)
        {
            MyRig = GetComponent<Rigidbody2D>();
            StartCoroutine(Die());
        }
    }
    protected virtual void Update()
    {
        if (IsServer)
        {
            MyRig.velocity = transform.up * Speed;
        }
    }

    public void DectectCollisionCircleCast(Vector2 position1, float radius1, Vector2 direction1, float distance1)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position1, radius1, direction1, distance1);
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
    public IEnumerator Die()
    {
        yield return new WaitForSeconds(Timer);
        MyAnim.SetBool("DIE", true);
        Debug.Log("breh");
        yield return new WaitForSeconds(0.34f);
        if (IsServer)
        {
            MyCore.NetDestroyObject(this.NetId);
        }
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
        MyAnim = GetComponent<Animator>();
        if (IsClient)
        {
            StartCoroutine(Die());
        }
    }
}
