using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : Projectile
{
    protected override void Start()
    {
        base.Start();
        if (IsServer)
        {
            position = transform.position + (transform.up * -0.3125f);
            direction = this.transform.up;
            radius = 0.1875f;
            distance = 0.625f;
        }
    }

    protected override void Update()
    {
        base.Update();
        DectectCollisionCircleCast();
    }
}
