public interface IPooledObject
{
    void OnObjectSpawn();

    void OnReturnToPool();

    void OnSceneChange();
}