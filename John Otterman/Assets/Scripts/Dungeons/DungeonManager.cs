using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public delegate void OnEnemyCountChangeCallback();
    public OnEnemyCountChangeCallback onEnemyCountChange;

    public bool DoNotSpawn;
    public bool TestGenerate;
    public int dungeonSizeOverride;

    public static DungeonManager instance;


    public int globalStageIndex { get; private set; } //scale from 0-8, the world level
    private int stageIndex; //scale from 0-2, the stage within a dimension

    [SerializeField] private World dimension;
    [SerializeField] private DungeonGenerator generator;
    private int[] dungeonSizePerLevel = { 5, 8, 12 };
    [Space]
    [SerializeField] private GameObject[] lightEnemyPrefabs;
    [SerializeField] private GameObject[] heavyEnemyPrefab;
    private float[] chanceToSpawnHeavy = { 0.25f, 0.3f, 0.35f };
    [Space]
    [SerializeField] private GameObject hubPortal;

    [SerializeField] private int minEnemiesToSpawn, maxEnemiesToSpawn;

    private Vector3[] roomCenters;
    private List<EnemyController> enemies;

    public int enemiesRemaining { get; private set; }
    public int totalEnemyCount { get; private set; }

    [SerializeField] private string[] m_lootPoolTags;
    private float portalSpawnDelayTime = 3;

    #region - INIT -
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (TestGenerate)
        {
            generator.GenerateDungeon(dungeonSizeOverride);
        }
    }

    public void SetStageNum(int num)
    {
        stageIndex = num;

        globalStageIndex = (int)dimension * 3 + stageIndex;

        CreateDungeon();
    }

    private void CreateDungeon()
    {
        generator.GenerateDungeon(dungeonSizePerLevel[stageIndex]);

        StartCoroutine(WaitForDungeonToBeCreated());
    }

    private IEnumerator WaitForDungeonToBeCreated()
    {
        while (generator.dungeonCreated == false)
        {
            yield return null;
        }

        roomCenters = new Vector3[generator.existingRooms.Count];
        for (int i = 0; i < roomCenters.Length; i++)
        {
            roomCenters[i] = generator.existingRooms[i].transform.position;
        }

        GenerateEnemies();
    }

    private void GenerateEnemies()
    {
        if (DoNotSpawn) return;
        //var rb = PlayerController.instance.gameObject.GetComponent<Rigidbody2D>();
        //rb.mass = 0.25f;
        //rb.drag = 0;
        //PlayerController.instance.ToggleMovement(false);

        enemies = new List<EnemyController>();

        for (int i = 0; i < roomCenters.Length; i++)
        {
            if (i == 0) continue; //Don't spawn enemies in the first room the player is in

            int numToSpawn = Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn) + stageIndex;

            for (int n = 0; n < numToSpawn; n++)
            {
                Vector3 pos = roomCenters[i];
                pos.x += Random.Range(-0.5f, 0.5f) * n;
                pos.y += Random.Range(-1f, 1f) * n;

                int index = Random.Range(0, heavyEnemyPrefab.Length);
                SpawnEnemy(true, index, pos);
            }
        }

        totalEnemyCount = enemiesRemaining;
    }
    
    private void SpawnEnemy(bool heavy, int index, Vector3 position)
    {
        GameObject enemy;
        if (heavy) enemy = Instantiate(heavyEnemyPrefab[index], position, Quaternion.identity);
        else enemy = Instantiate(lightEnemyPrefabs[index], position, Quaternion.identity);
        var newEnemy = enemy.GetComponent<EnemyController>();

        if (newEnemy != null)
        {
            enemies.Add(newEnemy);
            enemiesRemaining++;
            newEnemy.onEnemyDeath += UpdateStageEnemies;
        }
    }
    #endregion

    private void UpdateStageEnemies(EnemyController enemy)
    {
        enemiesRemaining--;
        enemies.Remove(enemy);

        enemy.onEnemyDeath -= UpdateStageEnemies;

        if (enemiesRemaining <= 0) OnStageComplete();
        if (enemiesRemaining <= 10)
        {
            //All charge player, spawn in nearby
        }
        
        onEnemyCountChange?.Invoke();
    }

    private void OnStageComplete()
    {
        PlayerController.instance.SetInvincible(true);

        if (globalStageIndex != 8)
            GameManager.instance.OnStageClear(globalStageIndex);       

        Vector3 desiredSpawnPosition = roomCenters[0];
        var dist = Vector2.Distance(roomCenters[0], PlayerController.instance.transform.position);
        for (int i = 1; i < roomCenters.Length; i++)
        {
            var newDist = Vector2.Distance(roomCenters[i], PlayerController.instance.transform.position);
            if (newDist < dist)
            {
                desiredSpawnPosition = roomCenters[i];
                dist = newDist;
            }
        }
        StartCoroutine(PortalSpawnDelay(desiredSpawnPosition));
    }

    
    private IEnumerator PortalSpawnDelay(Vector3 portalPos)
    {
        yield return new WaitForSeconds(portalSpawnDelayTime);
        Instantiate(hubPortal, portalPos, Quaternion.identity);
    }

    public string GetLootTag()
    {
        int index = Random.Range(0, m_lootPoolTags.Length);
        return m_lootPoolTags[index];
    }
}

public enum World { World_00, World_01, World_02 }
public enum Dimension { Dimension_01, Dimension_02, Dimension_03 }
