using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb2 : Projectile
{
    public GameObject OuterRing;

    public float RotationSpeed;

    public bool Outer;

    public Vector2 MoveDir;
    protected override void Start()
    {
        if (IsServer)
        {
            base.Start();
            MoveDir = this.transform.up;
            if (Outer)
            {
                OuterRing.SetActive(true);
            }
            else if (!Outer)
            {
                OuterRing.SetActive(false);
            }
        }
    }

    protected override void Update()
    {
        if (IsServer)
        {
            MyRig.velocity = MoveDir * Speed;

            if (!Outer)
            {
                position = transform.position + (transform.up * -0.25f);
                direction = this.transform.up;
                radius = 0.3125f;
                distance = 0.5f;
            }
            else if (Outer)
            {
                position = transform.position + (transform.up * -0.9375f);
                direction = this.transform.up;
                radius = 0.9375f;
                distance = 1.875f;

                MyRig.rotation += RotationSpeed;
            } 
            DectectCollisionCircleCast(position, radius, direction, distance);
        }
    }

    public void SetOuter(bool value)
    {
        Outer = value;
        SendUpdate("OUTER", Outer.ToString());
    }

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "OUTER" && IsClient)
        {
            Outer = bool.Parse(value);
            if (Outer)
            {
                OuterRing.SetActive(true);
            }
            else if (!Outer)
            {
                OuterRing.SetActive(false);
            }
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer)
            {
                if (IsDirty)
                {
                    SendUpdate("OUTER", Outer.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}
