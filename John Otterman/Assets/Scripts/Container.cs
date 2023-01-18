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
    public void OnDamage(int dmg)
    {
        if (!isActive) return;

        //Drops an item
        string tag = DungeonManager.instance.GetLootTag();
        ObjectPooler.Spawn(tag, transform.position, Quaternion.identity);

        OnReturnToPool();
    }

    public void OnDamagePlayer(int dmg)
    {
        OnDamage(dmg);
    }

    public void OnDamageEnemy(int dmg)
    {
        OnDamage(dmg);
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
