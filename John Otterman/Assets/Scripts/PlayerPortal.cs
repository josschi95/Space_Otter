using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPortal : MonoBehaviour, IPooledObject
{
    [SerializeField] private PlayerPortal_Child subPortal;
    [SerializeField] private float timeToDespawn = 5f;
    private float despawnTimer;
    private bool isActive;

    private Dimension primaryDimension;
    private Dimension secondaryDimension;

    private void Start()
    {
        PlayerController.instance.onNewPortal += OnNewPortal;
    }

    private void OnNewPortal()
    {
        if (isActive) OnReturnToPool();
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        
        despawnTimer = timeToDespawn;
    }

    public void OnReturnToPool()
    {
        if (!isActive) return;
        isActive = false;
        subPortal.OnDeactivate();
        ObjectPooler.Return("playerPortal", gameObject);
    }

    public void OnSceneChange()
    {
        //Do nothing
    }

    //Sets the layer of the portal, and the layer of its child
    //Should allow going back and forth
    public void SetDimension(Dimension fromDimension)
    {
        primaryDimension = fromDimension;

        if (primaryDimension == Dimension.Dimension_01)
        {
            secondaryDimension = Dimension.Dimension_00;
        }
        else
        {
            secondaryDimension = Dimension.Dimension_01;
        }

        //Debug.Log("Primary: " + primaryDimension.ToString() + ", " + "Secondary: " + secondaryDimension.ToString());

        int newLayer = LayerMask.NameToLayer(primaryDimension.ToString());
        gameObject.layer = newLayer;

        subPortal.OnActivate(primaryDimension, secondaryDimension);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("On Enter");
        if (!isActive) return;
        var target = collision.gameObject.GetComponentInChildren<IDimensionHandler>();
        if (target != null)
        {
            if (target.GetDimension() == primaryDimension) target.SetDimension(secondaryDimension);
            else if (collision.gameObject.GetComponent<Projectile>())
            {
                Debug.Log("Projectile Failed to Register");
                //Debug.Break();
            }
        }
    }

    private void Update()
    {
        if (isActive)
        {
            despawnTimer -= Time.deltaTime;

            if (despawnTimer <= 0)
            {
                OnReturnToPool();
            }
        }
    }
}
