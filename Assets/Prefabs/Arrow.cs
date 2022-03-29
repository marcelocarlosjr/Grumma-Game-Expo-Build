using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    protected override void Start()
    {
        base.Start();

        if (IsServer)
        {
            position = transform.position + (transform.up * -0.375f);
            direction = this.transform.up;
            radius = 0.1875f;
            distance = 0.6875f;
        }
    }

    protected override void Update()
    {
        base.Update();
        DectectCollisionCircleCast();
    }
}
