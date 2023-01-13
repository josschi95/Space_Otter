using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    private static Reticle instance;
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private GameObject crossHair;
    private Vector3 mousePosition;

    private void Start()
    {
        if (crossHair == null)
            crossHair = transform.GetChild(0).gameObject;
    }

    private void LateUpdate()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
    }

    private void FixedUpdate()
    {
        transform.position = mousePosition;
    }

    public Vector3 CrossHairPosition()
    {
        return crossHair.transform.position;
    }

    public static Vector3 GetPos()
    {
        return instance.CrossHairPosition();
    }

    private void ShakeReticle(float duration, float magnitude)
    {
        StartCoroutine(ReticleShake(duration, magnitude));
    }

    public static void Shake(float duration, float magnitude)
    {
        instance.ShakeReticle(duration, magnitude);
    }

    private IEnumerator ReticleShake(float duration, float magnitude)
    {
        float timeElapsed = 0;
        Vector3 originPos = crossHair.transform.localPosition;

        while (timeElapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            crossHair.transform.localPosition = new Vector3(x, y, originPos.z);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        crossHair.transform.localPosition = Vector3.zero;
    }
}
