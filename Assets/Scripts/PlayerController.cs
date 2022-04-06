using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;
using UnityEngine.UI;

public abstract class PlayerController : NetworkComponent
{
    [Header("Components")]
    public Rigidbody2D MyRig;
    public Animator AnimController;
    public Text NameBox;
    public GameObject ShadowBox;
    public InventoryObject Inventory;

    [Header("Player Inputs")]
    public Vector2 MoveInput;
    public Vector2 AimInput;
    public Vector2 AimDir;
    public bool LFireInput;
    public bool RFireInput;
    public bool SprintInput;
    public float AimRot;

    [Header("Player Stats")]
    public float MaxHealth;
    public float Health;
    public float Damage;
    public float HealthRegeneration;
    public float AttackSpeed;
    public float XPMod;
    public float MaxStamina;
    public float Stamina;
    public float MoveSpeed;
    public float SprintMod = 1;

    [Header("Player Info")]
    public string Name;

    [Header("Current Input")]
    public string InputType;
    protected string KBM = "Keyboard&Mouse";
    protected string GP = "Gamepad";


    protected bool LFireCD;
    protected bool LFireAnimation;
    protected bool RFireCD;
    protected bool RFireAnimation;
    protected bool TakingDamage;
    protected bool Dead;
    protected bool DeadCycle;


