using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to store all player data including high scores, clams, progress, and weapon upgrades/unlocks
/// </summary>
public class PlayerData
{
    public int playerClams;

    public int playerMaxHealth;
    public int playerMaxArmor;

    public bool[] stagesCleared;
    public int[] stageHighScores;

    public PlayerWeapon[] playerWeaponData;

    public PlayerData(PlayerCombat combat)
    {
        var manager = GameManager.instance;

        playerClams = manager.playerClams;

        playerMaxHealth = PlayerController.instance.MaxHealth;
        playerMaxArmor = PlayerController.instance.MaxArmor;

        stagesCleared = new bool[9];
        var clearList = manager.GetClearedStageList();
        for (int i = 0; i < stagesCleared.Length; i++)
        {
            stagesCleared[i] = clearList[i];
        }

        stageHighScores = new int[9];
        var scoreList = manager.GetHighScores();
        for (int i = 0; i < scoreList.Length; i++)
        {
            stageHighScores[i] = scoreList[i];
        }

        playerWeaponData = new PlayerWeapon[6];
        for (int i = 0; i < playerWeaponData.Length; i++)
        {
            if (combat == null) Debug.Log("combat is null");
            else if (combat.PlayerWeapons[i] == null) Debug.Log("weapons are null");

            playerWeaponData[i] = new PlayerWeapon(combat.PlayerWeapons[i].weapon);

            playerWeaponData[i].isUnlocked = combat.PlayerWeapons[i].isUnlocked;

            playerWeaponData[i].damage_Tier = combat.PlayerWeapons[i].damage_Tier;
            playerWeaponData[i].attackRate_Tier = combat.PlayerWeapons[i].attackRate_Tier;
            playerWeaponData[i].ammoCapacity_Tier = combat.PlayerWeapons[i].ammoCapacity_Tier;
            playerWeaponData[i].magazineCapacity_Tier = combat.PlayerWeapons[i].magazineCapacity_Tier;
        }
    }
}
