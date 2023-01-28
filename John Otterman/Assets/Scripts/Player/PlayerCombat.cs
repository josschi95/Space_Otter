using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public delegate void OnWeaponChangeCallback();
    public OnWeaponChangeCallback onNewWeaponUnlocked;
    public OnWeaponChangeCallback onNewWeaponEquipped;
    public OnWeaponChangeCallback onWeaponReload;

    public bool TESTING;

    private Transform pointer;
    private Animator anim;
    [SerializeField] private PlayerWeapon portalGun;
    [SerializeField] private PlayerWeapon[] m_playerWeapons;
    public PlayerWeapon[] PlayerWeapons => m_playerWeapons;

    //Attacking
    public PlayerWeapon currentWeapon { get; private set; }
    [HideInInspector] public ActiveWeapon currentActiveWeapon; //This is to reference the gameObject
    private int currentWeaponIndex;

    private int weaponDamage = 1;
    private float attackRate = 5;
    private float attackCooldownTimer;

    private float portalCooldown = 0.5f;
    private float timeOfNextPortal;

    [SerializeField] private int[] m_weaponAmmoCount;
    public int[] WeaponAmmoCount => m_weaponAmmoCount;

    private int[] weaponMaxAmmoCapacity;

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
            m_playerWeapons[i].magazineCapacity = m_playerWeapons[i].weapon.GetMagazineCapacity(m_playerWeapons[i].magazineCapacity_Tier);
            weaponMaxAmmoCapacity[i] = m_playerWeapons[i].weapon.GetAmmoCapacity(m_playerWeapons[i].ammoCapacity_Tier);
            OnReloadComplete(m_playerWeapons[i]);
        }

        OnNewWeaponEquipped(m_playerWeapons[0]);
        currentWeaponIndex = 0;
    }

    //Refills all weapons on a scene change
    public void RefillAllWeapons()
    {
        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            m_playerWeapons[i].roundsInMagazine = m_playerWeapons[i].weapon.GetMagazineCapacity(m_playerWeapons[i].magazineCapacity_Tier);
            m_weaponAmmoCount[i] = weaponMaxAmmoCapacity[i];
        }
    }

    //Enable or disable the player's combat capabilities depending on if they are in the Hub or not
    public void ToggleCombat(bool canAttack)
    {
        this.canAttack = canAttack;
    }

    #region - Weapon Swapping -
    public void CycleWeapons(float cycleDirection)
    {
        int newIndex = currentWeaponIndex;
        int indexModifier = Mathf.RoundToInt(cycleDirection);
        newIndex += indexModifier;

        if (newIndex > 5) newIndex = 0;
        else if (newIndex < 0) newIndex = 5;

        while(OnSwapWeapons(newIndex) == false)
        {
            newIndex += indexModifier;
            if (newIndex > 5) newIndex = 0;
            else if (newIndex < 0) newIndex = 5;
        }
    }

    public void OnSwapToPortalGun()
    {
        if (isReloading) OnReloadInterrupt();

        if (currentActiveWeapon != null)
        {
            ObjectPooler.Return(currentWeapon.weapon.ItemName, currentActiveWeapon.gameObject);
        }

        currentWeapon = portalGun;
        attackRate = currentWeapon.weapon.GetAttackRate(currentWeapon.attackRate_Tier);
        weaponDamage = currentWeapon.weapon.GetDamage(currentWeapon.damage_Tier);

        var go = ObjectPooler.Spawn(currentWeapon.weapon.ItemName, pointer.position, pointer.rotation);

        currentActiveWeapon = go.GetComponent<ActiveWeapon>();
        go.transform.SetParent(pointer);
        onNewWeaponEquipped?.Invoke();
    }

    public bool OnSwapWeapons(int index)
    {
        if (index == -1)
        {
            currentWeaponIndex = index;
            OnSwapToPortalGun();
            return true;
        }

        if (!m_playerWeapons[index].isUnlocked) return false;

        currentWeaponIndex = index;
        OnNewWeaponEquipped(m_playerWeapons[index]);
        return true;
    }

    public void OnNewWeaponEquipped(PlayerWeapon newWeapon)
    {
        if (isReloading) OnReloadInterrupt();

        if (currentActiveWeapon != null)
        {
            ObjectPooler.Return(currentWeapon.weapon.ItemName, currentActiveWeapon.gameObject);
        }

        currentWeapon = newWeapon;

        attackRate = currentWeapon.weapon.GetAttackRate(currentWeapon.attackRate_Tier);
        weaponDamage = currentWeapon.weapon.GetDamage(currentWeapon.damage_Tier);

        var go = ObjectPooler.Spawn(newWeapon.weapon.ItemName, pointer.position, pointer.rotation);

        currentActiveWeapon = go.GetComponent<ActiveWeapon>();
        go.transform.SetParent(pointer);

        //If the weapon magazine is empty, reload
        if (currentWeapon.roundsInMagazine == 0) OnReload();
        if (currentWeapon.weapon.Type == WeaponType.Sword) AudioManager.PlayClip("sword_reload");
        else AudioManager.PlayClip("gun_reload");
        onNewWeaponEquipped?.Invoke();
    }
    #endregion

    public void OnAttack()
    {
        if (attackCooldownTimer > Time.time || !canAttack || isReloading || Time.timeScale == 0) return;

        //if (currentWeapon == portalGun) OnPortalGunUse();
        if (currentWeapon.weapon.Type == WeaponType.Sword) OnSwingWeapon();
        else OnProjectileAttack();
    }

    public void OnPortalGunUse()
    {
        if (timeOfNextPortal > Time.time) return;
        PlayerController.instance.onNewPortal?.Invoke();

        var portalRot = Quaternion.Euler(0, 0, pointer.rotation.eulerAngles.z + 90);

        var pos = currentActiveWeapon.Muzzle.position + (Vector3.right * anim.GetFloat("horizontal") * 2);

        var portal = ObjectPooler.Spawn("playerPortal", pos, portalRot);
        portal.GetComponent<PlayerPortal>()?.SetDimension(PlayerController.instance.CurrentDimension);

        timeOfNextPortal = Time.time + portalCooldown;
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

        if (!PlayerController.instance.godMode) currentWeapon.roundsInMagazine--;       
        if (currentWeapon.roundsInMagazine <= 0) OnReload();

        attackCooldownTimer = Time.time + (1 / attackRate);
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
            if (colls[i].gameObject == gameObject || colls[i].gameObject.layer != gameObject.layer) continue;
            var target = colls[i].GetComponent<IDamageable>();
            if (target != null) target.OnDamage(weaponDamage);
            else
            {
                //Destroy enemy bullets
                colls[i].GetComponent<Projectile>()?.OnReturnToPool();
            }
        }
        attackCooldownTimer = Time.time + (1 / attackRate);
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
        if (spaceInMagazine == 0) return;

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
        onNewWeaponUnlocked?.Invoke();
    }

    public void OnWeaponUpgraded(int index, int stat)
    {
        if (stat == 0) //Damage
        {
            m_playerWeapons[index].damage_Tier++;
        }
        else if (stat == 1) //Attack rate
        {
            m_playerWeapons[index].attackRate_Tier++;
        }
        else if (stat == 2) //Ammo cap
        {
            m_playerWeapons[index].ammoCapacity_Tier++;
        }
        else if (stat == 3) //Magazine
        {
            m_playerWeapons[index].magazineCapacity_Tier++;
        }

        //Reload current stats
        attackRate = currentWeapon.weapon.GetAttackRate(currentWeapon.attackRate_Tier);
        weaponDamage = currentWeapon.weapon.GetDamage(currentWeapon.damage_Tier);

        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            //Refresh weapon magazine capacities
            m_playerWeapons[i].magazineCapacity = m_playerWeapons[i].weapon.GetMagazineCapacity(m_playerWeapons[i].magazineCapacity_Tier);
            //Refresh max ammo capacities
            weaponMaxAmmoCapacity[i] = m_playerWeapons[i].weapon.GetAmmoCapacity(m_playerWeapons[i].ammoCapacity_Tier);
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

    public void SetSavedValues(PlayerWeapon[] weaponValues)
    {
        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            m_playerWeapons[i].isUnlocked = weaponValues[i].isUnlocked;

            m_playerWeapons[i].damage_Tier = weaponValues[i].damage_Tier;
            m_playerWeapons[i].attackRate_Tier = weaponValues[i].attackRate_Tier;
            m_playerWeapons[i].ammoCapacity_Tier = weaponValues[i].ammoCapacity_Tier;
            m_playerWeapons[i].magazineCapacity_Tier = weaponValues[i].magazineCapacity_Tier;
        }
    }

    public void OnResetValues()
    {
        for (int i = 0; i < m_playerWeapons.Length; i++)
        {
            if (i <= 1) m_playerWeapons[i].isUnlocked = true;
            else m_playerWeapons[i].isUnlocked = false;

            m_playerWeapons[i].damage_Tier = 1;
            m_playerWeapons[i].attackRate_Tier = 1;
            m_playerWeapons[i].ammoCapacity_Tier = 1;
            m_playerWeapons[i].magazineCapacity_Tier = 1;
        }

        InitializeWeaponry();
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
    public int damage_Tier;
    public int attackRate_Tier;
    public int ammoCapacity_Tier;
    public int magazineCapacity_Tier;
    [Space]
    public bool isUnlocked;

    public PlayerWeapon(Weapon weapon, int rounds = 0, int dmgTier = 1, int rateTier = 1, int ammoCapTier = 1, int magCapTier = 1)
    {
        this.weapon = weapon;
        roundsInMagazine = rounds;

        damage_Tier = dmgTier;
        attackRate_Tier = rateTier;
        ammoCapacity_Tier = ammoCapTier;
        magazineCapacity_Tier = magCapTier;

        magazineCapacity = weapon.GetMagazineCapacity(magazineCapacity_Tier);

        isUnlocked = false;
        if (weapon.Type == WeaponType.Sword 
            || weapon.Type == WeaponType.Pistol) isUnlocked = true;
    }
}