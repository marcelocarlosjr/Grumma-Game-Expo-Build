using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Item : NetworkComponent
{
    public ItemObject item;

    public bool move;
    Vector3 location;
    Rigidbody2D MyRig;

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
            if (IsServer)
            {
                if (IsDirty)
                {
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ThrowItem(Vector3 _location)
    {
        move = true;
        location = _location;
    }

    private void Update()
    {
        if (move && IsServer)
        {
            if(Vector3.Distance(location, MyRig.transform.position) < 1f)
            {
                MyRig.velocity = Vector3.zero;
                MyRig.rotation = 0;
                move = false;
            }
            else
            {
                MyRig.velocity = Vector3.Lerp(MyRig.velocity, (location - MyRig.transform.position).normalized * 3, 0.1f);
            }
        }
    }
    private void Start()
    {
        MyRig = GetComponent<Rigidbody2D>();
        if (IsServer)
        {
            Invoke("Despawn", 180);
        }
    }

    public void Despawn()
    {
        MyCore.NetDestroyObject(this.NetId);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (move && IsServer)
        {
            if (collision.gameObject.tag == "WALL" || collision.gameObject.tag == "NODE")
            {
                MyRig.velocity = Vector3.zero;
                MyRig.rotation = 0;
                move = false;
            }
        }
    }
}
