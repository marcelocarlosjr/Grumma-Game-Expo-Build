using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;
using UnityEngine.UI;
using UnityEngine.Animations;
using System;
using UnityEngine.SceneManagement;

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
    public InputActionAsset InputActionPlayer;

    public InputAction MoveAction;
    public InputAction LFireAction;
    public InputAction RFireAction;
    public InputAction SprintAction;
    public InputAction AimAction;

    public InputAction ControlsChangedEventAction;


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
    public int EXP;
    public int EXPToLevel;
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
    float HealthUpgradeMod = 10;
    float DamageUpgradeMod = 3;
    float HealthRegenerationUpgradeMod = 0.25f;
    float AttackSpeedUpgradeMod = 0;
    float EXPModUpgradeMod = 0.5f;
    float StaminaUpgradesMod = 0.25f;

    [Header("Player Info")]
    public string Name;
    public int type;
    public bool IsSafe;

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

    public bool teleport;

    public static readonly int[] LevelEXP = { 100, 120, 160, 220, 300, 400, 520, 660, 820, 1000, 1200, 1420, 1660, 1920, 2200, 2500, 2820, 3160, 3520, 3900, 4300, 4720, 5160, 5620, 6100, 6600, 7120, 7660, 8220, 8800, 9400, 10020, 10660, 11320, 12000, 12700, 13420, 14160, 14920, 15700, 16500, 17320, 18160, 19020, 19900, 20800, 21720, 22660, 23620, 24600, 25600 };

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "UPGRADE" && IsServer)
        {
            switch (value)
            {
                case "damage":
                    DamageUpgrade += 1;
                    break;
                case "health":
                    HealthUpgrade += 1;
                    break;
                case "speed":
                    MoveSpeedUpgrade += 1;
                    break;
                case "stamina":
                    StaminaUpgrade += 1;
                    break;
                case "regen":
                    HealthRegenerationUpgrade += 1;
                    break;
                case "exp":
                    EXPModUpgrade += 1;
                    break;
            }
        }
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
            if(STATE == DEADSTATE)
            {
                Die();
                if (IsLocalPlayer)
                {
                    FindObjectOfType<DisplayInventory>().CallDropAllItems(this);
                    StartCoroutine(ShowRespawn());
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
            if(int.Parse(value) > Level)
            {
                int levelDelta = int.Parse(value) - Level;
                var temp = FindObjectOfType<UpgradeController>();
                temp.setPlayer(this);
                temp.Show();
                temp.SetCurrentUpgradeAmount(levelDelta);
            }

            Level = int.Parse(value);
        }

        if(flag == "TELEPORT" && IsLocalPlayer)
        {

            OfflinePlayerHolder temp = FindObjectOfType<OfflinePlayerHolder>();
            FindObjectOfType<PlayerHealthUI>().RemovePlayer();

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

        if(flag == "NAME" && IsClient)
        {
            Name = value;
            NameBox.text = Name;
            if (IsLocalPlayer)
            {
                FindObjectOfType<OfflinePlayerHolder>().RemoveLoading();
            }
        }

        if(flag == "SAFE" && IsClient)
        {
            IsSafe = bool.Parse(value);
            if (FindObjectOfType<PVPModeUI>())
            {
                FindObjectOfType<PVPModeUI>().SetPVP(IsSafe);
            }
            if (IsLocalPlayer)
            {
                if (IsSafe)
                {
                    FindObjectOfType<AudioManager>().Play("TownLoop");
                    FindObjectOfType<AudioManager>().Pause("OverworldLoop");
                }
                else
                {
                    FindObjectOfType<AudioManager>().Pause("TownLoop");
                    FindObjectOfType<AudioManager>().Play("OverworldLoop");
                }
            }
        }
    }

    public IEnumerator ShowRespawn()
    {
        yield return new WaitForSeconds(5);
        FindObjectOfType<RespawnUI>().Show();
    }

    public override void NetworkedStart()
    {
        if (IsServer)
        {
            SendUpdate("HP", Health.ToString());
            SendUpdate("MAXHP", MaxHealth.ToString());
            TouchingObjects = new List<Item>();
            Invoke("UpDateLocal", 0.5f);
        }

        if (IsLocalPlayer)
        {
            FindObjectOfType<PlayerHealthUI>().SetPlayer(this);
            DisplayUI = FindObjectOfType<DisplayInventory>();
            Inventory = ScriptableObject.CreateInstance<InventoryObject>();
            Inventory.database = StaticItemDatabase;

            FindObjectOfType<PlayerHealthUI>().SetPlayerImage(type);

            FindObjectOfType<PlayerInput>().actions = InputActionPlayer;

            MoveAction = GetComponent<PlayerInput>().currentActionMap.FindAction("Move", true);
            MoveAction.started += this.OnMoveInput;
            MoveAction.performed += this.OnMoveInput;
            MoveAction.canceled += this.OnMoveInput;
            LFireAction = GetComponent<PlayerInput>().currentActionMap.FindAction("LFire", true);
            LFireAction.started += this.OnLClickInput;
            LFireAction.performed += this.OnLClickInput;
            LFireAction.canceled += this.OnLClickInput;
            RFireAction = GetComponent<PlayerInput>().currentActionMap.FindAction("RFire", true);
            RFireAction.started += this.OnRClickInput;
            RFireAction.performed += this.OnRClickInput;
            RFireAction.canceled += this.OnRClickInput;
            SprintAction = GetComponent<PlayerInput>().currentActionMap.FindAction("Sprint", true);
            SprintAction.started += this.OnSprintInput;
            SprintAction.performed += this.OnSprintInput;
            SprintAction.canceled += this.OnSprintInput;
            AimAction = GetComponent<PlayerInput>().currentActionMap.FindAction("Aim", true);
            AimAction.started += this.OnAimInput;
            AimAction.performed += this.OnAimInput;
            AimAction.canceled += this.OnAimInput;
            InputType = KBM;

            Camera.main.transform.position = this.transform.position;

            if(SceneManager.GetActiveScene().buildIndex == 2 && SceneManager.GetActiveScene().buildIndex == 3)
            {
                FindObjectOfType<AudioManager>().Play("InstanceLoop");
                FindObjectOfType<AudioManager>().Pause("OverworldLoop");
                FindObjectOfType<AudioManager>().Pause("MainMenuLoop");
                FindObjectOfType<AudioManager>().Pause("TownLoop");
            }
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                FindObjectOfType<AudioManager>().Pause("InstanceLoop");
                FindObjectOfType<AudioManager>().Play("OverworldLoop");
                FindObjectOfType<AudioManager>().Pause("MainMenuLoop");
                FindObjectOfType<AudioManager>().Pause("TownLoop");
            }

        }
    }
    public void OnDestroy()
    {
        MoveAction.started -= this.OnMoveInput;
        MoveAction.performed -= this.OnMoveInput;
        MoveAction.canceled -= this.OnMoveInput;
        LFireAction.started -= this.OnLClickInput;
        LFireAction.performed -= this.OnLClickInput;
        LFireAction.canceled -= this.OnLClickInput;
        RFireAction.started -= this.OnRClickInput;
        RFireAction.performed -= this.OnRClickInput;
        RFireAction.canceled -= this.OnRClickInput;
        SprintAction.started -= this.OnSprintInput;
        SprintAction.performed -= this.OnSprintInput;
        SprintAction.canceled -= this.OnSprintInput;
        AimAction.started -= this.OnAimInput;
        AimAction.performed -= this.OnAimInput;
        AimAction.canceled -= this.OnAimInput;

    }
    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            if (IsDirty)
            {
                SendUpdate("STATE", STATE.ToString());
                SendUpdate("HP", Health.ToString());
                SendUpdate("MAXHP", MaxHealth.ToString());
                SendUpdate("NAME", Name);
                SendUpdate("SAFE", IsSafe.ToString());
                SendUpdate("MAXSTAMINA", MaxStamina.ToString());
                IsDirty = false;
            }
        }
        yield return new WaitForSeconds(0.05f);
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
            if(id == -1)
            {
                Health -= damage;
                SendUpdate("HP", Health.ToString());
                StartCoroutine(TakeDamageTimer());
            }
            else
            {
                if (!IsSafe)
                {
                    Health -= damage;
                    SendUpdate("HP", Health.ToString());
                    StartCoroutine(TakeDamageTimer());
                }
                else
                {
                    return;
                }
            }


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

                        p.levelSystem.AddExperience((int)((tempEXP/3) * p.EXPMulti));
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
            MyRig.velocity = Vector2.zero;
            this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
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
            if (!DeadCycle)
            {
                FindObjectOfType<AudioManager>().Play("PlayerD");
                this.GetComponent<PlayerInput>().enabled = false;
                MyRig.velocity = Vector2.zero;
                this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
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
    public IEnumerator TakeDamageTimer()
    {
        TakingDamage = true;
        STATE = TAKEDAMAGESTATE;
        FindObjectOfType<AudioManager>().Play("PlayerTD");
        yield return new WaitForSeconds(0.10f);
        TakingDamage = false;
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
                Stamina -= 0.01f;
            }
            SendUpdate("STAMINA", Stamina.ToString());
            SendUpdate("MAXSTAMINA", MaxStamina.ToString());
            yield return new WaitForSeconds(0.1f);
        }
    }
    public IEnumerator SprintStaminaRegen()
    {
        while (!SprintInput)
        {
            if(Stamina < MaxStamina)
            {
                NoStamina = false;
                Stamina += (MaxStamina * 0.01f);
                SendUpdate("STAMINA", Stamina.ToString());
            }
            else
            {
                Stamina = MaxStamina;
                SendUpdate("STAMINA", Stamina.ToString());
                SendUpdate("MAXSTAMINA", MaxStamina.ToString());
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    public virtual void Start()
    {
        Inventory = ScriptableObject.CreateInstance<InventoryObject>();
        Inventory.database = StaticItemDatabase;
        if (!teleport)
        {
            Health = HealthBase;
            MaxHealth = HealthBase;
            MaxStamina = StaminaBase;
            Stamina = StaminaBase;
        }
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

        if (teleport)
        {
            return;
        }
        else if(!teleport)
        {
            levelSystem = new LevelSystem();
            levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
            levelSystem.OnExperienceChanged += LevelSystem_OnExperienceChanged;
        }
    }

    public IEnumerator LevelTimer(int _level, int _exp)
    {
        yield return new WaitForSeconds(1.2f);
        levelSystem = new LevelSystem(true, _level, _exp);
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
    bool RegenHealth;
    public IEnumerator HealthRegen()
    {
        RegenHealth = true;
        if(Health + HealthRegeneration <= MaxHealth)
        {
            Health += HealthRegeneration;
            SendUpdate("HP", Health.ToString());
        }
        else
        {
            SendUpdate("HP", MaxHealth.ToString());
        }
        yield return new WaitForSeconds(5);
        RegenHealth = false;
    }
    bool WalkSound;
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

            if (!RegenHealth)
            {
                StartCoroutine(HealthRegen());
            }


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
            if(levelSystem != null)
            {
                EXP = levelSystem.experience;
                SendUpdate("EXPERIENCE", EXP + "," + levelSystem.GetExperienceToNextLevel(levelSystem.GetLevelNumber()));
                Level = levelSystem.level + 1;
                SendUpdate("LEVEL", Level.ToString());
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
            if (STATE != IDLESTATE && !Dead)
            {
                AnimController.SetFloat("SPEED", MyRig.velocity.magnitude);
            }
            else
            {
                AnimController.SetFloat("SPEED", 5);
            }
        }

        if (IsLocalPlayer)
        {
            if (MoveInput.magnitude > 0)
            {
                if(!walking)
                {
                    FindObjectOfType<AudioManager>().Play("Walk");
                    walking = true;
                }
            }
            else
            {
                if (walking)
                {
                    FindObjectOfType<AudioManager>().Pause("Walk");
                    walking = false;
                }
            }
        }
    }
    bool walking;


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
                levelSystem.GetExperience() + "," +
                levelSystem.GetExperienceToNextLevel(levelSystem.GetLevelNumber()) + "," +
                (levelSystem.GetLevelNumber()+1) + "," +
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

    bool SafeSet;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (!PickingUp && collision.GetComponent<Item>())
            {
                StartCoroutine(CollisionTimer());
            }

            if(collision.gameObject.tag == "SAFE")
            {
                if (!SafeSet)
                {
                    IsSafe = true;
                    SendUpdate("SAFE", IsSafe.ToString());
                    SafeSet = true;
                }
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

        if (other.gameObject.tag == "SAFE")
        {
            if (SafeSet)
            {
                IsSafe = false;
                SendUpdate("SAFE", IsSafe.ToString());
                SafeSet = false;
            }
        }
    }

    public IEnumerator CollisionTimer()
    {
        PickingUp = true;
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < TouchingObjects.Count; i++)
        {
            var item = TouchingObjects[i];
            if (item && Inventory.Container.Count <= 4 && !item.move)
            {
                Inventory.AddItem(item.item, 1, this.Owner);
                MyCore.NetDestroyObject(item.NetId);
            }
            yield return new WaitForSeconds(0.3f);
        }
        PickingUp = false;
    }

    public IEnumerator ReplaceItems(int item1, int item2, int item3, int item4, int item5)
    {
        yield return new WaitUntil(() => IsConnected);
        yield return new WaitForSeconds(0.7f);
        if (item1 != 0)
        {
            Inventory.AddItem(Inventory.database.GetItem[item1], 1, Owner, false);
            yield return new WaitForSeconds(0.3f);
        }
        if (item2 != 0)
        {
            Inventory.AddItem(Inventory.database.GetItem[item2], 1, Owner, false);
            yield return new WaitForSeconds(0.3f);
        }
        if (item3 != 0)
        {
            Inventory.AddItem(Inventory.database.GetItem[item3], 1, Owner, false);
            yield return new WaitForSeconds(0.3f);
        }
        if (item4 != 0)
        {
            Inventory.AddItem(Inventory.database.GetItem[item4], 1, Owner, false);
            yield return new WaitForSeconds(0.3f);
        }
        if (item5 != 0)
        {
            Inventory.AddItem(Inventory.database.GetItem[item5], 1, Owner, false);
        }
    }

    public void UpDateLocal()
    {
        SendUpdate("HP", Health.ToString());
        SendUpdate("MAXHP", MaxHealth.ToString());
        SendUpdate("STAMINA", Stamina.ToString());
        SendUpdate("MAXSTAMINA", MaxStamina.ToString());
        SendUpdate("EXPERIENCE", EXP.ToString());
        //SendUpdate("LEVEL", Level.ToString());

    }

    public void SendUpgrade(string _upgrade)
    {
        SendCommand("UPGRADE", _upgrade);
    }

    public IEnumerator SetName(string _name)
    {
        yield return new WaitUntil(() => IsConnected);
        yield return new WaitForSeconds(1.5f);
        Name = _name;
        SendUpdate("NAME", Name);
    }
}
