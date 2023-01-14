using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject, IProjectile, IDimensionHandler
{
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private bool isActive;
    private int damage;
    private Dimension dimension;
    [SerializeField] private float movementSpeed;
    [SerializeField] private string poolTag;
    [SerializeField] private float timeToDespawn = 5f;
    [SerializeField] private bool playerProjectile = false;

    private float despawnTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        if (isActive)
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0) OnReturnToPool();
        }
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        despawnTimer = timeToDespawn;
        rb.velocity = transform.right * movementSpeed;
    }

    public void SetDamage(int dmg, Dimension dimension)
    {
        damage = dmg;
        SetDimension(dimension);
    }

    public void OnReturnToPool()
    {
        if (!isActive) return;

        isActive = false;
        damage = 1;
        trail.Clear();
        ObjectPooler.Return(poolTag, gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;
        if (collision.gameObject.GetComponent<Projectile>()) return;

        var target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            if (playerProjectile) target.OnDamageEnemy(damage, dimension);
            else target.OnDamagePlayer(damage, dimension);
            OnReturnToPool();
            return;
        }
        else if (collision.collider.isTrigger) return;

        OnReturnToPool();
    }

    public void OnSceneChange()
    {
        OnReturnToPool();
    }

    public void SetDimension(Dimension dimension)
    {
        this.dimension = dimension;
        int newLayer = LayerMask.NameToLayer(dimension.ToString());
        gameObject.layer = newLayer;
        //Debug.Log("Current Layer: " + gameObject.layer);
    }
}
