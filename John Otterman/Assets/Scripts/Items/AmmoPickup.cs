using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : ItemPickup
{
    [SerializeField] WeaponType ammoType;
    [SerializeField] private int count;

    protected override void OnPlayerReached()
    {
        PlayerController.instance.combat.OnGainAmmo(count, ammoType);
        base.OnPlayerReached();
    }
}
