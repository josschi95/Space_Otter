using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public delegate void OnPlayerClamsChangeCallback();
    public OnPlayerClamsChangeCallback onClamsChange;

    public delegate void OnPlayerScoreChangeCallback();
    public OnPlayerScoreChangeCallback onScoreChange;

    public PlayerController player { get; private set; }

    //public JSONSaving saveSystem { get; private set; }

    public int globalStageIndex { get; private set; }

    public Material spriteFlashMat;
    private Coroutine scoreMultiplierDecayCoroutine;

    #region - Player and Stage Stats -
    public float playerScore { get; private set; }
    public float scoreMultiplier;
    public int playerClams { get; private set; }
    public int clamsGainedOnCurrentRun { get; private set; }

    [SerializeField] private bool[] m_clearedStages = { false, false, false, false, false, false, false, false, false };
    public bool[] ClearedStages => m_clearedStages;

    private int[] m_stageHighScores = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int[] StageHighScores => m_stageHighScores;
    #endregion

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
        //saveSystem = GetComponent<JSONSaving>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var dungeonManager = DungeonManager.instance;
        if (dungeonManager != null) //the player has entered a dungeon
        {
            scoreMultiplierDecayCoroutine = StartCoroutine(ScoreMultiplierDecay());
        }
        else if (scoreMultiplierDecayCoroutine != null) StopCoroutine(scoreMultiplierDecayCoroutine);

        PlayerSceneChanges(scene.buildIndex);

        playerScore = 0;
        onScoreChange?.Invoke();
        MergeClams();

        if (scene.buildIndex > 0) ObjectPooler.OnSceneChange();
        AudioManager.SetTheme(scene.buildIndex);
        UIManager.ToggleHUD(scene.buildIndex >= 1 && scene.buildIndex <= 6);

        //Save when the player enters the hub
        //if (scene.buildIndex == 1) saveSystem.SaveData();
    }

    private void PlayerSceneChanges(int buildIndex)
    {
        player = PlayerController.instance;

        bool canAttack = true;
        if (buildIndex == 0 || buildIndex == 2 || buildIndex == 9) canAttack = false;
        player.combat.ToggleCombat(canAttack);
        player.transform.position = Vector3.zero;
        player.OnRestoreAll();
        //if (buildIndex == 6) player.transform.position = Vector2.one * 100;
        
    }

    public int GetSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public static void LoadScene(int index)
    {
        instance.LoadSceneByIndex(index);
    }

    private void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    #region - Player Score -
    private void IncreaseScoreMultiplier()
    {
        scoreMultiplier += 0.1f;
    }

    private IEnumerator ScoreMultiplierDecay()
    {
        while (true)
        {
            scoreMultiplier -= 0.01f;
            if (scoreMultiplier <= 0) scoreMultiplier = 0;
            yield return new WaitForSeconds(1);
        }
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
    #endregion

    #region - Player Clams -
    public static void OnClamsGained(int clams)
    {
        if (DungeonManager.instance == null)
        {
            instance.playerClams += clams;
            return;
        }
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

    private void MergeClams()
    {
        playerClams += clamsGainedOnCurrentRun;
        clamsGainedOnCurrentRun = 0;
        onClamsChange?.Invoke();
    }
    #endregion

    #region - Stage Settings -
    public void SetStageDifficulty(int difficulty)
    {
        globalStageIndex = difficulty;
        //Debug.Log("Inter-Dimension Index set to " + difficulty);
    }

    public void OnStageClear(int stage)
    {
        if (stage >= m_clearedStages.Length) return;

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
    #endregion

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

    public void LockAll()
    {
        for (int i = 0; i < m_clearedStages.Length; i++)
        {
            m_clearedStages[i] = false;
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

    public void OnResetValues()
    {
        playerClams = 0;

        for (int i = 0; i < m_clearedStages.Length; i++)
        {
            m_clearedStages[i] = false;
        }

        for (int i = 0; i < m_stageHighScores.Length; i++)
        {
            m_stageHighScores[i] = 0;
        }
    }
}
