using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class JSONSaving : MonoBehaviour
{
    private PlayerData playerData;

    private string persistentPath = "";

    private void Start()
    {
        SetPath();
    }

    public bool ExistingSavesExist()
    {
        string[] saves = Directory.GetFiles(Application.persistentDataPath);
        if (saves.Length == 0)
        {
            Debug.Log("No Existing Save Files");
            return false;
        }

        for (int i = 0; i < saves.Length; i++)
        {
            //This works so long as the file name contains SaveData
            if (!saves[i].Contains("SaveData")) continue;
            StreamReader reader = new StreamReader(saves[i]);
            string json = reader.ReadToEnd();

            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
        }

        return true;
    }

    private void SetPath()
    {
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
    }

    private void CreatePlayerData()
    {
        playerData = new PlayerData(PlayerController.instance.combat);
    }

    public void SaveData()
    {
        if (persistentPath == "") SetPath();

        string savePath = persistentPath;
        //Debug.Log("Saving Data at " + persistentPath);

        CreatePlayerData();
        string json = JsonUtility.ToJson(playerData, true);
        //Debug.Log(json);

        using StreamWriter writer = new StreamWriter(savePath, false);
        writer.Write(json);
    }

    public void LoadData()
    {
        StreamReader reader = new StreamReader(persistentPath);
        string json = reader.ReadToEnd();

        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        GameManager.instance.SetSavedValues(data.playerClams, data.stagesCleared, data.stageHighScores);

        PlayerController.instance.SetSavedValues(data.playerMaxHealth, data.playerMaxArmor);

        var combat = PlayerController.instance.combat;
        combat.SetSavedValues(data.playerWeaponData);
    }
}
