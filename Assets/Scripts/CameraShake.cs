using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;

        while (duration > 0)
        {
            float x = Random.Range(-1, 1) * magnitude;
            float y = Random.Range(-1, 1) * magnitude;

            transform.position = Vector3.Lerp(this.transform.position, new Vector3(x, y, originalPosition.z), 0.6f);
            yield return new WaitForSeconds(0.15f);
            duration -= 0.15f;
        }

        transform.position = Vector3.Lerp(this.transform.position, originalPosition, 0.4f);
    }
}
