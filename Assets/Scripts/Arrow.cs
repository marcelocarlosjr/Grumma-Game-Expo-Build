using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile
{
    protected override void Start()
    {
        if (IsServer)
        {
            base.Start();
        }
        else
        {
            //test
            //FindObjectOfType<AudioManager>().Play("ArcherA");
        }
    }

    protected override void Update()
    {
        if (IsServer)
        {
            position = transform.position + (transform.up * -0.375f);
            direction = this.transform.up;
            radius = 0.1875f;
            distance = 0.6875f;
            base.Update();
            DectectCollisionCircleCast(position, radius, direction, distance);
        }
    }
}
