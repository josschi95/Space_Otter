using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemItemSpawner : MonoBehaviour, IPooledObject
{
    [SerializeField] private string poolTag;
    [SerializeField] private string itemToSpawnTag;
    [SerializeField] private ParticleSystem shower;
    private Particle[] particles;
    private Vector3[] particlePositions;
    private GameObject[] spawnedObjects;

    private bool objectsSpawned = false;


    private void Update()
    {
        //InitializeIfNeeded();

        if (shower == null) Debug.Log("shower null");
        int numParticles = shower.GetParticles(particles);

        if (numParticles > 0)
        {
            if (objectsSpawned == false) SpawnObjects(numParticles);
            else ObjectsFollowParticles();

            
            for (int i = 0; i < numParticles; i++)
            {
                particlePositions[i] = particles[i].position;
            }
        }      
    }

    private void SpawnObjects(int num)
    {
        spawnedObjects = new GameObject[num];
        particlePositions = new Vector3[num];

        for (int i = 0; i < num; i++)
        {
            var go = ObjectPooler.Spawn(itemToSpawnTag, particles[i].position, Quaternion.identity);
            spawnedObjects[i] = go;
        }

        objectsSpawned = true;
    }

    private void ObjectsFollowParticles()
    {
        for (int i = 0; i < spawnedObjects.Length; i++)
        {
            if (spawnedObjects[i] != null)
            {
                spawnedObjects[i].transform.position = particlePositions[i];
            }
        }
    }

    public void OnParticleSystemStopped()
    {
        particles = null;
        spawnedObjects = null;
        particlePositions = null;

        ObjectPooler.Return(poolTag, gameObject);
    }

    private void InitializeIfNeeded()
    {
        if (shower == null)
        {
            shower = GetComponent<ParticleSystem>();
        }

        if (particles == null || particles.Length < shower.main.maxParticles)
        {
            particles = new Particle[shower.main.maxParticles];
        }
    }

    public void OnObjectSpawn()
    {
        objectsSpawned = false;
        InitializeIfNeeded();
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
