using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;

public class PlayerController : NetworkComponent
{
    public Vector2 MoveInput;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.01f);
    }
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        SendCommand("MOVE", MoveInput.ToString());
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
}
