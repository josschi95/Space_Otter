using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public delegate void OnDamageTakenCallback();
    public OnDamageTakenCallback onDamageTaken;

    public delegate void OnEnemyDeathCallback(EnemyController enemy);
    public OnEnemyDeathCallback onEnemyDeath;

    [SerializeField] private LayerMask layersToIgnore;
    public Animator anim { get; private set; }
    private EnemyCombat combat;

    // health 
    public bool isAlive { get; private set; }
    [SerializeField] private int pointsValue = 50;
    [SerializeField] private int m_maxHealth = 10;
    public int MaxHealth => m_maxHealth;
    [SerializeField] private int maxArmor;
    public int currentHealth { get; private set; }
    public int currentArmor { get; private set; }

    [Space]
    [SerializeField] private float chaseRadius = 20f;
    [SerializeField] private float movementSpeed = 5f;

    public float distanceToPlayer { get; private set; }
    public float angleToPlayer { get; private set; }

    private bool m_canAct = true;
    public bool CanAct => m_canAct;

    private bool m_canSeePlayer;
    public bool CanSeePlayer => m_canSeePlayer;

    private Transform player;

    [SerializeField] private Dimension m_currentDimension;
    public Dimension CurrentDimension => m_currentDimension;

    private void Start()
    {
        isAlive = true;
        
        currentHealth = m_maxHealth;
        player = GameManager.instance.player.transform;
        anim = GetComponentInChildren<Animator>();
        combat = GetComponent<EnemyCombat>();
    }

    private void Update()
    {
        if (!isAlive || !m_canAct) return;

        MoveCharacter();

    }

    #region - Movement -
    private void MoveCharacter()
    {
        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        combat.activeWeapon.FlipSprite(!(angleToPlayer > -90 && angleToPlayer < 90));

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

        anim.SetFloat("speed", speed);
        anim.SetFloat("horizontal", facingDirection.x);
    }

    public void PauseCharacter()
    {
        m_canAct = !m_canAct;
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
        //collision.gameObject.GetComponent<PlayerController>()?.OnDamage(1);
    }

    public void OnDamage(int dmg, Dimension dimension)
    {
        if (!isAlive || dimension != m_currentDimension) return;

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

    public void OnDamagePlayer(int dmg, Dimension dimension)
    {
        //Return, no friendly fire
    }

    public void OnDamageEnemy(int dmg, Dimension dimension)
    {
        OnDamage(dmg, dimension);
    }

    private void KillEnemy()
    {
        isAlive = false;
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

    private void DropItem()
    {
        if (DungeonManager.instance == null) return;

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