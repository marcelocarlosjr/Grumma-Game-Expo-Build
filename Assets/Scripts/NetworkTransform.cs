using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class NetworkTransform : NetworkComponent
{
    public Vector3 LastPosition;
    public Vector3 LastRotation;
    public Vector3 LastScale;

    public float MinThreshold = 0.1f;
    public float MaxThreshold = 0.5f;

    public bool Smooth = true;

    public float NavMeshSpeed = 1;
    public float RotationSpeed = Mathf.PI / 2;

    public static Vector3 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector3(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
    }
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "POS")
        {
            Vector3 temp = NetworkTransform.VectorFromString(value);
            if ((temp - this.transform.position).magnitude > MaxThreshold || !Smooth)
            {
                this.transform.position = temp;
            }
            LastPosition = temp;
        }

        if (flag == "ROT")
        {
            Vector3 temp = NetworkTransform.VectorFromString(value);
            if ((temp - this.transform.rotation.eulerAngles).magnitude < MaxThreshold || !Smooth)
            {
                Quaternion qt = new Quaternion();
                qt.eulerAngles = temp;

                this.transform.rotation = qt;
            }
            LastRotation = temp;
        }

        if (flag == "SCALE")
        {
            Vector3 temp = NetworkTransform.VectorFromString(value);
            this.transform.localScale = temp;
            LastScale = temp;
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
                float DistCheck = (this.transform.position - LastPosition).magnitude;
                if (DistCheck > MinThreshold)
                {
                    SendUpdate("POS", this.transform.position.ToString("F2"));
                    LastPosition = this.transform.position;
                }
                float CheckRotation = (this.transform.rotation.eulerAngles - LastRotation).magnitude;
                if (CheckRotation > MinThreshold)
                {
                    SendUpdate("ROT", this.transform.rotation.eulerAngles.ToString("F2"));
                    LastRotation = this.transform.rotation.eulerAngles;
                }
                float CheckScale = (this.transform.localScale - LastScale).magnitude;
                if (CheckScale > MinThreshold)
                {
                    SendUpdate("SCALE", this.transform.localScale.ToString("F3"));
                    LastScale = this.transform.localScale;
                }
                if (IsDirty)
                {
                    SendUpdate("POS", this.transform.position.ToString("F2"));
                    SendUpdate("ROT", this.transform.rotation.eulerAngles.ToString("F2"));
                    SendUpdate("SCALE", this.transform.localScale.ToString("F3"));
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (IsClient && Smooth)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, LastPosition, 0.1f);
            //Quaternion qt = new Quaternion();
            //qt.eulerAngles = Vector3.Lerp(this.transform.rotation.eulerAngles, LastRotation, RotationSpeed * Time.deltaTime);
            //this.transform.rotation = qt;
        }
    }
}
