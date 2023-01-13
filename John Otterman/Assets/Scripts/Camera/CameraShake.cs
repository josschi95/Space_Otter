using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originPos = transform.localPosition;

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originPos.z);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originPos;
    }
}
