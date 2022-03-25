using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : PlayerController
{
    [Header("WARRIOR VARIABLES")]
    public BoxCollider2D SlashCollider;
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
    public override IEnumerator LFireAnim()
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
                StartCoroutine(RFire());
            }
        }
    }

    public override IEnumerator RFire()
    {
        if (IsServer)
        {
            while (RFireInput)
            {
                RFireCD = true;
                StartCoroutine(LFireAnim());
                //spawn slash animation
                yield return new WaitForSeconds(.35f);
                RFireCD = false;
            }
        }
    }

    public override IEnumerator RFireAnim()
    {
        RFireAnimation = true;
        STATE = RFIRESTATE;
        yield return new WaitForSeconds(.15f);
        RFireAnimation = false;
    }
}
