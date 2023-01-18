using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPooledObject, IProjectile, IDimensionHandler
{
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private SpriteRenderer spriteRenderer;
    private bool isActive;
    private int damage;

    private Dimension dimension;
    private float dimensionSwapCooldown = 0.5f;
    private float lastDimensionSwap;

    private Color spriteColor;
    private Color trailStartColor;
    private Color trailEndColor;

    [SerializeField] private float movementSpeed;
    [SerializeField] private string poolTag;
    [SerializeField] private float timeToDespawn = 5f;
    [SerializeField] private bool playerProjectile = false;

    private float despawnTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteColor = spriteRenderer.color;
        trailStartColor = trail.startColor;
        trailEndColor = trail.endColor;
    }

    private void Start()
    {
        PlayerController.instance.onPlayerDimensionChange += OnPlayerSwitchDimension;
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
        lastDimensionSwap = Time.time - 1;
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
            if (playerProjectile) target.OnDamageEnemy(damage);
            else target.OnDamagePlayer(damage);
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
        if (lastDimensionSwap > Time.time) return;

        this.dimension = dimension;
        int newLayer = LayerMask.NameToLayer(dimension.ToString());
        gameObject.layer = newLayer;
        OnPlayerSwitchDimension(PlayerController.instance.CurrentDimension);

        lastDimensionSwap = Time.time + dimensionSwapCooldown;
    }

    public void OnPlayerSwitchDimension(Dimension dimension)
    {
        Color newSpriteColor = spriteColor;
        Color newTrailStart = trailStartColor;
        Color newTrailEnd = trailEndColor;

        if (dimension == this.dimension)
        {
            newSpriteColor.a = 1f;
            newTrailStart.a = 1f;
            newTrailEnd.a = 1f;
        }
        else
        {
            newSpriteColor.a = 0.25f;
            newTrailStart.a = 0.25f;
            newTrailEnd.a = 0.25f;
        }

        spriteRenderer.color = newSpriteColor;
        trail.startColor = newTrailStart;
        trail.endColor = newTrailEnd;
    }

    public Dimension GetDimension()
    {
        return dimension;
    }
}
