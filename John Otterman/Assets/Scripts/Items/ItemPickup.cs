using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour, ICollectable, IPooledObject
{
    [SerializeField] protected string poolTag;
    protected float defaultMovementSpeed = 15;
    protected float movementSpeed = 15;
    protected bool isMoving = false;
    protected bool wasSpawned = false; //Determine whether to return to pool or destroy
    protected Coroutine movementCoroutine;

    public virtual void OnCollect()
    {
        if (!isMoving)
        {
            movementCoroutine = StartCoroutine(MoveToPlayer());
        }
    }

    protected IEnumerator MoveToPlayer()
    {
        isMoving = true;
        movementSpeed = defaultMovementSpeed;
        var target = PlayerController.instance.transform;

        while (Vector2.Distance(transform.position, target.position) > 0.1f)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * movementSpeed * Time.deltaTime;

            movementSpeed += Time.deltaTime * 5;
            yield return null;
        }

        OnPlayerReached();
        OnReturnToPool();
    }

    protected virtual void OnPlayerReached()
    {
        //Meant to be overwritten
    }

    public virtual void OnObjectSpawn()
    {
        wasSpawned = true;
    }

    public virtual void OnReturnToPool()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        isMoving = false;
        if (wasSpawned) ObjectPooler.Return(poolTag, gameObject);
        else Destroy(gameObject);
    }

    public void OnSceneChange()
    {
        OnReturnToPool();
    }
}
