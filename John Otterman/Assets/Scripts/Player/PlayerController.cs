using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable, IDimensionHandler
{
    public static PlayerController instance;

    #region - Callbacks -
    public delegate void OnHealthUpgradeCallback();
    public OnHealthUpgradeCallback onHealthUpgrade;
    public OnHealthUpgradeCallback onArmorUpgrade;

    public delegate void OnHealthChangeCallback();
    public OnHealthChangeCallback onHealthChange;
    public OnHealthChangeCallback onArmorChange;

    public delegate void OnPlayerDimensionChangeCallback(Dimension newDimension);
    public OnPlayerDimensionChangeCallback onPlayerDimensionChange;
    #endregion

    #region - Components -
    public PlayerCombat combat { get; private set; }
    public PlayerInventory inventory { get; private set; }
    private Rigidbody2D rb;
    private Animator anim;
    #endregion

    #region - Health -
    private int m_maxHealth = 10, m_maxArmor = 10;
    public int MaxHealth => m_maxHealth;
    public int MaxArmor => m_maxArmor;
    public int currentHealth { get; private set; }
    public int currentArmor { get; private set; }
    private float invincibilityTime = 0.5f;
    private float damageCooldown;
    [SerializeField] private bool isInvincible;
    private bool isAlive = true;
    #endregion

    #region - Movement -
    private Transform pointer;
    [SerializeField] private float movementSpeed = 12;
    private float verticalInput, horizontalInput;
    private Vector3 movementDirection; //movment input direction
    private Vector3 mousePosition; //Cursor Positioning
    private Transform interactionCheckPosition;
    private bool canMove = true;
    #endregion

    [SerializeField] private Dimension m_currentDimension;
    public Dimension CurrentDimension => m_currentDimension;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one isntance of PlayerController found");
            Destroy(gameObject);
            return;
        }

        instance = this;

        combat = GetComponent<PlayerCombat>();
        inventory = GetComponent<PlayerInventory>();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        pointer = transform.Find("Pointer").gameObject.transform;
        interactionCheckPosition = transform.Find("interactPos").gameObject.transform;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        currentHealth = m_maxHealth;
        currentArmor = 0;
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetPlayerInput()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        movementDirection = transform.right * horizontalInput + transform.up * verticalInput;
        movementDirection.z = 0;

        if (Input.GetKeyDown(KeyCode.E)) OnInteract();
        if (Input.GetKeyDown(KeyCode.R)) combat.OnReload();
        if (Input.GetKeyDown(KeyCode.Q)) CycleDimension();

        if (Input.GetMouseButton(0)) combat.OnAttack();

        //Weapon Selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) combat.OnSwapWeapons(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) combat.OnSwapWeapons(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) combat.OnSwapWeapons(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) combat.OnSwapWeapons(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) combat.OnSwapWeapons(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) combat.OnSwapWeapons(6);

        if (Input.GetKeyDown(KeyCode.Escape)) UIManager.PauseGame();
    }

    private void OnInteract()
    {
        var coll = Physics2D.OverlapCircle(interactionCheckPosition.position, 0.2f);
        coll?.GetComponent<IInteractable>()?.OnInteract();
    }

    #region - Movement -
    private void MovePlayer()
    {
        //Move Player
        if (canMove) transform.position += movementDirection.normalized * movementSpeed * Time.deltaTime;

        Vector3 aimDirection = (Reticle.GetPos() - pointer.transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        pointer.eulerAngles = new Vector3(0, 0, angle);
        combat.currentActiveWeapon.FlipSprite(!(angle > -90 && angle < 90));

        SetAnimatorParameters(aimDirection);

        interactionCheckPosition.localPosition = (mousePosition - transform.position).normalized;
        //Debug.DrawRay(transform.position, (mousePosition - transform.position).normalized, Color.green);
        //Debug.DrawLine(pointer.transform.position, Reticle.GetPos());
    }

    private void SetAnimatorParameters(Vector3 facingDirection)
    {
        if (!canMove) return;

        var speed = Mathf.Abs(horizontalInput) + Math.Abs(verticalInput);

        //if the player is moving, and if they are looking in the same direction as movement
        if (facingDirection.x > 0 && horizontalInput < 0) speed *= -1;
        else if (facingDirection.x < 0 && horizontalInput > 0) speed *= -1;

        anim.SetFloat("speed", speed);
        anim.SetFloat("horizontal", facingDirection.x); 
    }

    public void AddWeaponRecoil(float magnitude)
    {
        Vector3 aimDirection = (Reticle.GetPos() - transform.position).normalized;
        rb.AddForce(-aimDirection * magnitude, ForceMode2D.Impulse);
    }

    public void ToggleMovement(bool canMove)
    {
        this.canMove = canMove;

        anim.SetFloat("speed", 0);
        anim.SetFloat("horizontal", 0);
    }
    #endregion

    #region - Health -
    public void OnDamage(int dmg, Dimension dimension)
    {
        if (Time.time < damageCooldown || isInvincible || !isAlive) return;
        //if (dimension != m_currentDimension) return;

        if (currentArmor > 0)
        {
            if (currentArmor >= dmg)
            {
                currentArmor -= dmg;
                onArmorChange?.Invoke();
                return;
            }
            else
            {
                dmg -= currentArmor;
                currentArmor = 0;
                onArmorChange?.Invoke();
            }
        }

        currentHealth -= dmg;
        ObjectPooler.Spawn("blood", transform.position, Quaternion.identity);
        CameraController.ShakeCamera(0.1f, 0.1f);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnPlayerDeath();
        }

        damageCooldown = Time.time + invincibilityTime;
        onHealthChange?.Invoke();
    }

    public void OnDamagePlayer(int dmg, Dimension dimension)
    {
        OnDamage(dmg, dimension);
    }

    public void OnDamageEnemy(int dmg, Dimension dimension)
    {
        //Do nothing
    }

    public void RestoreHealth(int health)
    {
        currentHealth += health;
        if (currentHealth > m_maxHealth) currentHealth = m_maxHealth;

        onHealthChange?.Invoke();
    }

    public void RestoreArmor(int armor)
    {
        currentArmor += armor;
        if (currentArmor > m_maxArmor) currentArmor = m_maxArmor;

        onArmorChange?.Invoke();
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    private void OnPlayerDeath()
    {
        if (!isAlive) return;

        isAlive = false;
        canMove = false;
        combat.ToggleCombat(false);

        //Play anim
        AudioManager.PlayClip("player_Death");
        GameManager.OnGameOver();
        UIManager.instance.OnGameOver();
    }

    public void OnHealthUpgrade()
    {
        if (m_maxHealth >= 15) return;

        m_maxHealth++;
        currentHealth++;

        onHealthUpgrade?.Invoke();
        onHealthChange?.Invoke();
    }

    public void OnArmorUpgrade()
    {
        if (m_maxArmor >= 15) return;

        m_maxArmor++;
        currentArmor++;

        onArmorUpgrade?.Invoke();
        onArmorChange?.Invoke();
    }
    #endregion

    private void CycleDimension()
    {
        int i = (int)m_currentDimension + 1;
        if (i >= Enum.GetNames(typeof(Dimension)).Length) i = 0;
        SetDimension((Dimension)i);
        //m_currentDimension = (Dimension)i;
        Debug.Log("Cycling to dimension " + m_currentDimension.ToString());
    }

    public void SetDimension(Dimension dimension)
    {
        m_currentDimension = dimension;
        int newLayer = LayerMask.NameToLayer(dimension.ToString());
        gameObject.layer = newLayer;
        onPlayerDimensionChange?.Invoke(m_currentDimension);
        //Debug.Log("Current Layer: " + gameObject.layer);
    }

    public void SetSavedValues(int maxHealth, int maxArmor)
    {
        m_maxHealth = maxHealth;
        m_maxArmor = maxArmor;

        onHealthUpgrade?.Invoke();
        onArmorUpgrade?.Invoke();
    }

    public void OnRestoreAll()
    {
        isInvincible = false;

        currentHealth = m_maxHealth;
        currentArmor = m_maxArmor;
        combat.RefillAllWeapons();

        onHealthChange?.Invoke();
        onArmorChange?.Invoke();
    }
}
