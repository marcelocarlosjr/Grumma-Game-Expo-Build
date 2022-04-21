using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkRigidBody2D : NetworkComponent
{
    public Vector2 LastPosition;
    public float LastRotation;
    public Vector2 LastVelocity;

    public Vector2 OffsetVelocity;

    public float Threshold = 0.1f;
    public float EThreshold = 2.5f;
    public Rigidbody2D MyRig;
    public bool UseOffsetVelocity = true;

    public static Vector2 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector2(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()));
    }

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "POS")
        {
            LastPosition = VectorFromString(value);
            float d = (MyRig.position - LastPosition).magnitude;
            if (d > EThreshold || !UseOffsetVelocity || LastVelocity.magnitude < 0.1f)
            {
                OffsetVelocity = Vector2.zero;
                MyRig.position = LastPosition;
            }
            else
            {
                OffsetVelocity = (LastPosition - MyRig.position);
            }
        }
        if (flag == "VEL")
        {
            LastVelocity = VectorFromString(value);
        }
        if (flag == "ROT")
        {
            LastRotation = float.Parse(value);
            MyRig.rotation = LastRotation;
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer)
            {
                if ((LastPosition - MyRig.position).magnitude > Threshold)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"), false);
                    LastPosition = MyRig.position;
                }
                if ((LastVelocity - MyRig.velocity).magnitude > Threshold)
                {
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"), false);
                    LastVelocity = MyRig.velocity;
                }
                if ((Mathf.Abs(LastRotation - MyRig.rotation) > Threshold))
                {
                    SendUpdate("ROT", MyRig.rotation.ToString("F3"), false);
                    LastRotation = MyRig.rotation;
                }
                if (IsDirty)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"), false);
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"), false);
                    SendUpdate("ROT", MyRig.rotation.ToString("F3"), false);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    void Start()
    {
        MyRig = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsClient)
        {
            //prevents rotation desync on client
            MyRig.angularVelocity = 0;
            if (LastVelocity.magnitude < 0.05f)
            {
                OffsetVelocity = Vector2.zero;
            }
            if (UseOffsetVelocity)
            {
                MyRig.velocity = LastVelocity + OffsetVelocity;
            }
            else
            {
                MyRig.velocity = LastVelocity;
            }
        }
    }
}
