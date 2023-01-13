using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public delegate void OnWeaponChangeCallback();
    public OnWeaponChangeCallback onNewWeaponEquipped;
    public OnWeaponChangeCallback onWeaponReload;

    public bool TESTING;

    private Transform pointer;
    private Animator anim;
    [SerializeField] private PlayerWeapon[] m_playerWeapons;
    public PlayerWeapon[] PlayerWeapons => m_playerWeapons;

    //Attacking
    public PlayerWeapon currentWeapon { get; private set; }
    [HideInInspector] public ActiveWeapon currentActiveWeapon; //This is to reference the gameObject

    private int weaponDamage = 1;
    private float attackRate = 5;
    private float attackCooldownTimer;

    [SerializeField] private int[] m_weaponAmmoCount;
    public int[] WeaponAmmoCount => m_weaponAmmoCount;

    private int[] weaponMaxAmmoCapacity = { 0, 0, 0, 0, 0, 0 };

    public bool isReloading { get; private set; }
    private bool canAttack = true;

    private Coroutine reloadCoroutine;

    private void Awake()
    {
        m_weaponAmmoCount = new int[6];
        weaponMaxAmmoCapacity = new int[6];
    }

    private void Start()
    {
        pointer = transform.Find("Pointer").gameObject.transform;
        anim = GetComponentInChildren<Animator>();
        InitializeWeaponry();
    }

    private void InitializeWeaponry()
    {
        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            m_playerWeapons[i].magazineCapacity = m_playerWeapons[i].weapon.GetMagazineCapacity(m_playerWeapons[i].magazineCapacityTier);
            weaponMaxAmmoCapacity[i] = m_playerWeapons[i].weapon.GetAmmoCapacity(m_playerWeapons[i].ammoCapacityTier);

            if (TESTING) OnGainAmmo(100, m_playerWeapons[i].weapon.Type);
            OnReloadComplete(m_playerWeapons[i]);
        }

        OnNewWeaponEquipped(m_playerWeapons[0]);
    }

    //Refills all weapons on a scene change
    public void RefillAllWeapons()
    {
        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            m_playerWeapons[i].roundsInMagazine = m_playerWeapons[i].weapon.GetMagazineCapacity(m_playerWeapons[i].ammoCapacityTier);
            m_weaponAmmoCount[i] = weaponMaxAmmoCapacity[i];
        }
    }

    //Enable or disable the player's combat capabilities depending on if they are in the Hub or not
    public void ToggleCombat(bool canAttack)
    {
        this.canAttack = canAttack;
    }

    public void OnSwapWeapons(int weaponSlot)
    {
        int index = weaponSlot - 1;

        if (!m_playerWeapons[index].isUnlocked) return;

        OnNewWeaponEquipped(m_playerWeapons[index]);
    }

    public void OnNewWeaponEquipped(PlayerWeapon newWeapon)
    {
        if (isReloading) OnReloadInterrupt();

        if (currentActiveWeapon != null)
        {
            ObjectPooler.Return(currentWeapon.weapon.ItemName + "_player", currentActiveWeapon.gameObject);
        }

        currentWeapon = newWeapon;

        attackRate = currentWeapon.weapon.GetAttackRate(currentWeapon.attackRateTier);
        weaponDamage = currentWeapon.weapon.GetDamage(currentWeapon.damageTier);

        var go = ObjectPooler.Spawn(newWeapon.weapon.ItemName + "_player", pointer.position, pointer.rotation);

        currentActiveWeapon = go.GetComponent<ActiveWeapon>();
        go.transform.SetParent(pointer);

        //If the weapon magazine is empty, reload
        if (currentWeapon.roundsInMagazine == 0) OnReload();
        if (currentWeapon.weapon.Type == WeaponType.Sword) AudioManager.PlayClip("sword_reload");
        else AudioManager.PlayClip("gun_reload");
        onNewWeaponEquipped?.Invoke();
    }

    public void OnAttack()
    {
        if (attackCooldownTimer > Time.time || !canAttack || isReloading || Time.timeScale == 0) return;

        if (currentWeapon.weapon.Type == WeaponType.Sword) OnSwingWeapon();
        else OnProjectileAttack();

        attackCooldownTimer = Time.time + (1 / attackRate);
    }

    private void OnProjectileAttack()
    {
        //No ammo left, reload
        if (currentWeapon.roundsInMagazine <= 0)
        {
            OnReload();
            return;
        }

        var bullet = ObjectPooler.Spawn(currentWeapon.weapon.ProjectilePoolTag, currentActiveWeapon.Muzzle.position, pointer.rotation);
        bullet.GetComponent<IProjectile>()?.SetDamage(weaponDamage, PlayerController.instance.CurrentDimension);

        AudioManager.PlayClip(currentWeapon.weapon.AttackSFX.name);//SFX
        CameraController.ShakeCamera(0.1f, 0.1f); //Camera Shake
        PlayerController.instance.AddWeaponRecoil(currentWeapon.weapon.Recoil); //Player knockback
        Reticle.Shake(0.1f, 0.1f); //Shake the reticle
        currentActiveWeapon.PlayEffects(); //Muzzle Flash

        currentWeapon.roundsInMagazine--;
        if (currentWeapon.roundsInMagazine <= 0) OnReload();
    }

    private float sizeX = 4, sizeY = 6;
    public void OnSwingWeapon()
    {
        //Swing sword
        AudioManager.PlayClip(currentWeapon.weapon.AttackSFX);
        CameraController.ShakeCamera(0.1f, 0.1f); //Camera Shake
        Reticle.Shake(0.1f, 0.1f); //Shake the reticle
        currentActiveWeapon.PlayEffects(); //Muzzle Flash

        var pos = currentActiveWeapon.Muzzle.position + (Vector3.right * anim.GetFloat("horizontal"));
        Collider2D[] colls = Physics2D.OverlapBoxAll(pos, new Vector2(sizeX, sizeY), 0);
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].gameObject == gameObject) continue;
            var target = colls[i].GetComponent<IDamageable>();
            if (target != null) target.OnDamage(weaponDamage, PlayerController.instance.CurrentDimension);
            else
            {
                //Destroy enemy bullets
                colls[i].GetComponent<Projectile>()?.OnReturnToPool();
            }
        }
    }

    #region - Reloading -
    public void OnReload()
    {
        if (isReloading) return; //Already reloading
        if (currentWeapon.roundsInMagazine == currentWeapon.magazineCapacity) return; //magazine already at capacity
        if (currentWeapon.weapon.Type == WeaponType.Sword || currentWeapon.weapon.Type == WeaponType.MiniGun) return; //can't reload a sword
        if (m_weaponAmmoCount[(int)currentWeapon.weapon.Type] <= 0) return; //No Ammo to reload

        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(ReloadTimer());
        AudioManager.PlayClip("gun_reload");
        onWeaponReload?.Invoke();
    }

    private IEnumerator ReloadTimer()
    {
        isReloading = true;
        yield return new WaitForSeconds(currentWeapon.weapon.ReloadTime);

        AudioManager.PlayClip("gun_racking");
        OnReloadComplete(currentWeapon);
    }

    private void OnReloadInterrupt()
    {
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        isReloading = false;
    }

    private void OnReloadComplete(PlayerWeapon playerWeapon)
    {
        isReloading = false;
        
        //The number of rounds which can be loaded into magazine
        int spaceInMagazine = playerWeapon.magazineCapacity - playerWeapon.roundsInMagazine;

        //The player has enough ammunition to fully load the magazine
        if (spaceInMagazine <= m_weaponAmmoCount[(int)playerWeapon.weapon.Type])
        {
            m_weaponAmmoCount[(int)playerWeapon.weapon.Type] -= spaceInMagazine;
            playerWeapon.roundsInMagazine = playerWeapon.magazineCapacity;
        }
        //The player does not have enough ammunition to fully load magazine
        else
        {
            playerWeapon.roundsInMagazine += m_weaponAmmoCount[(int)playerWeapon.weapon.Type];
            m_weaponAmmoCount[(int)playerWeapon.weapon.Type] = 0;
        }
    }
    #endregion

    public void OnNewWeaponUnlocked(int index)
    {
        m_playerWeapons[index].isUnlocked = true;
    }

    public void OnWeaponUpgraded(int index, int stat)
    {
        if (stat == 0) //Damage
        {
            m_playerWeapons[index].damageTier++;
        }
        else if (stat == 1) //Attack rate
        {
            m_playerWeapons[index].attackRateTier++;
        }
        else if (stat == 2) //Ammo cap
        {
            m_playerWeapons[index].ammoCapacityTier++;
        }
        else if (stat == 3) //Magazine
        {
            m_playerWeapons[index].magazineCapacityTier++;
        }

        //Reload current stats
        attackRate = currentWeapon.weapon.GetAttackRate(currentWeapon.attackRateTier);
        weaponDamage = currentWeapon.weapon.GetDamage(currentWeapon.damageTier);

        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            //Refresh weapon magazine capacities
            m_playerWeapons[i].magazineCapacity = m_playerWeapons[i].weapon.GetMagazineCapacity(m_playerWeapons[i].magazineCapacityTier);
            //Refresh max ammo capacities
            weaponMaxAmmoCapacity[i] = m_playerWeapons[i].weapon.GetAmmoCapacity(m_playerWeapons[i].ammoCapacityTier);
        }
    }

    public void OnGainAmmo(int count, WeaponType type)
    {
        //Add ammo
        m_weaponAmmoCount[(int)type] += count;
        //Clamp by max value
        if (m_weaponAmmoCount[(int)type] > weaponMaxAmmoCapacity[(int)type])
            m_weaponAmmoCount[(int)type] = weaponMaxAmmoCapacity[(int)type];
    }

    public void SetSavedValues(int[] ammoCount, PlayerWeapon[] weaponValues)
    {
        m_weaponAmmoCount = new int[6];
        for (int i = 0; i < m_weaponAmmoCount.Length; i++)
        {
            m_weaponAmmoCount[i] = ammoCount[i];
        }

        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            m_playerWeapons[i].isUnlocked = weaponValues[i].isUnlocked;
            m_playerWeapons[i].roundsInMagazine = weaponValues[i].roundsInMagazine;

            m_playerWeapons[i].damageTier = weaponValues[i].damageTier;
            m_playerWeapons[i].attackRateTier = weaponValues[i].attackRateTier;
            m_playerWeapons[i].ammoCapacityTier = weaponValues[i].ammoCapacityTier;
            m_playerWeapons[i].magazineCapacityTier = weaponValues[i].magazineCapacityTier;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (currentActiveWeapon != null && currentWeapon.weapon.Type == WeaponType.Sword)
        {
            var pos = currentActiveWeapon.Muzzle.position + (Vector3.right * anim.GetFloat("horizontal"));
            Gizmos.DrawWireCube(pos, new Vector3(sizeX, sizeY));
            Gizmos.color = Color.red;
        }
    }
}

[System.Serializable]
public class PlayerWeapon
{
    public Weapon weapon;
    public int roundsInMagazine;
    public int magazineCapacity;

    [Space]
    public int damageTier;
    public int attackRateTier;
    public int ammoCapacityTier;
    public int magazineCapacityTier;
    [Space]
    public bool isUnlocked;

    public PlayerWeapon(Weapon weapon, int rounds = 0, int dmgTier = 1, int rateTier = 1, int ammoCapTier = 1, int magCapTier = 1)
    {
        this.weapon = weapon;
        roundsInMagazine = rounds;

        damageTier = dmgTier;
        attackRateTier = rateTier;
        ammoCapacityTier = ammoCapTier;
        magazineCapacityTier = magCapTier;

        magazineCapacity = weapon.GetMagazineCapacity(magazineCapacityTier);

        isUnlocked = false;
        if (weapon.Type == WeaponType.Sword 
            || weapon.Type == WeaponType.Pistol) isUnlocked = true;
    }
}