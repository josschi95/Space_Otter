using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBurst : MonoBehaviour, IPooledObject, IProjectile
{
    [SerializeField] private Transform[] pelletPositions;
    [SerializeField] private string poolTag;
    [SerializeField] private string pelletPoolTag;

    public void OnObjectSpawn()
    {
        //Do Nothing
    }

    public void SetDamage(int dmg, Dimension dimension)
    {
        for (int i = 0; i < pelletPositions.Length; i++)
        {
            var bullet = ObjectPooler.Spawn(pelletPoolTag, transform.position, pelletPositions[i].rotation);
            bullet.GetComponent<Projectile>()?.SetDamage(dmg, dimension);
        }

        OnReturnToPool();
    }

    public void OnReturnToPool()
    {
        ObjectPooler.Return(poolTag, gameObject);
    }

    public void OnSceneChange()
    {
        OnReturnToPool();
    }
}
