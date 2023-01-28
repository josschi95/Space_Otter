using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkBossBattle : MonoBehaviour
{
    private PlayerController player;

    [SerializeField] private EnemyController kingShark;
    [SerializeField] private Transform[] minionSpawnPositions;
    [Space]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Weapon rifle;
    [SerializeField] private GameObject hubPortal;
    private int battlePhaseIndex = 0; //The current boss battle phase, triggers events and determines behavior changes

    private void Start()
    {
        player = PlayerController.instance;

        kingShark.transform.position = new Vector3(0, 9, 0);
        kingShark.onDamageTaken += CheckForNextPhase;
        kingShark.onEnemyDeath += delegate { OnPlayerVictory(); };

        kingShark.OnPlayerSwitchDimension(player.CurrentDimension);

        AdjustWeaponSize();
    }

    private void AdjustWeaponSize()
    {
        var child = kingShark.transform.GetChild(1).gameObject.transform.GetChild(1);
        Debug.Log(child.name);
        child.transform.localScale = Vector3.one;
        child.transform.localEulerAngles = Vector3.zero;
    }

    private void CheckForNextPhase()
    {
        float current = kingShark.currentHealth;
        float percent = current / kingShark.MaxHealth;
        Debug.Log("Max: " + kingShark.MaxHealth + ", Current: " + kingShark.currentHealth + ", Percent: " + percent);
        switch (battlePhaseIndex)
        {
            case 0:
                if (percent <= 0.75) OnEnterFirstPhase();
                break;
            case 1:
                if (percent <= 0.5) OnEnterSecondPhase();
                break;
            case 2:
                if (percent <= 0.35) OnEnterThirdPhase();
                break;
        }
    }

    //Mobius health decreased to 65% of its max
    private void OnEnterFirstPhase()
    {
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);
        battlePhaseIndex = 1;
        OnSpawnMinions();
    }

    //Mobius health decreased to 50% of its max
    private void OnEnterSecondPhase()
    {
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);
        battlePhaseIndex = 2;
        OnSpawnMinions();
    }

    //Mobius health decreased to 35% of its max
    private void OnEnterThirdPhase()
    {
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);
        kingShark.GetComponent<EnemyCombat>().SetWeapon(rifle);
        AdjustWeaponSize();

        battlePhaseIndex = 3;
        OnSpawnMinions();
    }

    private void OnSpawnMinions()
    {
        //Debug.Log("Spawning Minions");
        for (int i = 1; i < minionSpawnPositions.Length; i++)
        {
            int index = Random.Range(0, enemyPrefabs.Length);
            var go = Instantiate(enemyPrefabs[index], minionSpawnPositions[i].position, Quaternion.identity);
            var newEnemy = go.GetComponent<EnemyController>();
            newEnemy.SetDimension((Dimension)Random.Range(0, 2));
        }
    }

    private void OnPlayerVictory()
    {
        player.SetInvincible(true);
        GameManager.instance.OnStageClear(5); //Player successfully cleared the first boss
        Invoke("SpawnPortal", 2.5f);

        var remainingEnemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < remainingEnemies.Length; i++)
        {
            remainingEnemies[i].OnDamageEnemy(500);
        }
    }

    private void SpawnPortal()
    {
        Instantiate(hubPortal, Vector3.zero, Quaternion.identity);
    }
}
