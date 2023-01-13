using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController instance;
    private void Awake()
    {
        instance = this;
    }

    private Transform player;
    private CameraShake cameraShake;
    //[SerializeField] private GameObject minimapCamera;

    private void Start()
    {
        player = PlayerController.instance.transform;
        cameraShake = GetComponentInChildren<CameraShake>();
    }

    private void LateUpdate()
    {
        float z = transform.position.z;
        transform.position = new Vector3(player.position.x, player.position.y, z);
    }

    public static void ShakeCamera(float duration, float magnitude)
    {
        instance.StartShake(duration, magnitude);
    }

    private void StartShake(float duration, float magnitude)
    {
        StartCoroutine(cameraShake.Shake(duration, magnitude));
    }
}
