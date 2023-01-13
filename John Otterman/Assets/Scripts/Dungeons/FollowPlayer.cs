using UnityEngine;

//Script to make mini map marker follow the player
public class FollowPlayer : MonoBehaviour
{
    private Transform target;
    private void Start()
    {
        target = PlayerController.instance.transform;
    }

    private void LateUpdate()
    {
        transform.position = target.position;
    }
}
