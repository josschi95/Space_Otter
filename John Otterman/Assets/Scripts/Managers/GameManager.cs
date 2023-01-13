using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            //Debug.LogWarning("NOTE: More than one instance of GameManager found. Carry on.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        saveSystem = GetComponent<JSONSaving>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        
    }

    public delegate void OnPlayerClamsChangeCallback();
    public OnPlayerClamsChangeCallback onClamsChange;

    public delegate void OnPlayerScoreChangeCallback();
    public OnPlayerScoreChangeCallback onScoreChange;

    public PlayerController player { get; private set; }
    public float playerScore { get; private set; }
    public float scoreMultiplier;
    public int playerClams { get; private set; }
    public int clamsGainedOnCurrentRun { get; private set; }

    public JSONSaving saveSystem { get; private set; }

    public int sceneStageDifficulty { get; private set; }

    public Material spriteFlashMat;

    [SerializeField] private bool[] m_clearedStages = { false, false, false, false, false, false, false, false, false };
    public bool[] ClearedStages => m_clearedStages;

    private int[] m_stageHighScores = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int[] StageHighScores => m_stageHighScores;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = PlayerController.instance;
        player.combat.ToggleCombat(scene.buildIndex >= 2 && scene.buildIndex <= 5);
        player.transform.position = Vector3.zero;
        if (scene.buildIndex == 6) player.transform.position = Vector2.one * 100;
        player.OnRestoreAll();
        playerScore = 0;
        onScoreChange?.Invoke();

        var dungeonManager = DungeonManager.instance;
        if (dungeonManager != null) //the player has entered a dungeon
        {
            dungeonManager.SetStageNum(sceneStageDifficulty);
        }

        playerClams += clamsGainedOnCurrentRun;
        clamsGainedOnCurrentRun = 0;
        onClamsChange?.Invoke();

        //if (scene.buildIndex > 0) ObjectPooler.OnSceneChange();
        AudioManager.SetTheme(scene.buildIndex);
        UIManager.ToggleHUD(scene.buildIndex >= 1 && scene.buildIndex <= 5);

        //Save when the player enters the hub
        if (scene.buildIndex == 1) saveSystem.SaveData();
    }

    private void IncreaseScoreMultiplier()
    {
        scoreMultiplier += 0.1f;
    }

    public static void OnEnemyHit()
    {
        instance.IncreaseScoreMultiplier();
    }

    public static void OnScoreChange(int points)
    {
        instance.playerScore += points * (1 + instance.scoreMultiplier);
        instance.onScoreChange?.Invoke();
    }

    #region - Player Clams -
    public static void OnClamsGained(int clams)
    {
        instance.clamsGainedOnCurrentRun += clams;
        instance.onClamsChange?.Invoke();
    }

    public static int GetNetClams()
    {
        return instance.playerClams + instance.clamsGainedOnCurrentRun;
    }

    public static void OnClamsLost(int clams)
    {
        instance.playerClams -= clams;

        if (instance.playerClams < 0)
            instance.playerClams = 0;

        instance.onClamsChange?.Invoke();
    }
    #endregion

    public void SetStageDifficulty(int difficulty)
    {
        sceneStageDifficulty = difficulty;
        //Debug.Log("Inter-Dimension Index set to " + difficulty);
    }

    public void OnStageClear(int stage)
    {
        m_clearedStages[stage] = true;
        //Debug.Log("Global Stage Index " + stage + " is cleared");
    }

    public bool[] GetClearedStageList()
    {
        return m_clearedStages;
    }

    public int[] GetHighScores()
    {
        return m_stageHighScores;
    }

    public bool OnSubmitNewScore()
    {
        int index = DungeonManager.instance.globalStageIndex;

        if (playerScore > m_stageHighScores[index])
        {
            m_stageHighScores[index] = Mathf.RoundToInt(playerScore);
            return true;
        }
        return false;
    }

    public static void OnGameOver()
    {
        instance.clamsGainedOnCurrentRun = 0;
        instance.onClamsChange?.Invoke();
    }

    public void UnlockAll()
    {
        for (int i = 0; i < m_clearedStages.Length; i++)
        {
            m_clearedStages[i] = true;
        }
    }

    public void SetSavedValues(int clams, bool[] clearedStages, int[] highScores)
    {
        playerClams = clams;

        for (int i = 0; i < m_clearedStages.Length; i++)
        {
            m_clearedStages[i] = clearedStages[i];
        }

        for (int i = 0; i < m_stageHighScores.Length; i++)
        {
            m_stageHighScores[i] = highScores[i];
        }
    }
}
