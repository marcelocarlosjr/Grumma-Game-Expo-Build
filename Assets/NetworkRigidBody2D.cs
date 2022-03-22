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
    public float LastAngular;

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
        if (flag == "POS" && IsClient)
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
        if (flag == "VEL" && IsClient)
        {
            LastVelocity = VectorFromString(value);
        }
        if (flag == "ROT" && IsClient)
        {
            LastRotation = float.Parse(value);
            MyRig.rotation = LastRotation;
        }
        if (flag == "ANG" && IsClient)
        {
            LastAngular = float.Parse(value);
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
                if ((LastRotation - MyRig.rotation > Threshold))
                {
                    SendUpdate("ROT", MyRig.rotation.ToString("F3"), false);
                    LastRotation = MyRig.rotation;
                }
                if ((LastAngular - MyRig.angularVelocity > Threshold))
                {
                    SendUpdate("ANG", MyRig.angularVelocity.ToString("F3"), false);
                    LastAngular = MyRig.angularVelocity;
                }
                if (IsDirty)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"), false);
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"), false);
                    SendUpdate("ROT", MyRig.rotation.ToString("F3"), false);
                    SendUpdate("ANG", MyRig.angularVelocity.ToString("F3"), false);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    void Start()
    {
        MyRig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient)
        {
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

            MyRig.angularVelocity = LastAngular;
        }
    }
}
