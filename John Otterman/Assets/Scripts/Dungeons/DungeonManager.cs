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
    private int[] dungeonSizePerLevel = { 5, 8, 10, 10, 12, 15, 15, 17, 20 };
    [Space]
    [SerializeField] private GameObject[] lightEnemyPrefabs;
    [SerializeField] private GameObject[] heavyEnemyPrefab;
    [SerializeField] private GameObject[] buildingPrefabs;
    private float[] chanceToSpawnHeavy = { 0.25f, 0.3f, 0.35f };
    [Space]
    [SerializeField] private GameObject hubPortal;

    [SerializeField] private int minEnemiesToSpawn, maxEnemiesToSpawn;
    [SerializeField] private GameObject[] remainingEnemyMapMarkers;

    private List<DungeonRoom> rooms;
    private List<EnemyController> enemies;
    private List<Vector3> nodePositions;

    public int enemiesRemaining { get; private set; }
    public int totalEnemyCount { get; private set; }

    [SerializeField] private string[] m_lootPoolTags;
    private float portalSpawnDelayTime = 2.5f;

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

        CullLootTags();

        for (int i = 0; i < remainingEnemyMapMarkers.Length; i++)
        {
            remainingEnemyMapMarkers[i].SetActive(false);
        }
    }

    public void SetStageNum(int num)
    {
        stageIndex = num;

        globalStageIndex = (int)dimension * 3 + stageIndex;

        if (dimension == World.Tutorial) TutorialSetup();
        else  CreateDungeon();
    }

    private void TutorialSetup()
    {
        PlayerController.instance.onPlayerDimensionChange += OnDimensionSwitch;

        rooms = new List<DungeonRoom>();
        enemies = new List<EnemyController>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            var newRoom = child.GetComponent<DungeonRoom>();
            if (newRoom != null) rooms.Add(newRoom);
            else
            {
                var enemy = child.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                    enemiesRemaining++;
                    enemy.onEnemyDeath += UpdateStageEnemies;
                }
            }
        }

        totalEnemyCount = enemiesRemaining;
        if (totalEnemyCount <= 10)
        {
            ActiveEnemyMapMarkers();
        }
    }

    private void CreateDungeon()
    {
        generator.GenerateDungeon(dungeonSizePerLevel[globalStageIndex]);

        PlayerController.instance.onPlayerDimensionChange += OnDimensionSwitch;

        StartCoroutine(WaitForDungeonToBeCreated());
    }

    private void CompileNodes()
    {
        nodePositions = new List<Vector3>();

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int x = 0; x < rooms[i].Entrances.Length; x++)
            {
                if (!nodePositions.Contains(rooms[i].Entrances[x].edge.position))
                {
                    nodePositions.Add(rooms[i].Entrances[x].edge.position);
                }
            }
        }
    }

    private IEnumerator WaitForDungeonToBeCreated()
    {
        while (generator.dungeonCreated == false)
        {
            yield return null;
        }

        rooms = new List<DungeonRoom>();
        rooms.AddRange(generator.dungeonRooms);

        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            if (rooms[i].gameObject.GetComponentInChildren<SpriteRenderer>() == null)
            {
                rooms.RemoveAt(i);
            }
        }

        CompileNodes();
        GenerateObstacles();
        GenerateEnemies();
    }

    private void GenerateObstacles()
    {
        float percentCoverage = Random.Range(0.35f, 0.5f);
        int numberOfBuildingsToSpawn = Mathf.FloorToInt(nodePositions.Count * percentCoverage);

        for (int i = 0; i < numberOfBuildingsToSpawn; i++)
        {
            int buildingIndex = Random.Range(0, buildingPrefabs.Length);
            int nodeIndex = Random.Range(0, nodePositions.Count);
            var go = Instantiate(buildingPrefabs[buildingIndex], nodePositions[nodeIndex], Quaternion.identity);

            nodePositions.RemoveAt(nodeIndex);

            int dimensionIndex = Random.Range(0, 3);
            go.GetComponent<Building>()?.SetDimension((Dimension)dimensionIndex);
        }
    }

    private void GenerateEnemies()
    {
        if (DoNotSpawn) return;

        enemies = new List<EnemyController>();

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].transform.position == Vector3.zero) continue; //Don't spawn enemies in the first room the player is in

            int numToSpawn = Random.Range(minEnemiesToSpawn, maxEnemiesToSpawn) + stageIndex;
            bool spawnHeavy = Random.value <= chanceToSpawnHeavy[stageIndex];
            for (int n = 0; n < numToSpawn; n++)
            {
                Vector3 pos = rooms[i].transform.position;
                pos.x += Random.Range(-0.5f, 0.5f) * n;
                pos.y += Random.Range(-1f, 1f) * n;

                int index;
                if (spawnHeavy) index = Random.Range(0, heavyEnemyPrefab.Length);
                else index = Random.Range(0, lightEnemyPrefabs.Length);
                SpawnEnemy(spawnHeavy, index, pos);
            }
        }

        totalEnemyCount = enemiesRemaining;
        if (totalEnemyCount <= 10)
        {
            ActiveEnemyMapMarkers();
        }
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
            newEnemy.SetDimension((Dimension)Random.Range(0, 3)); //Set the enemy to a random dimension

            newEnemy.OnPlayerSwitchDimension(PlayerController.instance.CurrentDimension);
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
            ActiveEnemyMapMarkers();
        }
        
        onEnemyCountChange?.Invoke();
    }

    private void OnDimensionSwitch(Dimension newDimension)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].SetDimensionDisplay(newDimension);
        }
    }

    private void OnStageComplete()
    {
        PlayerController.instance.SetInvincible(true);

        if (globalStageIndex != 8)
            GameManager.instance.OnStageClear(globalStageIndex);       

        Vector3 desiredSpawnPosition = rooms[0].transform.position;
        var dist = Vector2.Distance(rooms[0].transform.position, PlayerController.instance.transform.position);
        for (int i = 1; i < rooms.Count; i++)
        {
            var newDist = Vector2.Distance(rooms[i].transform.position, PlayerController.instance.transform.position);
            if (newDist < dist)
            {
                desiredSpawnPosition = rooms[i].transform.position;
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

    private void CullLootTags()
    {
        var combat = PlayerController.instance.combat;
        var stringList = new List<string>();

        if (!combat.PlayerWeapons[(int)WeaponType.Shotgun].isUnlocked) stringList.Add("ammoShotgun");
        if (!combat.PlayerWeapons[(int)WeaponType.Rifle].isUnlocked) stringList.Add("ammoRifle");
        if (!combat.PlayerWeapons[(int)WeaponType.SMG].isUnlocked) stringList.Add("ammoSMG");
        if (!combat.PlayerWeapons[(int)WeaponType.MiniGun].isUnlocked) stringList.Add("ammoMinigun");

        for (int i = m_lootPoolTags.Length - 1; i >= 0; i--)
        {
            if (stringList.Contains(m_lootPoolTags[i]))
            {
                m_lootPoolTags[i] = "clamShower";
            }
        }
    }

    private void ActiveEnemyMapMarkers()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive) AddMarker(enemies[i]);
        }
    }

    private void AddMarker(EnemyController enemy)
    {
        for (int i = 0; i < remainingEnemyMapMarkers.Length; i++)
        {
            if (!remainingEnemyMapMarkers[i].activeSelf)
            {
                remainingEnemyMapMarkers[i].transform.SetParent(enemy.gameObject.transform);
                remainingEnemyMapMarkers[i].transform.localPosition = Vector3.zero;
                remainingEnemyMapMarkers[i].SetActive(true);
                break;
            }
        }
    }

    public void KillAll()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].OnDamage(int.MaxValue);
        }
    }

    private void SetZeroGravity()
    {
        var rb = PlayerController.instance.gameObject.GetComponent<Rigidbody2D>();
        rb.mass = 0.25f;
        rb.drag = 0;
        PlayerController.instance.ToggleMovement(false);
    }
}

public enum World { World_00, World_01, World_02, Tutorial }
public enum Dimension { Dimension_00, Dimension_01, Dimension_02 }
