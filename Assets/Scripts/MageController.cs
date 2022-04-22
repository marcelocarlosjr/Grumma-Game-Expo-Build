using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageController : PlayerController
{
    [Header("MAGE VARIABLES")]
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
            while (LFireInput && !RFireAnimation && !TakingDamage && !Dead)
            {
                LFireCD = true;
                StartCoroutine(LFireAnim());
                var temp = MyCore.NetCreateObject(4, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                temp.GetComponent<Projectile>().Damage = Damage;
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
            while (RFireInput && !Dead)
            {
                RFireTimerDone = false;
                RFireAnimation = true;
                STATE = RFIRESTATE;
                for (int i = 0; i <= 17; i++)
                {
                    if (!RFireInput)
                    {
                        RFireTimerDone = true;

                        GameObject temp = MyCore.NetCreateObject(5, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                        temp.GetComponent<Projectile>().Damage = Damage + ((RFIRECOUNT / 2) * 2);
                        if (RFIRECOUNT >= 5)
                        {
                            temp.GetComponent<Orb2>().SetOuter(true);
                        }
                        else
                        {
                            temp.GetComponent<Orb2>().SetOuter(false);
                        }

                        STATE = RFIRESHOOTSTATE;
                        RFIRECOUNT = 1;
                        yield return new WaitForSeconds(0.25f);
                        RFireAnimation = false;
                        RFireCD = false;
                        yield break;
                    }
                    yield return new WaitForSeconds(.2f);
                    RFIRECOUNT += .4f;
                    if (!RFireInput)
                    {
                        RFireTimerDone = true;

                        GameObject temp1 = MyCore.NetCreateObject(5, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                        temp1.GetComponent<Projectile>().Damage = Damage + ((RFIRECOUNT / 2) * 2);
                        if (RFIRECOUNT >= 5)
                        {
                            temp1.GetComponent<Orb2>().SetOuter(true);
                        }
                        else
                        {
                            temp1.GetComponent<Orb2>().SetOuter(false);
                        }

                        STATE = RFIRESHOOTSTATE;
                        RFIRECOUNT = 1;
                        yield return new WaitForSeconds(0.25f);
                        RFireAnimation = false;
                        RFireCD = false;
                        yield break;
                    }
                }
                yield return new WaitUntil(() => !RFireInput);
                RFireTimerDone = true;

                GameObject temp2 = MyCore.NetCreateObject(5, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                temp2.GetComponent<Projectile>().Damage = Damage + ((RFIRECOUNT/2) * 2);
                if (RFIRECOUNT >= 5)
                {
                    temp2.GetComponent<Orb2>().SetOuter(true);
                }
                else
                {
                    temp2.GetComponent<Orb2>().SetOuter(false);
                }
                STATE = RFIRESHOOTSTATE;
                RFIRECOUNT = 1;
                yield return new WaitForSeconds(0.25f);
                RFireAnimation = false;
                RFireCD = false;
                yield break;
            }
        }
    }
}
