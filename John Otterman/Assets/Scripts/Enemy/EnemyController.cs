using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable, IDimensionHandler
{
    public delegate void OnDamageTakenCallback();
    public OnDamageTakenCallback onDamageTaken;

    public delegate void OnEnemyDeathCallback(EnemyController enemy);
    public OnEnemyDeathCallback onEnemyDeath;

    public Animator anim { get; private set; }
    [SerializeField] private SpriteRenderer spriteRenderer;
    private EnemyCombat combat;

    // health 
    private bool m_isAlive = true;
    public bool IsAlive => m_isAlive;
    [SerializeField] private int pointsValue = 50;
    [SerializeField] private int m_maxHealth = 10;
    public int MaxHealth => m_maxHealth;
    [SerializeField] private int maxArmor;
    [SerializeField] private bool m_damageOnContact = false;
    public int currentHealth { get; private set; }
    public int currentArmor { get; private set; }

    [Space]
    [SerializeField] private float chaseRadius = 20f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private bool jelly = false;
    public bool Jelly => jelly;

    public float distanceToPlayer { get; private set; }
    public float angleToPlayer { get; private set; }

    private bool m_canAct = true;
    public bool CanAct => m_canAct;

    private bool m_canSeePlayer;
    public bool CanSeePlayer => m_canSeePlayer;

    private Transform player;

    private float dimensionSwapCooldown = 0.5f;
    private float lastDimensionSwap;
    [SerializeField] private Dimension m_currentDimension;
    public Dimension CurrentDimension => m_currentDimension;

    private void Start()
    {       
        anim = GetComponentInChildren<Animator>();
        combat = GetComponent<EnemyCombat>();

        currentHealth = m_maxHealth;

        player = GameManager.instance.player.transform;
        PlayerController.instance.onPlayerDimensionChange += OnPlayerSwitchDimension;
    }

    private void OnDestroy()
    {
        PlayerController.instance.onPlayerDimensionChange -= OnPlayerSwitchDimension;
    }

    private void Update()
    {
        if (!m_isAlive || !m_canAct) return;

        MoveCharacter();

    }

    #region - Movement -
    private void MoveCharacter()
    {
        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        combat.activeWeapon?.FlipSprite(!(angleToPlayer > -90 && angleToPlayer < 90));

        Vector2 movement = Vector2.MoveTowards(transform.position, player.transform.position, movementSpeed * Time.deltaTime);

        //RaycastHit2D hit;
        //hit = Physics2D.Raycast(transform.position, directionToPlayer, chaseRadius);
        //m_canSeePlayer = (hit.collider != null && hit.collider.GetComponentInParent<PlayerController>());
        m_canSeePlayer = Physics2D.Raycast(transform.position, directionToPlayer, chaseRadius).collider?.GetComponentInParent<PlayerController>();

            if (distanceToPlayer <= chaseRadius && distanceToPlayer > combat.AttackRadius) transform.position = movement;
        else movement = Vector2.zero;

        SetAnimatorParameters(movement, directionToPlayer);
    }

    private void SetAnimatorParameters(Vector2 movement, Vector2 facingDirection)
    {
        var speed = Mathf.Abs(movement.x) + Mathf.Abs(movement.y);

        if (facingDirection.x > 0 && movement.x < 0) speed *= -1;
        else if (facingDirection.x < 0 && movement.x > 0) speed *= -1;

        if (jelly)
        {
            spriteRenderer.flipX = speed > 0;
        }
        else
        {
            anim.SetFloat("speed", speed);
            anim.SetFloat("horizontal", facingDirection.x);
        }
    }

    public void PauseCharacter()
    {
        m_canAct = !m_canAct;
    }
    #endregion

    #region - Dimension Settings -
    public void OnPlayerSwitchDimension(Dimension dimension)
    {
        Color newColor = Color.white;
        if (dimension == m_currentDimension) newColor.a = 1f;
        else newColor.a = 0.25f;

        spriteRenderer.color = newColor;
    }

    public void SetDimension(Dimension dimension)
    {
        if (lastDimensionSwap > Time.time) return;

        m_currentDimension = dimension;
        int newLayer = LayerMask.NameToLayer(dimension.ToString());
        gameObject.layer = newLayer;
        OnPlayerSwitchDimension(PlayerController.instance.CurrentDimension);

        lastDimensionSwap = Time.time + dimensionSwapCooldown;
    }

    public Dimension GetDimension()
    {
        return m_currentDimension;
    }
    #endregion

    #region - Health -
    public void OnDamage(int dmg)
    {
        if (!m_isAlive) return;

        GameManager.OnEnemyHit();

        if (currentArmor > 0)
        {
            if (currentArmor >= dmg)
            {
                currentArmor -= dmg;
                return;
            }
            else
            {
                dmg -= currentArmor;
                currentArmor = 0;
            }
        }

        currentHealth -= dmg;
        ObjectPooler.Spawn("blood", transform.position, Quaternion.identity);

        if (currentHealth <= 0) KillEnemy();

        onDamageTaken?.Invoke();
    }

    public void OnDamagePlayer(int dmg)
    {
        //Return, no friendly fire
    }

    public void OnDamageEnemy(int dmg)
    {
        OnDamage(dmg);
    }

    private void KillEnemy()
    {
        m_isAlive = false;
        anim.SetFloat("speed", 0);
        anim.SetFloat("horizontal", 0);

        anim.enabled = false;
        transform.rotation = Quaternion.Euler(0, 0, -90);

        GetComponent<Collider2D>().enabled = false;
        //Play death anim
        AudioManager.PlayEnemyClip("enemy_Death");
        //Grant player points
        GameManager.OnScoreChange(pointsValue);
        GameManager.OnClamsGained(10);

        DropItem();

        onEnemyDeath?.Invoke(this);
    }
    #endregion

    public void SetEnemyProperties(int health, int armor, int pointsValue)
    {
        m_maxHealth = health;
        currentHealth = m_maxHealth;

        maxArmor = armor;
        currentArmor = maxArmor;

        this.pointsValue = pointsValue;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_damageOnContact) collision.gameObject.GetComponent<PlayerController>()?.OnDamage(1);
    }

    private void DropItem()
    {
        if (DungeonManager.instance == null)
        {
            ObjectPooler.Spawn("health", transform.position, Quaternion.identity);
            return;
        }

        var chanceToDropItem = Random.value;
        if (chanceToDropItem <= 0.5)
        {
            //Drops an item
            string tag = DungeonManager.instance.GetLootTag();
            ObjectPooler.Spawn(tag, transform.position, Quaternion.identity);
        }
    }
}
public enum EnemyState { Idle, Chase, Attack, Stagger }