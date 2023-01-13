using UnityEngine;

public class HealthArmorPickup : ItemPickup
{
    [Space]
    [SerializeField] private PowerUpType type;
    [SerializeField] private int magnitude;
    private bool isActive;

    public override void OnCollect()
    {
        if (wasSpawned &&!isActive) return;

        base.OnCollect();
    }

    public override void OnObjectSpawn()
    {
        base.OnObjectSpawn();
        isActive = true;
    }

    public override void OnReturnToPool()
    {
        isActive = false;
        base.OnReturnToPool();
    }

    protected override void OnPlayerReached()
    {
        switch (type)
        {
            case PowerUpType.Health:
                PlayerController.instance.RestoreHealth(magnitude);
                break;
            case PowerUpType.Armor:
                PlayerController.instance.RestoreArmor(magnitude);
                break;
        }
    }
}

public enum PowerUpType { Health, Armor}
