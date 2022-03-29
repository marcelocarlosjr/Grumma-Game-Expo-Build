using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorController : PlayerController
{
    [Header("WARRIOR VARIABLES")]
    public float LFireDamage;
    public float RFireDamage;
    public float RFireTimer;
    public bool RFireTimerDone;

    protected float RFIRESLASHSTATE = 4;

    public override void HandleMessage(string flag, string value)
    {
        base.HandleMessage(flag, value);
        if (flag == "STATE" && IsClient)
        {
            SlashAnim.SetInteger("SLASH", (int)STATE);
        }
    }
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
                yield return new WaitForSeconds(.35f);
                LFireCD = false;
            }
        }
    }
    public IEnumerator LFireAnim()
    {
        LFireAnimation = true;
        STATE = LFIRESTATE;
        LFireRaycast();
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
                for(int i = 0; i <= 5; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    RFireTimer += 0.2f;
                    if (!RFireInput)
                    {
                        RFireTimerDone = true;
                        StartCoroutine(RFireAnim());
                        RFireTimer = 0;
                        yield break;
                    }
                }
                yield return new WaitUntil(() => !RFireInput);
                RFireTimerDone = true;
                STATE = RFIRESLASHSTATE;
                StartCoroutine(RFireAnim());
                RFireTimer = 0;
                yield break;
            }
        }
    }

    public IEnumerator RFireAnim()
    {
        RFireRaycast();
        STATE = RFIRESLASHSTATE;
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
            if(collision.collider.gameObject != this.gameObject)
            {
                if (collision.collider.gameObject.GetComponent<PlayerController>())
                {
                    collision.collider.gameObject.GetComponent<PlayerController>().TakeDamage(RFireDamage * (RFireTimer + 1));
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
    public override void Start()
    {
        base.Start();
        SlashAnim = this.transform.GetChild(0).GetComponent<Animator>();
        if (SlashAnim == null)
        {
            throw new System.Exception("ERROR: Could not find Slash Animator!");
        }
    }
}
