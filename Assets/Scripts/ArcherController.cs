using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    [Header("ARCHER VARIABLES")]
    public float LFireDamage;
    public float RFireDamage;

    public override void LFire(bool state)
    {
        if (LFireInput)
        {
            if (!LFireCD)
            {
                StartCoroutine(LFire());
            }
        }
    }
    public override IEnumerator LFire()
    {
        if (IsServer)
        {
            while (LFireInput && !RFireAnimation && !TakingDamage)
            {
                LFireCD = true;
                StartCoroutine(LFireAnim());
                MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                yield return new WaitForSeconds(1);
                LFireCD = false;

            }
        }
    }
    public IEnumerator LFireAnim()
    {
        LFireAnimation = true;
        STATE = LFIRESTATE;
        yield return new WaitForSeconds(.15f);
        LFireAnimation = false;
    }

    public override void RFire(bool state)
    {
        if (RFireInput && !LFireAnimation && !TakingDamage)
        {
            if (!RFireCD)
            {

                StartCoroutine(RFire());
            }
        }
        else
        {
            StopCoroutine(RFire());
        }
    }

    public override IEnumerator RFire()
    {
        if (IsServer)
        {
            while (RFireInput)
            {


                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public IEnumerator RFireAnim()
    {
        RFireRaycast();

        yield return new WaitForSeconds(.252f);

        yield break;
    }

    public void RFireRaycast()
    {
        Vector2 position = transform.position;
        Vector2 direction = this.transform.up;
        float radius = .4f;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(position, radius, direction, 1.7f);
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.gameObject != this.gameObject)
            {
                if (collision.collider.gameObject.GetComponent<PlayerController>())
                {
                    
                }
            }
        }
    }
    public void LFireRaycast()
    {
        Vector2 position = transform.position;
        Vector2 direction = this.transform.up;
        float radius = .4f;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(position, radius, direction, .5f);
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.gameObject != this.gameObject)
            {
                if (collision.collider.gameObject.GetComponent<PlayerController>())
                {
                    collision.collider.gameObject.GetComponent<PlayerController>().TakeDamage(LFireDamage);
                }
            }
        }
    }
}
