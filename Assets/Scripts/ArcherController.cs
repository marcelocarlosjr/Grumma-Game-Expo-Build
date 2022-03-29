using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    [Header("ARCHER VARIABLES")]
    public float LFireDamage;
    public float RFireDamage;

    public float RFIRECOUNT = 1;
    public bool RFireTimerDone;
    float RFIRESHOOTSTATE = 4;

    public override void LFire(bool state)
    {
        if (LFireInput && !Dead)
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
        if (RFireInput && !LFireAnimation && !TakingDamage && !Dead)
        {
            if (!RFireCD)
            {
                RFireCD = true;
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
                RFireTimerDone = false;
                RFireAnimation = true;
                STATE = RFIRESTATE;
                for (int i = 0; i <= 17; i++)
                {
                    if (!RFireInput)
                    {
                        if (RFIRECOUNT < 3)
                        {
                            MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                            yield break;
                        }
                    }
                    yield return new WaitForSeconds(.2f);
                    RFIRECOUNT += .4f;
                    if (!RFireInput)
                    {
                        if (RFIRECOUNT >= 1)
                        {
                            MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                            if (RFIRECOUNT >= 3)
                            {
                                MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.4f) + (transform.up * -0.15f)), Quaternion.LookRotation(transform.forward, transform.up));
                                MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.4f) + (transform.up * -0.15f)), Quaternion.LookRotation(transform.forward, transform.up));
                                if (RFIRECOUNT >= 5)
                                {
                                    MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.8f) + (transform.up * -0.3f)), Quaternion.LookRotation(transform.forward, transform.up));
                                    MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.8f) + (transform.up * -0.3f)), Quaternion.LookRotation(transform.forward, transform.up));
                                }
                            }
                        }
                        RFireTimerDone = true;
                        StartCoroutine(RFireAnim());
                        RFIRECOUNT = 1;
                        yield break;
                    }
                }
                yield return new WaitUntil(() => !RFireInput);
                RFireTimerDone = true;
                if(RFIRECOUNT >= 1)
                {
                    MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                    if (RFIRECOUNT >= 3)
                    {
                        MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.2f) + (transform.up * -0.1f)), Quaternion.LookRotation(transform.forward, transform.up));
                        MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.2f) + (transform.up * -0.1f)), Quaternion.LookRotation(transform.forward, transform.up));
                        if (RFIRECOUNT == 5)
                        {
                            MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.4f) + (transform.up * -0.2f)), Quaternion.LookRotation(transform.forward, transform.up));
                            MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.4f) + (transform.up * -0.2f)), Quaternion.LookRotation(transform.forward, transform.up));
                        }
                    }
                }
                STATE = RFIRESHOOTSTATE;
                StartCoroutine(RFireAnim());
                RFIRECOUNT = 1;
                yield break;
            }
        }
    }

    public IEnumerator RFireAnim()
    {
        RFireRaycast();
        STATE = RFIRESHOOTSTATE;
        yield return new WaitForSeconds(.252f);
        RFireAnimation = false;
        RFireCD = false;
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
