using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMapMarker : MonoBehaviour
{
    private void OnEnable()
    {
        var enemy = GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            //Debug.Log("Attaching to enemy");
            enemy.onEnemyDeath += delegate
            {
                gameObject.SetActive(false);
            };
        }
    }
}
