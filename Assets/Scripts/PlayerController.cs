using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;
using UnityEngine.UI;
using UnityEngine.Animations;
using System;

public abstract class PlayerController : NetworkComponent
{
    [Header("Components")]
    public Rigidbody2D MyRig;
    public Animator AnimController;
    public Text NameBox;
    public GameObject ShadowBox;
    public InventoryObject Inventory;
    public ItemDatabaseObject StaticItemDatabase;
    public DisplayInventory DisplayUI;
    public int LastEnemyAttacked;
    public LevelSystem levelSystem;

    [Header("Player Inputs")]
    public Vector2 MoveInput;
    public Vector2 AimInput;
    public Vector2 AimDir;
    public bool LFireInput;
    public bool RFireInput;
    public bool SprintInput;
    public float AimRot;

    [Header("Player Current Stats")]
    public float MaxHealth;
    public float Health;
    public float Damage;
    public float HealthRegeneration;
    public float AttackSpeed;
    public float MaxStamina;
    public float Stamina;
    public float MoveSpeed;
    public float EXPMulti;
    public float SprintMod = 1;
    public float EXP;
    public float EXPToLevel;
    public int Level;

    [Header("Player Base Stats")]
    public float MoveSpeedBase;
    public float HealthBase;
    public float DamageBase;
    public float HealthRegenerationBase;
    public float AttackSpeedBase;
    public float EXPBase;
    public float StaminaBase;

    [Header("Player Item Mods")]
    public float MoveSpeedMod;
    public float HealthMod;
    public float DamageMod;
    public float HealthRegenerationMod;
    public float AttackSpeedMod;
    public float EXPMod;
    public float StaminaMod;

    [Header("Player Level Upgrades")]
    public float MoveSpeedUpgrade;
    public float HealthUpgrade;
    public float DamageUpgrade;
    public float HealthRegenerationUpgrade;
    public float AttackSpeedUpgrade;
    public float EXPModUpgrade;
    public float StaminaUpgrade;

    float MoveSpeedUpgradeMod = 0.2f;
    float HealthUpgradeMod = 5;
    float DamageUpgradeMod = 1;
    float HealthRegenerationUpgradeMod = 0.25f;
    float AttackSpeedUpgradeMod = 0;
    float EXPModUpgradeMod = 0.1f;
    float StaminaUpgradesMod = 0.1f;

    [Header("Player Info")]
    public string Name;
    public int type;

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

