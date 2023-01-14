using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private const float ATTACK_RATE_PENALTY = 0.5f;

    private EnemyController controller;
    private Transform pointer;

    private float attackRate;
    private float attackCooldownTimer;
    [SerializeField] private float attackRadius = 17.5f;
    public float AttackRadius => attackRadius;
    [SerializeField] private int damageModifier = 0;
    public Weapon defaultWeapon;
    public ActiveWeapon activeWeapon { get; private set; }
    [SerializeField] private GameObject sharkHands;
    [SerializeField] private bool unarmedOverride;
    private Weapon weapon;

    private int roundsInMag, magazineSize;
    private bool isReloading;
    private int weaponDamage;

    private bool borrowedFromObjectPooler = false;

    private void Start()
    {
        controller = GetComponent<EnemyController>();
        pointer = transform.Find("Pointer").gameObject.transform;
        controller.onEnemyDeath += delegate { OnDeath(); };

        SetWeapon(defaultWeapon);
    }

    public void SetWeapon(Weapon weapon)
    {
        this.weapon = weapon;

        if (unarmedOverride)
        {
            activeWeapon = sharkHands.GetComponentInChildren<ActiveWeapon>(); //SharkUnarmed
        }
        else
        {
            sharkHands.SetActive(false);

            var go = ObjectPooler.Spawn(weapon.ItemName, pointer.position, Quaternion.identity);
            go.transform.SetParent(pointer);
            borrowedFromObjectPooler = true;
            activeWeapon = go.GetComponentInChildren<ActiveWeapon>();
        }


        weaponDamage = weapon.BaseDamage + damageModifier;
        attackRate = weapon.BaseAttackRate - ATTACK_RATE_PENALTY;

        magazineSize = weapon.BaseMagazineCapacity;
        roundsInMag = magazineSize;

        if (weapon.Type == WeaponType.Sword) attackRadius = 3;
        else attackRadius = 15;
    }

    private void Update()
    {
        if (!controller.IsAlive || !controller.CanAct) return;

        pointer.eulerAngles = new Vector3(0, 0, controller.angleToPlayer);

        if (CanAttack()) OnAttack();
    }

    private bool CanAttack()
    {
        if (attackCooldownTimer > Time.time) return false;
        if (isReloading) OnReloadComplete();

        if (controller.distanceToPlayer > attackRadius) return false;
        if (!controller.CanSeePlayer) return false;

        return true;
    }

    private void OnAttack()
    {
        if (weapon.Type == WeaponType.Sword) OnSwingWeapon();
        else OnFireWeapon();

        activeWeapon.PlayEffects(); //Muzzle Flash
        AudioManager.PlayEnemyClip(weapon.AttackSFX);
        attackCooldownTimer = Time.time + (1 / attackRate);
    }

    private void OnFireWeapon()
    {
        var bullet = ObjectPooler.Spawn(weapon.ProjectilePoolTag + "_enemy", activeWeapon.Muzzle.position, pointer.rotation);
        bullet.GetComponent<IProjectile>()?.SetDamage(weaponDamage, controller.CurrentDimension);
        
        roundsInMag--;
        if (roundsInMag <= 0)
        {
            attackCooldownTimer += weapon.ReloadTime;
            AudioManager.PlayEnemyClip("gun_reload");
            isReloading = true;
        }
    }

    private void OnSwingWeapon()
    {
        var pos = activeWeapon.Muzzle.position + (Vector3.right * controller.anim.GetFloat("horizontal"));
        Collider2D[] colls = Physics2D.OverlapBoxAll(pos, new Vector2(3, 5), 0);
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].gameObject == gameObject) continue;
            colls[i].GetComponent<IDamageable>()?.OnDamagePlayer(weaponDamage, controller.CurrentDimension);
        }
    }

    private void OnReloadComplete()
    {
        //Attack cooldown was just increased by reload time, which has now elapsed
        roundsInMag = magazineSize;
        isReloading = false;
        AudioManager.PlayEnemyClip("gun_racking");
    }

    private void OnDeath()
    {
        if (borrowedFromObjectPooler)
            ReturnWeaponToPool();
    }

    private void ReturnWeaponToPool()
    {
        ObjectPooler.Return(weapon.ItemName, activeWeapon.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.red;
    }
}
