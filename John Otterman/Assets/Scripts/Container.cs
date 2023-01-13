using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IDamageable, IPooledObject
{
    [SerializeField] private string poolTag = "crate";
    private bool isActive = false;
    private Dimension dimension;

    public void SetDimension(Dimension dimension)
    {
        this.dimension = dimension;
    }
    public void OnDamage(int dmg, Dimension dimension)
    {
        if (!isActive || dimension != this.dimension) return;

        //Drops an item
        string tag = DungeonManager.instance.GetLootTag();
        ObjectPooler.Spawn(tag, transform.position, Quaternion.identity);

        OnReturnToPool();
    }

    public void OnDamagePlayer(int dmg, Dimension dimension)
    {
        OnDamage(dmg, dimension);
    }

    public void OnDamageEnemy(int dmg, Dimension dimension)
    {
        OnDamage(dmg, dimension);
    }

    public void OnObjectSpawn()
    {
        isActive = true;
    }

    public void OnReturnToPool()
    {
        isActive = false;
        ObjectPooler.Return(poolTag, gameObject);
    }

    public void OnSceneChange()
    {
        OnReturnToPool();
    }
}
