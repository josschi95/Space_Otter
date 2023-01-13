using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IDamageable, IPooledObject
{
    [SerializeField] private string poolTag = "explosiveBarrel";
    private ParticleSystem explosion;
    [SerializeField] private float explosionRadius;
    [SerializeField] private int damage;
    private bool isActive = false;
    private Dimension dimension;

    private void Start()
    {
        explosion = GetComponentInChildren<ParticleSystem>();
    }

    public void OnDamage(int dmg, Dimension dimension)
    {
        if (!isActive || dimension != this.dimension) return;

        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].gameObject == gameObject) continue;
            colls[i].GetComponent<IDamageable>()?.OnDamage(damage, dimension);
        }

        //I would like an explosion

        //Good ol' camera shake
        CameraController.ShakeCamera(0.1f, 0.2f);
        explosion.Play();
    }

    public void OnDamagePlayer(int dmg, Dimension dimension)
    {
        OnDamage(dmg, dimension);
    }

    public void OnDamageEnemy(int dmg, Dimension dimension)
    {
        OnDamage(dmg, dimension);
    }

    private void OnParticleSystemStopped()
    {
        OnReturnToPool();
    }

    public void SetDimension(Dimension dimension)
    {
        this.dimension = dimension;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.red;
    }
}
