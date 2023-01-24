using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region - Pools -
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool expandable;
        public List<GameObject> freeList;
        public List<GameObject> usedList;
    }
    #endregion

    #region - Singleton -
    private static ObjectPooler instance;
    private void Awake()
    {
        if (instance != null)
        {
            //Would destroy, but the gamemanager script should be doing the destroying
            return;
        }
        instance = this;

        GeneratePools();
    }
    #endregion

    public List<Pool> pools;
    public Dictionary<string, Pool> poolDictionary;

    /*private void Start()
    {
        poolDictionary = new Dictionary<string, Pool>();

        foreach (Pool newPool in pools)
        {
            newPool.freeList = new List<GameObject>();
            newPool.usedList = new List<GameObject>();

            for (int i = 0; i < newPool.size; i++)
            {
                GenerateNewObject(newPool);
            }

            poolDictionary.Add(newPool.tag, newPool);
        }
    }*/

    private void GeneratePools()
    {
        poolDictionary = new Dictionary<string, Pool>();

        foreach (Pool newPool in pools)
        {
            newPool.freeList = new List<GameObject>();
            newPool.usedList = new List<GameObject>();

            for (int i = 0; i < newPool.size; i++)
            {
                GenerateNewObject(newPool);
            }

            poolDictionary.Add(newPool.tag, newPool);
        }
    }

    public static void OnSceneChange()
    {
        instance.OnSceneChanged();
    }

    private void OnSceneChanged()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                child.GetComponent<IPooledObject>()?.OnSceneChange();
            }
        }
    }

    public static GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        return instance.SpawnFromPool(tag, position, rotation);
    }

    private GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary == null) Debug.Log(tag + " is being called before Awake");
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
            return null;
        }

        Pool pool = poolDictionary[tag];
        GameObject go = GetObject(pool);
        go.SetActive(true);
        go.transform.position = position;
        go.transform.rotation = rotation;

        IPooledObject pooledObj = go.GetComponent<IPooledObject>();
        if (pooledObj != null) pooledObj.OnObjectSpawn();

        return go;
    }

    private GameObject GetObject(Pool pool)
    {
        if (pool.freeList.Count == 0 && !pool.expandable) return null; //Another option here would be to find the first item in the usedList and return it to the pool
        else if (pool.freeList.Count == 0) GenerateNewObject(pool);

        int totalFree = pool.freeList.Count;

        GameObject go = pool.freeList[totalFree - 1];
        pool.freeList.RemoveAt(totalFree - 1);
        pool.usedList.Add(go);
        return go;
    }

    private void GenerateNewObject(Pool pool)
    {
        GameObject go = Instantiate(pool.prefab);
        go.transform.SetParent(transform, false);
        //go.transform.parent = transform;
        go.SetActive(false);
        pool.freeList.Add(go);
    }

    #region - Return To Pool -
    public static void Return(string tag, GameObject go)
    {
        instance.ReturnToPool(tag, go);
    }

    private void ReturnToPool(string tag, GameObject go)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
            return;
        }

        Pool pool = poolDictionary[tag];
        ReturnObject(pool, go);
    }

    private void ReturnObject(Pool pool, GameObject go)
    {
        if (!pool.usedList.Contains(go))
        {
            Debug.Log(go.name + "; " + pool.tag);
        }
        Debug.Assert(pool.usedList.Contains(go));

        go.SetActive(false);
        go.transform.SetParent(transform, false);
        //go.transform.parent = transform;
        pool.usedList.Remove(go);
        pool.freeList.Add(go);
    }
    #endregion
}