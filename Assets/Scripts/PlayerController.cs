using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;

public class PlayerController : NetworkComponent
{
    public Rigidbody2D MyRig;
    public Animator AnimController;

    public Vector2 MoveInput;
    public Vector2 AimInput;
    public Vector2 AimDir;
    public bool LFireInput;
    public bool RFireInput;
    public bool SprintInput;

    public float AimRot;

    public float MoveSpeed;
    public float SprintMod = 1;

    public string InputType;
    string KBM = "Keyboard&Mouse";
    string GP = "Gamepad";

    bool LFireCD;
    bool LFireAnimation;


    float STATE = 0;
    float IDLESTATE = 0;
    float RUNSTATE = 1;
    float LFIRESTATE = 2;

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
            if(LFireInput)
            {
                if (!LFireCD)
                {
                    StartCoroutine(LFire());
                }
            }
        }
        if (flag == "RFIRE" && IsServer)
        {
            RFireInput = bool.Parse(value);
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
        }
    }

    public override void NetworkedStart()
    {
        
    }
    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.01f);
    }
    public IEnumerator LFire()
    {
        if (IsServer)
        {
            while (LFireInput)
            {
                LFireCD = true;
                StartCoroutine(LFireAnim());
                //spawn slash animation
                yield return new WaitForSeconds(.7f);
                LFireCD = false;
            }
        }
    }
    public IEnumerator LFireAnim()
    {
        LFireAnimation = true;
        STATE = LFIRESTATE;
        yield return new WaitForSeconds(0.3f);
        LFireAnimation = false;
    }
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
        if (IsLocalPlayer)
        {
            MoveInput = context.ReadValue<Vector2>();
            SendCommand("MOVE", MoveInput.ToString());
        }
    }
    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
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
        if (IsLocalPlayer)
        {
            LFireInput = BoolFromFloat(context.ReadValue<float>());
            SendCommand("LFIRE", LFireInput.ToString());
        }
    }
    public void OnRClickInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
        {
            RFireInput = BoolFromFloat(context.ReadValue<float>());
            SendCommand("RFIRE", RFireInput.ToString());
        }
    }
    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
        {
            SprintInput = BoolFromFloat(context.ReadValue<float>());
            SendCommand("SPRINT", SprintInput.ToString());
        }
    }
    public void OnInputChange(PlayerInput input)
    {
        InputType = input.currentControlScheme;
    }
    public void LFire(bool state)
    {
        
    }
    public void RFire(bool state)
    {

    }
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
    private void Start()
    {
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
            MyRig.velocity = (MoveInput * MoveSpeed * SprintMod);
            MyRig.SetRotation(AimRot);

            if (MyRig.velocity.magnitude == 0 && !LFireAnimation)
            {
                STATE = IDLESTATE;
            }
            if (MyRig.velocity.magnitude > 0 && !LFireAnimation)
            {
                STATE = RUNSTATE;
            }
            AnimController.SetFloat("STATE", STATE);
            SendUpdate("STATE", STATE.ToString());
        }
    }
}
