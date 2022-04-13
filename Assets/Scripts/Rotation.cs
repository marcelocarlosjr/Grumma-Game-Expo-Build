using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public GameObject FreezeRotGO;
    public GameObject ParentGO;
    public float YSpace;
    void Update()
    {
        FreezeRotGO.transform.rotation = Quaternion.AngleAxis(-1 * ParentGO.transform.rotation.z, Vector3.forward);
        var zRot = ParentGO.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        FreezeRotGO.transform.localPosition = new Vector3(Mathf.Sin(zRot) * YSpace, Mathf.Cos(zRot) * YSpace, 0);
    }
}