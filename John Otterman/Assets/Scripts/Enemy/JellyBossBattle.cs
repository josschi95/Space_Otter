using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBossBattle : MonoBehaviour
{
    private PlayerController player;

    [SerializeField] private EnemyController kingJelly;
    [SerializeField] private Transform bulletSpawnParent;
    [SerializeField] private Transform[] bulletSpawnPositions;
    [SerializeField] private Transform[] minionSpawnPositions;
    [Space]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject hubPortal;
    private int battlePhaseIndex = 0; //The current boss battle phase, triggers events and determines behavior changes

    private Coroutine areaAttackCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.instance;

        kingJelly.onDamageTaken += CheckForNextPhase;
        kingJelly.onEnemyDeath += delegate { OnPlayerVictory(); };

        kingJelly.OnPlayerSwitchDimension(player.CurrentDimension);
        //Need to check for current player dimension
    }


    private void CheckForNextPhase()
    {
        float percent = kingJelly.currentHealth / kingJelly.MaxHealth;
        switch (battlePhaseIndex)
        {
            case 0:
                if (percent <= 0.65) OnEnterFirstPhase();
                break;
            case 1:
                if (percent <= 0.5) OnEnterSecondPhase();
                break;
            case 2:
                if (percent <= 0.35) OnEnterThirdPhase();
                break;
            case 3:
                if (percent <= 0.2) OnEnterFourthPhase();
                break;
        }
    }

    //Mobius health decreased to 65% of its max
    private void OnEnterFirstPhase()
    {
        Debug.Log("Entering battle phase: " + battlePhaseIndex);
        
        battlePhaseIndex = 1;

        if (areaAttackCoroutine != null) StopCoroutine(areaAttackCoroutine);
        areaAttackCoroutine = StartCoroutine(AreaAttackCoroutine());

        OnSpawnMinions();
    }

    //Mobius health decreased to 50% of its max
    private void OnEnterSecondPhase()
    {
        Debug.Log("Entering battle phase: " + battlePhaseIndex);
        
        battlePhaseIndex = 2;

        if (areaAttackCoroutine != null) StopCoroutine(areaAttackCoroutine);
        areaAttackCoroutine = StartCoroutine(AreaAttackCoroutine());
    }

    //Mobius health decreased to 35% of its max
    private void OnEnterThirdPhase()
    {
        Debug.Log("Entering battle phase: " + battlePhaseIndex);
        
        battlePhaseIndex = 3;

        
        if (areaAttackCoroutine != null) StopCoroutine(areaAttackCoroutine);
        areaAttackCoroutine = StartCoroutine(AreaAttackCoroutine());

        OnSpawnMinions();
    }

    //Mobius health decreased to 20% of its max
    private void OnEnterFourthPhase()
    {
        Debug.Log("Entering battle phase: " + battlePhaseIndex);

        battlePhaseIndex = 4;

        if (areaAttackCoroutine != null) StopCoroutine(areaAttackCoroutine);
        areaAttackCoroutine = StartCoroutine(AreaAttackCoroutine());
    }

    private void OnSpawnMinions()
    {
        Debug.Log("Spawning Minions");
        for (int i = 1; i < minionSpawnPositions.Length; i++)
        {
            int index = Random.Range(0, enemyPrefabs.Length);
            var go = Instantiate(enemyPrefabs[index], minionSpawnPositions[i].position, Quaternion.identity);
            var newEnemy = go.GetComponent<EnemyController>();
            newEnemy.SetDimension((Dimension)Random.Range(0, 3));
        }
    }

    private void OnPlayerVictory()
    {
        player.SetInvincible(true);
        if (areaAttackCoroutine != null) StopCoroutine(areaAttackCoroutine);

        GameManager.instance.OnStageClear(2); //Player successfully cleared the first boss
        Invoke("SpawnPortal", 2.5f);
    }

    private void SpawnPortal()
    {
        Instantiate(hubPortal, Vector3.zero, Quaternion.identity);
    }

    private IEnumerator AreaAttackCoroutine()
    {
        kingJelly.transform.position = transform.position;

        kingJelly.PauseCharacter();
        float t = 0; float t2 = 0;
        float timeToRun = 10 * battlePhaseIndex;
        float attackDelay = 0.25f;

        while (t < timeToRun)
        {
            t2 -= Time.deltaTime;
            if (t2 <= 0)
            {
                for (int i = 0; i < bulletSpawnPositions.Length; i++)
                {
                    var bullet = ObjectPooler.Spawn("bullet_enemy", bulletSpawnPositions[i].position, bulletSpawnPositions[i].rotation);
                    var dimension = (Dimension)Random.Range(0, 3);
                    bullet.GetComponent<IProjectile>()?.SetDamage(1, dimension);
                }
                t2 = attackDelay;
            }

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (!kingJelly.CanAct) kingJelly.PauseCharacter();
    }
}
