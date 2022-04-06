using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering.Universal;

public class EnemyAI : NetworkComponent
{
    [Header("ENEMY COMPONENT")]
    public NavMeshAgent MyAgent;
    public Rigidbody2D MyRig;
    public Animator MyAnim;
    public Animator AttackSprite;

    [Header("ENEMY AI STATS")]
    public float RoamDistance = 5;
    public float MinStopTime = 1.5f;
    public float MaxStopTime = 5;
    public float Speed = 1.5f;
    public float AgroDistance = 6;
    public float AgroTimer = 6;
    Coroutine AgroCo;

    [Header("ENEMY STATS")]
    public float MaxHealth;
    public float Health;
    public float Damage;

    bool Dead = false;
    bool DeadCycle = false;
    bool StopTimer;
    bool FollowPlayer;
    bool DestinationMet = false;
    float rotationAngle;
    bool StopMove;
    bool Attacking;
    bool AttackAnim;
    bool Agro;
    int CurrentAgroID;

    float STATE;
    float IDLESTATE = 0;
    float RUNSTATE = 1;
    float ATTACKSTATE = 2;
    float DEADSTATE = 3;
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "STATE" && IsClient)
        {
            STATE = float.Parse(value);
            MyAnim.SetFloat("STATE", STATE);
            MyAnim.SetInteger("ISTATE", (int)STATE);
            AttackSprite.SetInteger("ISTATE", (int)STATE);
            if (STATE == DEADSTATE)
            {
                Die();
            }
        }
    }

    public override void NetworkedStart()
    {
        if (IsClient)
        {
            MyAnim = GetComponent<Animator>();
            MyRig = GetComponent<Rigidbody2D>();
            MyAgent = GetComponent<NavMeshAgent>();
            MyAgent.enabled = false;
            AttackSprite = transform.GetChild(0).GetComponent<Animator>();
        }
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer)
            {
                if (IsDirty)
                {
                    SendUpdate("STATE", STATE.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void Start()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        MyAgent.updateRotation = false;
        MyAgent.updateUpAxis = false;
        MyAnim = GetComponent<Animator>();
        MyRig = GetComponent<Rigidbody2D>();
        MyAgent.speed = Speed;
        AttackSprite = transform.GetChild(0).GetComponent<Animator>();
        Health = MaxHealth;
    }

    void Update()
    {
        if (STATE != IDLESTATE && !Dead)
        {
            MyAnim.SetFloat("SPEED", MyRig.velocity.magnitude);
        }
        else if (!Dead)
        {
            MyAnim.SetFloat("SPEED", 5);
        }

        if (IsServer && IsConnected && !Dead)
        {
            if(FindObjectsOfType<PlayerController>() != null && !Agro)
            {
                foreach (PlayerController p in FindObjectsOfType<PlayerController>())
                {
                    if (Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance)
                    {
                        MyAgent.speed = Speed * 2;
                        DestinationMet = true;
                        FollowPlayer = true;
                        Vector2 position = transform.position;
                        Vector2 direction = this.transform.up;
                        float radius = 0.6f;
                        float distance = 1.85f;

                        DetectPlayer(position, radius, direction, distance);

                        MyAgent.SetDestination(p.gameObject.transform.position);
                    }
                    else
                    {
                        FollowPlayer = false;
                        MyAgent.speed = Speed;
                    }

                }
            }

            if (!FollowPlayer && !Agro)
            {
                MyAgent.speed = Speed;
                if (DestinationMet)
                {
                    if (!StopTimer)
                    {
                        StopTimer = true;
                        StartCoroutine(Stop(Random.Range(MinStopTime, MaxStopTime)));
                    }

                    if (!StopMove)
                    {
                        MyAgent.SetDestination(new Vector3(this.transform.position.x + Random.Range((RoamDistance + 1) * -1, RoamDistance + 1), this.transform.position.y + Random.Range((RoamDistance + 1) * -1, RoamDistance + 1)));
                        DestinationMet = false;
                        StopTimer = false;
                    }
                }
                if (!DestinationMet)
                {
                    if (MyAgent.remainingDistance < 0.3f)
                    {
                        DestinationMet = true;
                    }
                }
            }

            if (Agro)
            {
                foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
                {
                    if(pc.Owner == CurrentAgroID)
                    {
                        MyAgent.speed = Speed * 2;
                        Vector2 position = transform.position;
                        Vector2 direction = this.transform.up;
                        float radius = 0.6f;
                        float distance = 1.85f;

                        DetectPlayer(position, radius, direction, distance);

                        MyAgent.SetDestination(pc.gameObject.transform.position);
                    }
                }
            }

            if (MyAgent.isStopped == false && !AttackAnim)
            {
                STATE = RUNSTATE;
            }
            else if (!AttackAnim && MyAgent.isStopped == true)
            {
                STATE = IDLESTATE;
            }

            MyAnim.SetFloat("STATE", STATE);
            MyAnim.SetInteger("ISTATE", (int)STATE);
            AttackSprite.SetInteger("ISTATE", (int)STATE);
            SendUpdate("STATE", STATE.ToString());

            MyRig.velocity = MyAgent.velocity;

            if(MyRig.velocity.magnitude > 0.2f)
            {
                Vector2 relative = MyAgent.velocity.normalized;
                rotationAngle = (Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg) - 90;
                MyRig.rotation = Mathf.LerpAngle(MyRig.rotation, rotationAngle, 2);
            }
        }
        if (Dead)
        {
            MyAgent.isStopped = true;
            MyAgent.velocity = Vector3.zero;
            MyRig.velocity = MyAgent.velocity;
        }
    }

    public IEnumerator Stop(float timer)
    {
        StopMove = true;
        MyAgent.isStopped = true;
        MyAgent.velocity = Vector3.zero;
        yield return new WaitForSeconds(timer);
        StopMove = false;
        if (!FollowPlayer)
        {
            MyAgent.isStopped = false;
        }
        MyAgent.SetDestination(new Vector3(this.transform.position.x + Random.Range((RoamDistance + 1) * -1, RoamDistance + 1), this.transform.position.y + Random.Range((RoamDistance + 1) * -1, RoamDistance + 1)));
    }
    public IEnumerator Attack()
    {
        Attacking = true;
        StartCoroutine(AttackAnimation());
        //raycast to check to do damage
        yield return new WaitForSeconds(1f);
        Attacking = false;

    }
    public IEnumerator AttackAnimation()
    {
        AttackAnim = true;
        STATE = ATTACKSTATE;
        yield return new WaitForSeconds(0.1f);
        AttackAnim = false;
    }

     public void DetectPlayer(Vector2 position1, float radius1, Vector2 direction1, float distance1)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position1, radius1, direction1, (distance1 - radius1));
        foreach (RaycastHit2D collision in hits)
        {
            if (collision.collider.gameObject.GetComponent<PlayerController>())
            {
                MyAgent.isStopped = true;
                MyAgent.velocity = Vector3.zero;
                if (!Attacking)
                {
                    StartCoroutine(Attack());
                }
            }
            else
            {
                MyAgent.isStopped = false;
            }
            
        }
    }

    public void Die()
    {
        Dead = true;
        if (!IsLocalPlayer)
        {
            this.GetComponent<ShadowCaster2D>().enabled = false;
            this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            this.GetComponent<NetworkRigidBody2D>().enabled = false;
            this.GetComponent<CapsuleCollider2D>().enabled = false;
            this.GetComponent<NavMeshAgent>().enabled = false;
            this.GetComponent<SpriteRenderer>().sortingLayerName = "Death";
            this.GetComponent<SpriteRenderer>().sortingOrder = 0;
            DeadCycle = true;
            this.GetComponent<NetworkID>().enabled = false;
            this.GetComponent<EnemyAI>().enabled = false;
        }
        if (IsLocalPlayer)
        {
            if (!DeadCycle)
            {
                this.GetComponent<ShadowCaster2D>().enabled = false;
                this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                this.GetComponent<NetworkRigidBody2D>().enabled = false;
                this.GetComponent<CapsuleCollider2D>().enabled = false;
                this.GetComponent<NavMeshAgent>().enabled = false;
                this.GetComponent<SpriteRenderer>().sortingLayerName = "Death";
                this.GetComponent<SpriteRenderer>().sortingOrder = 0;
                DeadCycle = true;
                this.GetComponent<NetworkID>().enabled = false;
                this.GetComponent<EnemyAI>().enabled = false;
            }
        }
    }

    public void TakeDamage(int attackerid, float damage)
    {
        if(AgroCo != null)
        {
            StopCoroutine(AgroCo);
        }
        Health -= damage;
        if(Health <= 0)
        {
            STATE = DEADSTATE;
            Die();
            SendUpdate("STATE", DEADSTATE.ToString());
            return;
        }

        Agro = true;
        CurrentAgroID = attackerid;
        AgroCo = StartCoroutine(FollowPlayerTimer());
    }

    public IEnumerator FollowPlayerTimer()
    {
        yield return new WaitForSeconds(AgroTimer);
        CurrentAgroID = -99;
        Agro = false;

        AgroCo = null;

        foreach (PlayerController p in FindObjectsOfType<PlayerController>())
        {
            if (Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance)
            {
                yield break;
            }
            else
            {
                StartCoroutine(Stop(Random.Range(MinStopTime, MaxStopTime)));
            }
        }
    }
}