    public static readonly int[] LevelEXP = { 100, 120, 160, 220, 300, 400, 520, 660, 820, 1000, 1200, 1420, 1660, 1920, 2200, 2500, 2820, 3160, 3520, 3900, 4300, 4720, 5160, 5620, 6100, 6600, 7120, 7660, 8220, 8800, 9400, 10020, 10660, 11320, 12000, 12700, 13420, 14160, 14920, 15700, 16500, 17320, 18160, 19020, 19900, 20800, 21720, 22660, 23620, 24600, 25600 };

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
            if (SprintInput)
            {
                StartCoroutine(SprintStamina());
            }
            else
            {
                StartCoroutine(SprintStaminaRegen());
            }
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
                if (IsLocalPlayer)
                {
                    FindObjectOfType<DisplayInventory>().CallDropAllItems(this);
                }
            }
        }
        if(flag == "HP" && IsClient)
        {
            Health = float.Parse(value);
        }
        if (flag == "MAXHP" && IsClient)
        {
            MaxHealth = float.Parse(value);
            //update health bar
        }
        if (flag == "UPDATEINV" && IsLocalPlayer)
        {
            string[] args = value.Split(',');
            Inventory.AddItem(Inventory.database.GetItem[int.Parse(args[0])], int.Parse(args[1]), -99);
        }
        if (flag == "REMOVEINV" && IsServer)
        {
            string[] args = value.Split(',');
            Inventory.RemoveItem(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), this.Owner, this.transform.position, this.transform.up, this.transform.right);
        }
        if (flag == "REMOVEAllINV" && IsServer)
        {
            string[] args = value.Split(',');
            Inventory.RemoveALLItem(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), this.Owner, this.transform.position);
        }
        if (flag == "STAMINA" && IsLocalPlayer)
        {
            Stamina = float.Parse(value);
        }
        if (flag == "MAXSTAMINA" && IsLocalPlayer)
        {
            MaxStamina = float.Parse(value);
        }
        if (flag == "LASTENEMY" && IsLocalPlayer)
        {
            LastEnemyAttacked = int.Parse(value);
            foreach (EnemyAI ID in FindObjectsOfType<EnemyAI>())
            {
                if (ID.NetId != LastEnemyAttacked)
                {
                    ID.gameObject.GetComponent<EnemyAI>().DisableHealthBar();
                }
                else if (ID.NetId == LastEnemyAttacked)
                {
                    ID.gameObject.GetComponent<EnemyAI>().EnableHealthBar(Owner);
                }
            }
        }
        if(flag == "EXPERIENCE" && IsLocalPlayer)
        {
            string[] args = value.Split(',');
            EXP = int.Parse(args[0]);
            EXPToLevel = int.Parse(args[1]);
        }
        if (flag == "LEVEL" && IsLocalPlayer)
        {
            Level = int.Parse(value);
        }

        if(flag == "TELEPORT" && IsLocalPlayer)
        {

            OfflinePlayerHolder temp = FindObjectOfType<OfflinePlayerHolder>();

            string[] args = value.Split(',');
            temp.Health = float.Parse(args[0]);
            temp.Stamina = float.Parse(args[1]);
            temp.EXP = float.Parse(args[2]);
            temp.EXPToLevel = float.Parse(args[3]);
            temp.Level = int.Parse(args[4]);
            temp.MoveSpeedMod = float.Parse(args[5]);
            temp.HealthMod = float.Parse(args[6]);
            temp.DamageMod = float.Parse(args[7]);
            temp.HealthRegenerationMod = float.Parse(args[8]);
            temp.AttackSpeedMod = float.Parse(args[9]);
            temp.EXPMod = float.Parse(args[10]);
            temp.StaminaMod = float.Parse(args[11]);
            temp.MoveSpeedUpgrade = float.Parse(args[12]);
            temp.HealthUpgrade = float.Parse(args[13]);
            temp.DamageUpgrade = float.Parse(args[14]);
            temp.HealthRegenerationUpgrade = float.Parse(args[15]);
            temp.AttackSpeedUpgrade = float.Parse(args[16]);
            temp.EXPModUpgrade = float.Parse(args[17]);
            temp.StaminaUpgrade = float.Parse(args[18]);
            int SceneNum = int.Parse(args[19]);
            temp.item1ID = int.Parse(args[20]);
            temp.item2ID = int.Parse(args[21]);
            temp.item3ID = int.Parse(args[22]);
            temp.item4ID = int.Parse(args[23]);
            temp.item5ID = int.Parse(args[24]);

            temp.StartCoroutine(temp.Teleport(SceneNum));
        }
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            SendUpdate("HP", Health.ToString());
            SendUpdate("MAXHP", MaxHealth.ToString());
            TouchingObjects = new List<Item>();
        }

        if (IsLocalPlayer)
        {
            FindObjectOfType<PlayerHealthUI>().SetPlayer(this);
            DisplayUI = FindObjectOfType<DisplayInventory>();
            Inventory = ScriptableObject.CreateInstance<InventoryObject>();
            Inventory.database = StaticItemDatabase;
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

    public void GetLastEnemy(int _id)
    {
        SendUpdate("LASTENEMY", _id.ToString());
    }

    public void AddStat(string _attribute, string _rarity)
    {
        switch (_rarity)
        {
            case "Common":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod += 5;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod += 5;
                        break;
                    case "Health_Increase":
                        HealthMod += 5;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod += 5;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod += 5;
                        break;
                    case "Stamina_Increase":
                        StaminaMod += 5;
                        break;
                    case "Exp_Increase":
                        EXPMod += 5;
                        break;
                }
                break;
            case "Uncommon":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod += 10;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod += 10;
                        break;
                    case "Health_Increase":
                        HealthMod += 10;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod += 10;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod += 10;
                        break;
                    case "Stamina_Increase":
                        StaminaMod += 10;
                        break;
                    case "Exp_Increase":
                        EXPMod += 10;
                        break;
                }
                break;
            case "Rare":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod += 15;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod += 15;
                        break;
                    case "Health_Increase":
                        HealthMod += 15;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod += 15;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod += 15;
                        break;
                    case "Stamina_Increase":
                        StaminaMod += 15;
                        break;
                    case "Exp_Increase":
                        EXPMod += 15;
                        break;
                }
                break;
            case "Legendary":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod += 25;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod += 25;
                        break;
                    case "Health_Increase":
                        HealthMod += 25;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod += 25;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod += 25;
                        break;
                    case "Stamina_Increase":
                        StaminaMod += 25;
                        break;
                    case "Exp_Increase":
                        EXPMod += 25;
                        break;
                }
                break;
        }
    }

    public void RemoveStat(string _attribute, string _rarity)
    {
        switch (_rarity)
        {
            case "Common":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod -= 5;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod -= 5;
                        break;
                    case "Health_Increase":
                        HealthMod -= 5;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod -= 5;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod -= 5;
                        break;
                    case "Stamina_Increase":
                        StaminaMod -= 5;
                        break;
                    case "Exp_Increase":
                        EXPMod -= 5;
                        break;
                }
                break;
            case "Uncommon":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod -= 10;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod -= 10;
                        break;
                    case "Health_Increase":
                        HealthMod -= 10;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod -= 10;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod -= 10;
                        break;
                    case "Stamina_Increase":
                        StaminaMod -= 10;
                        break;
                    case "Exp_Increase":
                        EXPMod -= 10;
                        break;
                }
                break;
            case "Rare":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod -= 15;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod -= 15;
                        break;
                    case "Health_Increase":
                        HealthMod -= 15;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod -= 15;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod -= 15;
                        break;
                    case "Stamina_Increase":
                        StaminaMod -= 15;
                        break;
                    case "Exp_Increase":
                        EXPMod -= 15;
                        break;
                }
                break;
            case "Legendary":
                switch (_attribute)
                {
                    case "Damage_Increase":
                        DamageMod -= 25;
                        break;
                    case "Regen_Increase":
                        HealthRegenerationMod -= 25;
                        break;
                    case "Health_Increase":
                        HealthMod -= 25;
                        break;
                    case "MoveSpeed_Increase":
                        MoveSpeedMod -= 25;
                        break;
                    case "AttackSpeed_Increase":
                        AttackSpeedMod -= 25;
                        break;
                    case "Stamina_Increase":
                        StaminaMod -= 25;
                        break;
                    case "Exp_Increase":
                        EXPMod -= 25;
                        break;
                }
                break;
        }
    }

    public void UpdateInv(int _id, int _amount)
    {
        if (IsServer)
        {
            SendUpdate("UPDATEINV", _id + "," + _amount);
        }
    }
    public void RemoveInv(int _index, int _id, int _amount)
    {
        if (IsLocalPlayer)
        {
            SendCommand("REMOVEINV", _index + "," + _id + "," + _amount);
            Inventory.RemoveItem(_index, _id, _amount, this.Owner, Vector3.zero, Vector3.zero, Vector3.zero);
        }
    }
    public void RemoveAllInv(int _index, int _id, int _amount)
    {
        if (IsLocalPlayer)
        {
            SendCommand("REMOVEAllINV", _index + "," + _id + "," + _amount);
            Inventory.RemoveItem(_index, _id, _amount, this.Owner, Vector3.zero, Vector3.zero, Vector3.zero);
        }
    }
    int tempEXP;
    public void TakeDamage(float damage, int id)
    {
        if (IsServer)
        {
            Health -= damage;
            SendUpdate("HP", Health.ToString());
            StartCoroutine(TakeDamageTimer());
            //screenshake
            if (Health <= 0 && !Dead)
            {
                tempEXP = 0;
                STATE = DEADSTATE;
                foreach(PlayerController p in FindObjectsOfType<PlayerController>())
                {
                    if(p.Owner == id)
                    {
                        for (int i = 0; i < Level; i++)
                        {
                            tempEXP += LevelEXP[i];
                        }

                        p.levelSystem.AddExperience((int)tempEXP/3);
                    }
                }
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
            EXPMod += amount;
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
            DisplayUI.GetMousePos(AimInput);
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
    bool NoStamina;
    public void Sprint(bool state)
    {
        if (!state || NoStamina)
        {
            SprintMod = 1;
        }
        if (state && !NoStamina)
        {
            SprintMod = 1.6f;
        }
    }
    bool Sprinting;
    bool RegenStamina;
    public IEnumerator SprintStamina()
    {
        while (SprintInput)
        {
            if(Stamina <= 0)
            {
                Stamina = 0;
                NoStamina = true;
            }
            else if(Stamina > 0)
            {
                Stamina -= 0.004f;
            }
            SendUpdate("STAMINA", Stamina.ToString());
            yield return new WaitForSeconds(0.01f);
        }
    }
    public IEnumerator SprintStaminaRegen()
    {
        while (!SprintInput)
        {
            if(Stamina <= MaxStamina)
            {
                NoStamina = false;
                Stamina += 0.001f;
                SendUpdate("STAMINA", Stamina.ToString());
            }
            else
            {
                Stamina = MaxStamina;
                SendUpdate("STAMINA", Stamina.ToString());
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
    public virtual void Start()
    {
        Inventory = ScriptableObject.CreateInstance<InventoryObject>();
        Inventory.database = StaticItemDatabase;
        Health = HealthBase;
        MaxHealth = HealthBase;
        MaxStamina = StaminaBase;
        Stamina = StaminaBase;
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

        levelSystem = new LevelSystem();
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
        levelSystem.OnExperienceChanged += LevelSystem_OnExperienceChanged;
    }

    public void LevelSystem_OnExperienceChanged(object sender, EventArgs e)
    {
        EXP = levelSystem.experience;
        SendUpdate("EXPERIENCE", EXP + ","+ levelSystem.GetExperienceToNextLevel(levelSystem.GetLevelNumber()));
    }

    public void LevelSystem_OnLevelChanged(object sender, EventArgs e)
    {
        Level = levelSystem.level + 1;
        SendUpdate("LEVEL", Level.ToString());

        var temp = FindObjectOfType<UpgradeController>();
        temp.FadeIN();
        temp.SetCurrentUpgradeAmount(1);
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
            if (NoStamina)
            {
                SprintMod = 1;
            }


            Damage = (DamageBase + (DamageUpgradeMod * DamageUpgrade)) + ((DamageMod * .01f) * (DamageBase + (DamageUpgradeMod * DamageUpgrade)));
            MaxHealth = (HealthBase + (HealthUpgradeMod * HealthUpgrade)) + ((HealthMod * .01f) * (HealthBase + (HealthUpgradeMod * HealthUpgrade)));
            HealthRegeneration = (HealthRegenerationBase + (HealthRegenerationUpgradeMod * HealthRegenerationUpgrade)) + ((HealthRegenerationMod * .01f) * (HealthRegenerationBase + (HealthRegenerationUpgradeMod * HealthRegenerationUpgrade)));
            MoveSpeed = (MoveSpeedBase + (MoveSpeedUpgradeMod * MoveSpeedUpgrade)) + ((MoveSpeedMod * .01f) * (MoveSpeedBase + (MoveSpeedUpgradeMod * MoveSpeedUpgrade)));
            AttackSpeed = (AttackSpeedBase + (AttackSpeedUpgradeMod * AttackSpeedUpgrade)) + ((AttackSpeedMod * .01f) * (AttackSpeedBase + (AttackSpeedUpgradeMod * AttackSpeedUpgrade)));
            MaxStamina = (StaminaBase + (StaminaUpgradesMod * StaminaUpgrade)) + ((StaminaMod * .01f) * (StaminaBase + (StaminaUpgradesMod * StaminaUpgrade)));
            EXPMulti = (EXPBase + (EXPModUpgradeMod * EXPModUpgrade)) + ((EXPMod * .01f) * (EXPBase + (EXPModUpgradeMod * EXPModUpgrade)));



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

    public List<Item> TouchingObjects;
    bool PickingUp;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (Inventory.Container.Count <= 4)
            {
                if (!TouchingObjects.Contains(collision.GetComponent<Item>()))
                {
                    TouchingObjects.Add(collision.GetComponent<Item>());
                }
            }
            else
            {
                TouchingObjects.Clear();
            }

            if (!PickingUp)
            {
                StartCoroutine(CollisionTimer());
            }
        }

        if (collision.gameObject.tag == "DOOR" && IsServer)
        {
            int[] ids = {0,0,0,0,0 };
            for (int i = 0; i < Inventory.Container.Count; i++)
            {
                ids[i] = Inventory.database.GetID[Inventory.Container[i].item];
            }

            SendUpdate("TELEPORT",
                Health + "," +
                Stamina + "," +
                EXP + "," +
                EXPToLevel + "," +
                Level + "," +
                MoveSpeedMod + "," +
                HealthMod + "," +
                DamageMod + "," +
                HealthRegenerationMod + "," +
                AttackSpeedMod + "," +
                EXPMod + "," +
                StaminaMod + "," +
                MoveSpeedUpgrade + "," +
                HealthUpgrade + "," +
                DamageUpgrade + "," +
                HealthRegenerationUpgrade + "," +
                AttackSpeedUpgrade + "," +
                EXPModUpgrade + "," +
                StaminaUpgrade + "," + collision.gameObject.name + "," + ids[0] + "," + ids[1] + "," + ids[2] + "," + ids[3] + "," + ids[4]);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (!PickingUp && collision.GetComponent<Item>())
            {
                StartCoroutine(CollisionTimer());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsServer)
        {
            if (TouchingObjects.Contains(other.GetComponent<Item>()))
            {
                TouchingObjects.Remove(other.GetComponent<Item>());
            }
        }

        if (other.gameObject.tag == "DOOR" && IsLocalPlayer)
        {
            GameObject.FindObjectOfType<OfflinePlayerHolder>().IsTeleporting = false;
        }
    }

    public IEnumerator CollisionTimer()
    {
        PickingUp = true;
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < TouchingObjects.Count; i++)
        {
            var item = TouchingObjects[i];
            if (item && Inventory.Container.Count <= 4 && !item.move)
            {
                Inventory.AddItem(item.item, 1, this.Owner);
                MyCore.NetDestroyObject(item.NetId);
            }
            yield return new WaitForSeconds(0.15f);
        }
        PickingUp = false;
    }
}
