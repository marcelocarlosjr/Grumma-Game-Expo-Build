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

    public float MoveSpeed;
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE" && IsServer)
        {
            MoveInput = VectorFromString(value);
        }
        if(flag == "AIM" && IsServer)
        {
            AimDir = VectorFromString(value);
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public static Vector2 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector2(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()));
    }

    public Vector2 GlobalMousePos(Vector2 position)
    {
        Vector3 mousePos = new Vector3(position.x, position.y, Mathf.Infinity);

        Ray rayPos = Camera.main.ScreenPointToRay(mousePos);

        Vector2 relative = (rayPos.origin - MyRig.transform.position).normalized;

        return relative;
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.01f);
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
            SendCommand("AIM", GlobalMousePos(AimInput).ToString());
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
            MyRig.transform.up = AimDir;
        }
    }
}