    protected float STATE = 0;
    protected float IDLESTATE = 0;
    protected float RUNSTATE = 1;
    protected float LFIRESTATE = 2;
    protected float RFIRESTATE = 3;
    protected float TAKEDAMAGESTATE = 5;
    protected float DEADSTATE = 69;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE" && IsServer)
        {
            MoveInput = VectorFromString(value);
        }
        if(flag == "AIM" && IsServer)
        {
            AimRot = float.Parse(value);
            if(AimRot == 0)
            {
                MyRig.rotation = 0;
            }
        }
        if(flag == "LFIRE" && IsServer)
        {
            LFireInput = bool.Parse(value);
            LFire(LFireInput);
        }
        if (flag == "RFIRE" && IsServer)
        {
            RFireInput = bool.Parse(value);
            RFire(RFireInput);
        }
        if (flag == "SPRINT" && IsServer)
        {
            SprintInput = bool.Parse(value);
            Sprint(SprintInput);
        }
        if(flag == "STATE" && IsClient)
        {
            STATE = float.Parse(value);
            AnimController.SetFloat("STATE", STATE);
            AnimController.SetInteger("ISTATE", (int)STATE);
            //SlashAnim.SetInteger("SLASH", (int)STATE);
            if(STATE == DEADSTATE)
            {
                Die();
            }
        }
        if(flag == "HP" && IsClient)
        {
            Health = float.Parse(value);
            //update health bar
        }
        if (flag == "MAXHP" && IsClient)
        {
            MaxHealth = float.Parse(value);
            //update health bar
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            SendUpdate("HP", Health.ToString());
            SendUpdate("MAXHP", MaxHealth.ToString());
        }
    }
    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            if (IsDirty)
            {
                SendUpdate("HP", Health.ToString());
                SendUpdate("MAXHP", MaxHealth.ToString());
                IsDirty = false;
            }
        }
        yield return new WaitForSeconds(0.01f);
    }
    public void TakeDamage(float damage)
    {
        if (IsServer)
        {
            Health -= damage;
            SendUpdate("HP", Health.ToString());
            StartCoroutine(TakeDamageTimer());
            //screenshake
            if (Health <= 0 && !Dead)
            {
                STATE = DEADSTATE;
                Die();
                SendUpdate("STATE", DEADSTATE.ToString());
            }
        }
    }
    public void Die()
    {
        Dead = true;
        if (!IsLocalPlayer)
        {
            this.GetComponent<PlayerInput>().enabled = false;
            this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            this.GetComponent<NetworkRigidBody2D>().enabled = false;
            this.GetComponent<CircleCollider2D>().enabled = false;
            this.GetComponent<SpriteRenderer>().sortingLayerName = "Death";
            this.GetComponent<SpriteRenderer>().sortingOrder = 0;
            DeadCycle = true;
            NameBox.enabled = false;
            ShadowBox.gameObject.SetActive(false);
            this.GetComponent<NetworkID>().enabled = false;
            this.GetComponent<PlayerController>().enabled = false;
        }
        if (IsLocalPlayer)
        {
            foreach(NPM npm in FindObjectsOfType<NPM>())
            {
                if(npm.GetComponent<NPM>().Owner == this.Owner)
                {
                    if (!DeadCycle)
                    {
                        npm.ShowCanvas();
                        this.GetComponent<PlayerInput>().enabled = false;
                        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        this.GetComponent<NetworkRigidBody2D>().enabled = false;
                        this.GetComponent<CircleCollider2D>().enabled = false;
                        this.GetComponent<SpriteRenderer>().sortingLayerName = "Death";
                        this.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        DeadCycle = true;
                        NameBox.enabled = false;
                        ShadowBox.gameObject.SetActive(false);
                        this.GetComponent<NetworkID>().enabled = false;
                        this.GetComponent<PlayerController>().enabled = false;
                    }
                }
            }
        }
    }
    public IEnumerator TakeDamageTimer()
    {
        TakingDamage = true;
        STATE = TAKEDAMAGESTATE;
        yield return new WaitForSeconds(0.10f);
        TakingDamage = false;
    }
    public void Heal(float amount)
    {
        if (IsServer)
        {
            if (Health + amount >= MaxHealth)
            {
                Health = MaxHealth;
            }
            else
            {
                Health += amount;
            }
            SendUpdate("HP", Health.ToString());
        }
    }
    public void IncreaseMaxHealth(float amount)
    {
        if (IsServer)
        {
            MaxHealth += amount;
            SendUpdate("MAXHP", MaxHealth.ToString());
        }
    }
    public void IncreaseMoveSpeed(float amount)
    {
        if (IsServer)
        {
            MoveSpeed += amount;
        }
    }
    public void IncreaseDamage(float amount)
    {
        if (IsServer)
        {
            Damage += amount;
        }
    }
    public void IncreaseHealthRegen(float amount)
    {
        if (IsServer)
        {
            HealthRegeneration += amount;
        }
    }
    public void IncreaseAttackSpeed(float amount)
    {
        if (IsServer)
        {
            AttackSpeed += amount;
        }
    }
    public void IncreaseXPMod(float amount)
    {
        if (IsServer)
        {
            XPMod += amount;
        }
    }
    public void IncreaseMaxStamina(float amount)
    {
        if (IsServer)
        {
            MaxStamina += amount;
        }
    }
    public void IncreaseStamina(float amount)
    {
        if (IsServer)
        {
            if(Stamina + amount >= MaxStamina)
            {
                Stamina = MaxStamina;
            }
            else
            {
                Stamina += amount;
            }
        }
    }
    public abstract IEnumerator LFire();
    public abstract IEnumerator RFire();

    public static Vector2 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector2(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()));
    }
    public static bool BoolFromFloat(float value)
    {
        if(value < 0.3)
        {
            return false;
        }
        else if (value >= 0.3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GlobalMousePos(Vector2 position)
    {
        Vector3 mousePos = new Vector3(position.x, position.y, Mathf.Infinity);

        Ray rayPos = Camera.main.ScreenPointToRay(mousePos);

        Vector2 relative = rayPos.origin - MyRig.transform.position;

        AimDir = relative.normalized * ((Mathf.Clamp(relative.magnitude / 5, 0, 1)));

        float angle = Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg;

        return angle - 90;
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer && !Dead)
        {
            MoveInput = context.ReadValue<Vector2>();
            SendCommand("MOVE", MoveInput.ToString());
        }
    }
    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer && !Dead)
        {
            AimInput = context.ReadValue<Vector2>();
            if (InputType == KBM)
            {
                SendCommand("AIM", GlobalMousePos(AimInput).ToString());
            }
            if(InputType == GP)
            {
                if(AimInput.magnitude > 0)
                {
                    AimDir = AimInput;
                    SendCommand("AIM", ((Mathf.Atan2(AimInput.y, AimInput.x) * Mathf.Rad2Deg) - 90).ToString());
                }
                else
                {
                    AimDir = Vector2.zero;
                }
            }
        }
    }
    
    public void OnLClickInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer && !Dead)
        {
            LFireInput = BoolFromFloat(context.ReadValue<float>());
            SendCommand("LFIRE", LFireInput.ToString());
        }
    }
    public void OnRClickInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer && !Dead)
        {
            RFireInput = BoolFromFloat(context.ReadValue<float>());
            SendCommand("RFIRE", RFireInput.ToString());
        }
    }
    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer && !Dead)
        {
            SprintInput = BoolFromFloat(context.ReadValue<float>());
            SendCommand("SPRINT", SprintInput.ToString());
        }
    }
    public void OnInputChange(PlayerInput input)
    {
        InputType = input.currentControlScheme;
    }
    public abstract void LFire(bool state);
    public abstract void RFire(bool state);
    public void Sprint(bool state)
    {
        if (!state)
        {
            SprintMod = 1;
        }
        if (state)
        {
            SprintMod = 1.6f;
        }
    }
    public virtual void Start()
    {
        Inventory = ScriptableObject.CreateInstance<InventoryObject>();
        Health = MaxHealth;
        SprintMod = 1;
        MyRig = GetComponent<Rigidbody2D>();
        if(MyRig == null)
        {
            throw new System.Exception("ERROR: Could not find Rigidbody!");
        }
        AnimController = GetComponent<Animator>();
        if (AnimController == null)
        {
            throw new System.Exception("ERROR: Could not find Animator!");
        }

        foreach(NPM npm in FindObjectsOfType<NPM>())
        {
            if(npm.Owner == this.Owner)
            {
                Name = npm.Name;
            }
        }

        NameBox.text = Name;
    }
    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            float cameraSpeed = 5f;
            Vector3 offsetVector = transform.forward * -10 + (new Vector3(AimDir.x, AimDir.y, 0) * 1.3f);
            Vector3 targetCameraPosition = this.gameObject.transform.position + offsetVector;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, cameraSpeed * Time.deltaTime);
            Camera.main.transform.forward = new Vector3(0, 0, 1);
        }
    }
    private void Update()
    {
        if (IsServer)
        {
            if (!Dead)
            {
                MyRig.velocity = (MoveInput * MoveSpeed * SprintMod);
                MyRig.SetRotation(AimRot);
            }

            if (MyRig.velocity.magnitude == 0 && !LFireAnimation && !RFireAnimation && !TakingDamage && !Dead)
            {
                STATE = IDLESTATE;
            }
            if (MyRig.velocity.magnitude > 0 && !LFireAnimation && !RFireAnimation && !TakingDamage && !Dead)
            {
                STATE = RUNSTATE;
            }

            if (!Dead)
            {
                AnimController.SetFloat("STATE", STATE);
                AnimController.SetInteger("ISTATE", (int)STATE);
                SendUpdate("STATE", STATE.ToString());
            }
        }

        if (IsClient)
        {
            if(STATE != IDLESTATE && !Dead)
            {
                AnimController.SetFloat("SPEED", MyRig.velocity.magnitude);
            }
            else
            {
                AnimController.SetFloat("SPEED", 5);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            var item = collision.GetComponent<Item>();
            if (item)
            {
                Inventory.AddItem(item.item, 1);
                MyCore.NetDestroyObject(collision.GetComponent<Item>().NetId);
            }
        }
    }
}
