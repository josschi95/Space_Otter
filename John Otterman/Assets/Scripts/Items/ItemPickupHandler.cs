using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupHandler : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<ICollectable>()?.OnCollect();
    }
}
