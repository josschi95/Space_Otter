using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPortal_Child : MonoBehaviour
{
    private Dimension primaryDimension;
    private Dimension secondaryDimension;

    private bool isActive;

    public void OnActivate(Dimension primary, Dimension secondary)
    {
        isActive = true;

        primaryDimension = primary;
        secondaryDimension = secondary;

        int newLayer = LayerMask.NameToLayer(secondaryDimension.ToString());
        gameObject.layer = newLayer;
    }

    public void OnDeactivate()
    {
        isActive = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        var target = collision.gameObject.GetComponentInChildren<IDimensionHandler>();
        if (target != null)
        {
            if (target.GetDimension() == secondaryDimension) target.SetDimension(primaryDimension);
            else if (collision.gameObject.GetComponent<Projectile>())
            {
                Debug.Log("Projectile");
                Debug.Break();
            }
        }
    }
}
