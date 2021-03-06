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

    public int type;
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
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position1, radius1, direction1, (distance1 - radius1));
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.gameObject.tag == "WALL")
            {
                MyCore.NetDestroyObject(this.NetId);
                return;
            }
            if (collision.collider.gameObject.GetComponent<PlayerController>())
            {
                if (collision.collider.GetComponent<NetworkID>().Owner != this.gameObject.GetComponent<NetworkID>().Owner)
                {
                    collision.collider.gameObject.GetComponent<PlayerController>().TakeDamage(Damage, Owner);
                    MyCore.NetDestroyObject(this.NetId);
                }
            }
            if (collision.collider.gameObject.GetComponent<EnemyAI>())
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
    public IEnumerator Die()
    {
        yield return new WaitForSeconds(Timer);
        MyAnim.SetBool("DIE", true);
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
            yield return new WaitForSeconds(.05f);
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
