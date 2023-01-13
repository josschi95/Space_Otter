using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapons")]
public class Weapon : Item
{
    [Header("Weapon Properties")]
    [SerializeField] private WeaponType m_weaponType;
    [SerializeField] private string m_projectilePoolTag;
    [SerializeField] private Sprite m_weaponSprite;
    [SerializeField] private AudioClip m_attackSFX;
    [SerializeField] private AudioClip m_reloadSFX;
    [Space]
    [SerializeField] private float m_reloadTime;
    [SerializeField] private float m_recoil;
    [Space]
    [SerializeField] private int m_baseDamage;
    [SerializeField] private float m_baseAttackRate;
    [Space]
    [SerializeField] private int m_baseMagazineCapacity;
    [SerializeField] private int m_baseAmmoCapacity;

    [Header("Upgrades")]
    [SerializeField] private int m_damageIncreasePerTier;
    [SerializeField] private float m_attackRateIncreasePerTier;
    [SerializeField] private int m_magazineCapIncreasePerTier;
    [SerializeField] private int m_ammoCapIncreasePerTier;
    [Space]
    [SerializeField] private float m_damageUpgradeCostMod;
    [SerializeField] private float m_attackRateUpgradeCostMod;
    [SerializeField] private float m_magazineCapacityUpgradeCostMost;
    [SerializeField] private float m_ammoCapacityUpgradeCostMod;


    public WeaponType Type => m_weaponType;
    public string ProjectilePoolTag => m_projectilePoolTag;
    public Sprite WeaponSprite => m_weaponSprite;
    public AudioClip AttackSFX => m_attackSFX;
    public AudioClip ReloadSFX => m_reloadSFX;

    public float ReloadTime => m_reloadTime;
    public float Recoil => m_recoil;

    //The Tier 1 stats of the weapon
    public int BaseDamage => m_baseDamage;
    public float BaseAttackRate => m_baseAttackRate;
    public int BaseMagazineCapacity => m_baseMagazineCapacity;
    public int BaseAmmoCapacity => m_baseAmmoCapacity;

    //The cost modifier for upgrading the weapon to the next tier
    public float DamageUpgradeCostMod => m_damageUpgradeCostMod;
    public float AttackRateUpgradeCostMod => m_attackRateUpgradeCostMod;
    public float MagazineCapacityUpgradeCostMod => m_magazineCapacityUpgradeCostMost;
    public float AmmoCapacityUpgradeCostMod => m_ammoCapacityUpgradeCostMod;

    //The amount each stat increases per Tier
    public int DamageIncreasePerTier => m_damageIncreasePerTier;
    public float AttackRateIncreasePerTier => m_attackRateIncreasePerTier;
    public int MagazineCapacityIncreasePerTier => m_magazineCapIncreasePerTier;
    public int AmmoCapIncreasePerTier => m_ammoCapIncreasePerTier;

    public int GetDamage(int tier)
    {
        return m_baseDamage + (m_damageIncreasePerTier * (tier - 1));
    }

    public float GetAttackRate(int tier)
    {
        return m_baseAttackRate * (1 + (m_attackRateIncreasePerTier * (tier - 1)));
    }

    public int GetAmmoCapacity(int tier)
    {
        return m_baseAmmoCapacity * (1 + (m_ammoCapIncreasePerTier * (tier - 1)));
    }

    public int GetMagazineCapacity(int tier)
    {
        return m_baseMagazineCapacity * (1 + (m_magazineCapIncreasePerTier * (tier - 1)));
    }

    #region - Upgrade Costs -
    public int GetDamageUpgradeCost(int currentTier)
    {
        return Mathf.RoundToInt((1 + currentTier) * 100 * m_damageUpgradeCostMod);
    }

    public int GetAttackRateUpgradeCost(int currentTier)
    {
        return Mathf.RoundToInt((1 + currentTier) * 100 * m_attackRateUpgradeCostMod);
    }

    public int GetAmmoCapUpgradeCost(int currentTier)
    {
        return Mathf.RoundToInt((1 + currentTier) * 100 * m_ammoCapacityUpgradeCostMod);
    }

    public int GetMagazineUpgradeCost(int currentTier)
    {
        return Mathf.RoundToInt((1 + currentTier) * 100 * m_magazineCapacityUpgradeCostMost);
    }
    #endregion
}

public enum WeaponType { Sword, Pistol, Shotgun, Rifle, SMG, MiniGun }
public enum WeaponStat { Damage, Rate, Magazine, Ammo }