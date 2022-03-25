using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : PlayerController
{
    [Header("WARRIOR VARIABLES")]
    public BoxCollider2D SlashCollider;
    public BoxCollider2D SlashCollider2;
    public float LFireDamage;
    public float RFireDamage;
    public float RFireTimer;
    public bool RFireTimerDone;

    protected float RFIRESLASHSTATE = 4;
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
            while (LFireInput)
            {
                LFireCD = true;
                StartCoroutine(LFireAnim());
                //spawn slash animation
                yield return new WaitForSeconds(.35f);
                LFireCD = false;
            }
        }
    }
    public IEnumerator LFireAnim()
    {
        SlashCollider.enabled = true;
        LFireAnimation = true;
        STATE = LFIRESTATE;
        yield return new WaitForSeconds(.15f);
        SlashCollider.enabled = false;
        LFireAnimation = false;
    }

    public override void RFire(bool state)
    {
        if (RFireInput)
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
                for(int i = 0; i <= 5; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    RFireTimer += 0.5f;
                    if (!RFireInput)
                    {
                        RFireTimerDone = true;
                        RFireCD = false;
                        StartCoroutine(RFireAnim(RFireTimer));
                        RFireTimer = 0;
                        yield break;
                    }
                }
                yield return new WaitUntil(() => !RFireInput);
                RFireTimerDone = true;
                STATE = RFIRESLASHSTATE;
                yield return new WaitForSeconds(.3f);
                RFireCD = false;
                RFireTimer = 0;
            }
        }
    }

    public IEnumerator RFireAnim(float time)
    {
        //damage = damage * time;
        SlashCollider2.enabled = true;
        STATE = RFIRESLASHSTATE;
        yield return new WaitForSeconds(.3f);
        SlashCollider2.enabled = false;
        RFireAnimation = false;
    }
}
