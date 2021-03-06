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
            while (LFireInput && !RFireAnimation && !TakingDamage && !Dead)
            {
                LFireCD = true;
                StartCoroutine(LFireAnim());
                var temp = MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
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
                FindObjectOfType<AudioManager>().Play("ArcherS");
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
                        if (RFIRECOUNT < 3)
                        {
                            var temp = MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                            temp.GetComponent<Projectile>().Damage = Damage;
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
                        if (RFIRECOUNT >= 1)
                        {
                            var temp = MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                            temp.GetComponent<Projectile>().Damage = Damage;
                            if (RFIRECOUNT >= 3)
                            {
                                var temp1 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.4f) + (transform.up * -0.15f)), Quaternion.LookRotation(transform.forward, transform.up));
                                temp1.GetComponent<Projectile>().Damage = Damage;
                                var temp2 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.4f) + (transform.up * -0.15f)), Quaternion.LookRotation(transform.forward, transform.up));
                                temp2.GetComponent<Projectile>().Damage = Damage;
                                if (RFIRECOUNT >= 5)
                                {
                                    var temp3 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.8f) + (transform.up * -0.3f)), Quaternion.LookRotation(transform.forward, transform.up));
                                    temp3.GetComponent<Projectile>().Damage = Damage;
                                    var temp4 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.8f) + (transform.up * -0.3f)), Quaternion.LookRotation(transform.forward, transform.up));
                                    temp4.GetComponent<Projectile>().Damage = Damage;
                                }
                            }
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
                if (RFIRECOUNT >= 1)
                {
                    var temp = MyCore.NetCreateObject(3, this.Owner, this.transform.position, Quaternion.LookRotation(transform.forward, transform.up));
                    temp.GetComponent<Projectile>().Damage = Damage;
                    if (RFIRECOUNT >= 3)
                    {
                        var temp1 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.4f) + (transform.up * -0.15f)), Quaternion.LookRotation(transform.forward, transform.up));
                        temp1.GetComponent<Projectile>().Damage = Damage;
                        var temp2 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.4f) + (transform.up * -0.15f)), Quaternion.LookRotation(transform.forward, transform.up));
                        temp2.GetComponent<Projectile>().Damage = Damage;
                        if (RFIRECOUNT >= 5)
                        {
                            var temp3 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * 0.8f) + (transform.up * -0.3f)), Quaternion.LookRotation(transform.forward, transform.up));
                            temp3.GetComponent<Projectile>().Damage = Damage;
                            var temp4 = MyCore.NetCreateObject(3, this.Owner, (this.transform.position + (transform.right * -0.8f) + (transform.up * -0.3f)), Quaternion.LookRotation(transform.forward, transform.up));
                            temp4.GetComponent<Projectile>().Damage = Damage;
                        }
                    }
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
