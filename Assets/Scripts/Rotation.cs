using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    void Update()
    {
        this.transform.rotation = Quaternion.AngleAxis(-1 * transform.parent.transform.rotation.z, Vector3.forward);
        var zRot = transform.parent.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        this.transform.localPosition = new Vector3(Mathf.Sin(zRot) * -1f, Mathf.Cos(zRot) * -1f, 0);
    }
}