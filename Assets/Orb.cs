using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : Projectile
{
    Vector2 position;
    Vector2 direction;
    float radius;
    float distance;
    protected override void Start()
    {
        if (IsServer)
        {
            position = transform.position + (transform.up * -0.3125f);
            direction = this.transform.up;
            radius = 0.1875f;
            distance = 0.625f;

            base.Start();
        }
    }

    protected override void Update()
    {
        if (IsServer)
        {
            base.Update();
            DectectCollisionCircleCast(position, radius, direction, distance);
        }
    }
}
