using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;

public class PlayerController : NetworkComponent
{
    public Rigidbody2D MyRig;

    public Vector2 MoveInput;
    public Vector2 AimInput;
    public Vector2 AimDir;

    public float AimRot;

    public float MoveSpeed;

    public string InputType;
    string KBM = "Keyboard&Mouse";
    string GP = "Gamepad";

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE" && IsServer)
        {
            MoveInput = VectorFromString(value);
        }
        if(flag == "AIM" && IsServer)
        {
            AimRot = float.Parse(value);
        }
    }

    public override void NetworkedStart()
    {
        
    }
    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.01f);
    }
    public static Vector2 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector2(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()));
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

    }
    public void OnRClickInput(InputAction.CallbackContext context)
    {

    }
    public void OnSprintInput(InputAction.CallbackContext context)
    {

    }
    public void OnInputChange(PlayerInput input)
    {
        InputType = input.currentControlScheme;
    }
    private void Start()
    {
        MyRig = GetComponent<Rigidbody2D>();
        if(MyRig == null)
        {
            throw new System.Exception("ERROR: Could not find Rigidbody!");
        }
    }
    private void Update()
    {
        if (IsServer)
        {
            MyRig.velocity = (MoveInput * MoveSpeed);
            MyRig.rotation = AimRot;
        }

        if (IsLocalPlayer)
        {
            float cameraSpeed = 5f;
            Vector3 offsetVector = transform.forward * -10 + (new Vector3(AimDir.x, AimDir.y, 0) * 2);
            Vector3 targetCameraPosition = this.gameObject.transform.position + offsetVector;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, cameraSpeed * Time.deltaTime);
            Camera.main.transform.forward = new Vector3(0, 0, 1);
        }
    }
}
