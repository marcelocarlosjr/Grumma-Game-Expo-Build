using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class EnemyAI : NetworkComponent
{
    public enum RangeType
    {
        Melee,
        Ranged
    }

    public RangeType rangeType;

    [Header("ENEMY COMPONENT")]
    public NavMeshAgent MyAgent;
    public Rigidbody2D MyRig;
    public Animator MyAnim;
    public Animator AttackSprite;
    public GameObject HealthBar;
    public int ProjectilePrefab;

    [Header("ENEMY AI STATS")]
    public float RoamDistance = 5;
    public float MinStopTime = 1.5f;
    public float MaxStopTime = 5;
    public float Speed = 1.5f;
    public float AgroDistance = 6;
    public float AgroTimer = 6;
    Coroutine AgroCo;

    public float radius;
    public float distance;

    [Header("ENEMY STATS")]
    public int Level;
    public int ExpDrop;
    public float MaxHealth;
    public float Health;
    public float Damage;
    public List<int> ItemDrops;
    public int EmptyDrops;
    public float AttackSpeed;
    public float ProjectileSpeed;

    bool Dead = false;
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
        if (flag == "STATE" && IsClient)
        {
            STATE = float.Parse(value);
            MyAnim.SetFloat("STATE", STATE);
            MyAnim.SetInteger("ISTATE", (int)STATE);
            if(rangeType == RangeType.Melee)
            {
                AttackSprite.SetInteger("ISTATE", (int)STATE);
            }
            if (STATE == DEADSTATE)
            {
                HealthBar.transform.GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
                HealthBar.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
                Die();
            }
        }
        if (flag == "HEALTH" && IsClient)
        {
            Health = float.Parse(value);
        }
        if(flag == "DAMAGE" && IsClient)
        {
            StartCoroutine(FlashRed());
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
            if (rangeType == RangeType.Melee)
            {
                AttackSprite = transform.GetChild(0).GetComponent<Animator>();
            }
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
                    SendUpdate("HEALTH", Health.ToString());
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
        if (rangeType == RangeType.Melee)
        {
            AttackSprite = transform.GetChild(0).GetComponent<Animator>();
        }
        Health = MaxHealth;
    }

    void Update()
    {
        if(Health <= 0 && IsLocalPlayer)
        {
            HealthBar.transform.GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
            HealthBar.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
        }
        if (STATE != IDLESTATE && !Dead)
        {
            MyAnim.SetFloat("SPEED", MyRig.velocity.magnitude);
        }
        else if (!Dead)
        {
            MyAnim.SetFloat("SPEED", 5);
        }
        if (rangeType == RangeType.Melee)
        {
            if (IsServer && IsConnected && !Dead)
            {
                if (FindObjectsOfType<PlayerController>() != null && !Agro)
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
                        if (pc.Owner == CurrentAgroID)
                        {
                            MyAgent.speed = Speed * 2;
                            Vector2 position = transform.position;
                            Vector2 direction = this.transform.up;

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

                if (MyRig.velocity.magnitude > 0.2f)
                {
                    Vector2 relative = MyAgent.velocity.normalized;
                    rotationAngle = (Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg) - 90;
                    MyRig.rotation = Mathf.LerpAngle(MyRig.rotation, rotationAngle, 2);
                }
            }
        }
        Coroutine shoot;
        if (rangeType == RangeType.Ranged)
        {
            if (IsServer && IsConnected && !Dead)
            {
                if (FindObjectsOfType<PlayerController>() != null)
                {
                    foreach (PlayerController p in FindObjectsOfType<PlayerController>())
                    {
                        if (Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance + AgroDistance / 1.5f)
                        {
                            if (!shooting)
                            {
                                shoot = StartCoroutine(ShootEnemy((p.transform.position - this.transform.position).normalized));
                            }
                        }
                    }
                }
                if (FindObjectsOfType<PlayerController>() != null && !Agro)
                {
                    foreach (PlayerController p in FindObjectsOfType<PlayerController>())
                    {
                        if (Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance)
                        {
                            FollowPlayer = true;
                            Vector3 OppositePlayerDirection = ((p.transform.position - this.transform.position).normalized * -1);
                            NavMeshHit CheckBehind = new NavMeshHit();
                            bool hit = MyAgent.Raycast(OppositePlayerDirection, out CheckBehind);
                            if (hit)
                            {
                                MyAgent.isStopped = false;
                                MyAgent.speed = Speed * 1.3f;
                                MyAgent.SetDestination(transform.position + OppositePlayerDirection);
                            }
                            else
                            {
                                OppositePlayerDirection = Vector3.Cross((p.transform.position - this.transform.position).normalized, Vector3.up);
                                CheckBehind = new NavMeshHit();
                                hit = MyAgent.Raycast(OppositePlayerDirection, out CheckBehind);
                                if (hit)
                                {
                                    MyAgent.isStopped = false;
                                    MyAgent.speed = Speed * 1.3f;
                                    MyAgent.SetDestination(transform.position + OppositePlayerDirection);
                                }
                                else
                                {
                                    OppositePlayerDirection = Vector3.Cross((p.transform.position - this.transform.position).normalized, Vector3.up) * -1;
                                    CheckBehind = new NavMeshHit();
                                    hit = MyAgent.Raycast(OppositePlayerDirection, out CheckBehind);
                                    if (hit)
                                    {
                                        MyAgent.isStopped = false;
                                        MyAgent.speed = Speed * 1.3f;
                                        MyAgent.SetDestination(transform.position + OppositePlayerDirection);
                                    }
                                }
                            }
                            break;
                        }
                        else if(Vector2.Distance(this.transform.position, p.transform.position) > AgroDistance + 1.14 && Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance + AgroDistance/1.5f)
                        {
                            MyAgent.speed = Speed * 1.3f;
                            FollowPlayer = true;
                            MyAgent.isStopped = false;
                            MyAgent.SetDestination(p.gameObject.transform.position);
                        }
                        else if (Vector2.Distance(this.transform.position, p.transform.position) > AgroDistance + 1 && Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance + 1.15)
                        {
                            MyAgent.speed = Speed * 1.3f;
                            FollowPlayer = true;
                            MyAgent.isStopped = true;
                            MyAgent.velocity = Vector3.zero;
                            //attack player
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
                    foreach (PlayerController p in FindObjectsOfType<PlayerController>())
                    {
                        if (p.Owner == CurrentAgroID)
                        {
                            if (Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance)
                            {
                                FollowPlayer = true;
                                Vector3 OppositePlayerDirection = ((p.transform.position - this.transform.position).normalized * -1);
                                NavMeshHit CheckBehind = new NavMeshHit();
                                bool hit = MyAgent.Raycast(OppositePlayerDirection, out CheckBehind);
                                if (hit)
                                {
                                    MyAgent.isStopped = false;
                                    MyAgent.speed = Speed * 1.3f;
                                    MyAgent.SetDestination(transform.position + OppositePlayerDirection);
                                }
                                else
                                {
                                    OppositePlayerDirection = Vector3.Cross((p.transform.position - this.transform.position).normalized, Vector3.up);
                                    CheckBehind = new NavMeshHit();
                                    hit = MyAgent.Raycast(OppositePlayerDirection, out CheckBehind);
                                    if (hit)
                                    {
                                        MyAgent.isStopped = false;
                                        MyAgent.speed = Speed * 1.3f;
                                        MyAgent.SetDestination(transform.position + OppositePlayerDirection);
                                    }
                                    else
                                    {
                                        OppositePlayerDirection = Vector3.Cross((p.transform.position - this.transform.position).normalized, Vector3.up) * -1;
                                        CheckBehind = new NavMeshHit();
                                        hit = MyAgent.Raycast(OppositePlayerDirection, out CheckBehind);
                                        if (hit)
                                        {
                                            MyAgent.isStopped = false;
                                            MyAgent.speed = Speed * 1.3f;
                                            MyAgent.SetDestination(transform.position + OppositePlayerDirection);
                                        }
                                    }
                                }
                                break;
                            }
                            else if (Vector2.Distance(this.transform.position, p.transform.position) > AgroDistance + 1.14)
                            {
                                MyAgent.speed = Speed * 1.3f;
                                FollowPlayer = true;
                                MyAgent.isStopped = false;
                                MyAgent.SetDestination(p.gameObject.transform.position);
                            }
                            else if (Vector2.Distance(this.transform.position, p.transform.position) > AgroDistance + 1 && Vector2.Distance(this.transform.position, p.transform.position) < AgroDistance + 1.15)
                            {
                                MyAgent.speed = Speed * 1.3f;
                                FollowPlayer = true;
                                MyAgent.isStopped = true;
                                MyAgent.velocity = Vector3.zero;
                                //attack player
                            }
                            else
                            {
                                FollowPlayer = false;
                                MyAgent.speed = Speed;
                            }

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
                SendUpdate("STATE", STATE.ToString());
                MyRig.velocity = MyAgent.velocity;
                if (MyRig.velocity.magnitude > 0.2f)
                {
                    Vector2 relative = MyAgent.velocity.normalized;
                    rotationAngle = (Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg) - 90;
                    MyRig.rotation = Mathf.LerpAngle(MyRig.rotation, rotationAngle, 2);
                }
            }
        }

        if (Dead)
        {
            if (FindObjectOfType<NavMeshAgent>().enabled)
            {
                MyAgent.isStopped = true;
                MyAgent.velocity = Vector3.zero;
                MyRig.velocity = MyAgent.velocity;
            }
        }


        HealthBar.transform.GetChild(1).GetComponent<RectTransform>().localScale = Vector3.Lerp(HealthBar.transform.GetChild(1).GetComponent<RectTransform>().localScale, new Vector3(Health / MaxHealth,1,1), 1f);
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
        yield return new WaitForSeconds(AttackSpeed);
        Attacking = false;

    }
    public IEnumerator AttackAnimation()
    {
        AttackAnim = true;
        STATE = ATTACKSTATE;
        yield return new WaitForSeconds(0.1f);
        AttackAnim = false;
    }
    bool shooting;
    public IEnumerator ShootEnemy(Vector3 direction)
    {
        shooting = true;
        StartCoroutine(AttackAnimation());
        var temp = MyCore.NetCreateObject(ProjectilePrefab, -1, this.transform.position, Quaternion.LookRotation(transform.forward, direction));
        temp.GetComponent<EnemyProjectile>().SetData(ProjectileSpeed, Damage);
        yield return new WaitForSeconds(AttackSpeed);
        shooting = false;
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
                    collision.collider.GetComponent<PlayerController>().TakeDamage(Damage, -1);
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
        this.GetComponent<ShadowCaster2D>().enabled = false;
        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        this.GetComponent<NetworkRigidBody2D>().enabled = false;
        if (this.GetComponent<CapsuleCollider2D>())
        {
            this.GetComponent<CapsuleCollider2D>().enabled = false;
        }
        if (this.GetComponent<CircleCollider2D>())
        {
            this.GetComponent<CircleCollider2D>().enabled = false;
        }
        this.GetComponent<NavMeshAgent>().enabled = false;
        this.GetComponent<SpriteRenderer>().sortingLayerName = "Death";
        this.GetComponent<SpriteRenderer>().sortingOrder = 0;
        if (IsServer)
        {
            Invoke("DestroyBody", 10f);
        }
        //this.GetComponent<NetworkID>().enabled = false;
        //this.GetComponent<EnemyAI>().enabled = false;
    }
    public IEnumerator FlashRed()
    {
        SpriteRenderer temp = GetComponent<SpriteRenderer>();
        temp.color = new Color32(255, 0, 0, 255);
        yield return new WaitForSeconds(0.1f);
        temp.color = new Color32(255, 255, 255, 255);
        yield return new WaitForSeconds(0.1f);
    }
    public void TakeDamage(int attackerid, float damage)
    {
        SendUpdate("DAMAGE", "1");
        if (rangeType == RangeType.Melee)
        {
            if (AgroCo != null)
            {
                StopCoroutine(AgroCo);
            }
            Health -= damage;
            SendUpdate("HEALTH", Health.ToString());
            if (Health <= 0)
            {
                STATE = DEADSTATE;
                foreach(PlayerController p in FindObjectsOfType<PlayerController>())
                {
                    if(p.Owner == attackerid)
                    {
                        p.levelSystem.AddExperience(ExpDrop);
                    }
                }
                DropItem();
                Die();
                SendUpdate("STATE", DEADSTATE.ToString());
                return;
            }

            Agro = true;
            CurrentAgroID = attackerid;
            AgroCo = StartCoroutine(FollowPlayerTimer());
        }
        if(rangeType == RangeType.Ranged)
        {
            if (AgroCo != null)
            {
                StopCoroutine(AgroCo);
            }
            Health -= damage;
            SendUpdate("HEALTH", Health.ToString());
            if (Health <= 0)
            {
                STATE = DEADSTATE;
                foreach (PlayerController p in FindObjectsOfType<PlayerController>())
                {
                    if (p.Owner == attackerid)
                    {
                        p.levelSystem.AddExperience(ExpDrop);
                    }
                }
                DropItem();
                Die();
                SendUpdate("STATE", DEADSTATE.ToString());
                return;
            }
                        Agro = true;
            CurrentAgroID = attackerid;
            AgroCo = StartCoroutine(FollowPlayerTimer());
        }
    }
    bool ItemDropped;
    public void DropItem()
    {
        if (!ItemDropped)
        {
            int item = Random.Range(0, ItemDrops.Count + EmptyDrops);
            if (item >= ItemDrops.Count)
            {
                return;
            }
            else if (item < ItemDrops.Count)
            {
                var temp = MyCore.NetCreateObject(ItemDrops[item], -1, this.transform.position, Quaternion.identity);
                temp.GetComponent<Item>().ThrowItem(this.transform.position + (new Vector3(0, 1, 0) * Random.Range(-1.5f, 1.5f)) + (new Vector3(1, 0, 0) * Random.Range(-1.5f, 1.5f)));
            }
            ItemDropped = true;
        }
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

    public void DestroyBody()
    {
        MyCore.NetDestroyObject(NetId);
    }

    public void DisableHealthBar()
    {
        HealthBar.SetActive(false);
    }
    public void EnableHealthBar(int owner)
    {
        if (owner == MyCore.LocalPlayerId)
        {
            HealthBar.SetActive(true);
        }
    }
}
