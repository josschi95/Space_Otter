using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private Color ammoSelectedColor = new Color(99f, 171f, 146f, 1f);

    private PlayerController player;
    private PlayerWeapon playerWeapon;

    [SerializeField] private GameObject panelParent; //The GameObject that holds all other elements
    [SerializeField] private Sprite[] pipStyles;
    [SerializeField] private Image[] emptyHealthPips;
    [SerializeField] private Image[] healthPips;
    [SerializeField] private Image[] emptyArmorPips;
    [SerializeField] private Image[] armorPips;
    [SerializeField] private Image[] weaponSlots;
    [SerializeField] private Sprite[] lockedWeaponsSprites;
    [SerializeField] private Sprite[] unlockedWeaponSprites;
    [SerializeField] private Text magCount, ammoCount;

    [Header("PauseMenu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button continueButton, quitToHubButton, quitButton, unlockAllButton, clearDataButton;
    private bool gameIsPaused = false;

    [Header("Dungeon Stats")]
    [SerializeField] private GameObject dungeonStatParent;
    [SerializeField] private Text playerScore, playerClams, enemyCount;

    [Space]
    [SerializeField] private GameObject realoadDisplay;
    [SerializeField] private Slider reloadTimerBar;

    [SerializeField] private Text noAmmoWarning;
    private Color warningColor = Color.red;
    [SerializeField] private Text[] allAmmoCountText;

    [Header("Stage Clear Menu")]
    [SerializeField] private GameObject stageClearMenu;
    [SerializeField] private Text highScore, score, enemiesCleared;
    [SerializeField] private Button returnToHubButton;

    [Header("Game Over Menu")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Button returnToHubButton2;

    private Coroutine reloadCoroutine;

    private void Start()
    {
        GameManager.instance.onScoreChange += UpdateScoreDisplay;
        GameManager.instance.onClamsChange += UpdateClamsDisplay;

        realoadDisplay.SetActive(false);
        noAmmoWarning.gameObject.SetActive(false);

        for (int i = 0; i < allAmmoCountText.Length; i++)
        {
            allAmmoCountText[i].color = ammoSelectedColor;
        }

        player = PlayerController.instance;

        player.onHealthUpgrade += UpdateHealthAndArmorBars;
        player.onArmorUpgrade += UpdateHealthAndArmorBars;

        player.onHealthChange += OnHealthChange;
        player.onArmorChange += OnArmorChange;

        player.combat.onNewWeaponUnlocked += UpdateWeaponSlots;
        player.combat.onNewWeaponEquipped += UpdateWeaponDisplay;
        player.combat.onWeaponReload += DisplayWeaponReload;

        continueButton.onClick.AddListener(TogglePause);
        quitToHubButton.onClick.AddListener(QuitToHub);
        quitButton.onClick.AddListener(delegate { Application.Quit(); });
        unlockAllButton.onClick.AddListener(delegate { GameManager.instance.UnlockAll(); });
        clearDataButton.onClick.AddListener(delegate { GameManager.instance.saveSystem.ResetSaveFile(); });

        returnToHubButton.onClick.AddListener(OnReturnToHub);
        returnToHubButton2.onClick.AddListener(OnReturnToHub);
        Invoke("HUDInit", 0.01f);
    }

    public static void PauseGame()
    {
        instance.TogglePause();
    }

    private void QuitToHub()
    {
        Time.timeScale = 1;
        GameManager.OnGameOver();
        OnReturnToHub();
    }

    private void TogglePause()
    {
        if (gameIsPaused)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        else
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        gameIsPaused = !gameIsPaused;
    }

    public static void ToggleHUD(bool showDisplay)
    {
        instance.panelParent.SetActive(showDisplay);
    }

    private void HUDInit()
    {
        OnHealthChange();
        OnArmorChange();
        UpdateWeaponDisplay();
    }

    private void Update()
    {
        DisplayAmmoCount();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Loading main menu or the hub or credits
        if (scene.buildIndex <= 1 || scene.buildIndex >= 6)
        {
            dungeonStatParent.SetActive(false);
            if (DungeonManager.instance != null)
                DungeonManager.instance.onEnemyCountChange -= DisplayEnemyCount;
        }
        else if (scene.buildIndex == 5)
        {
            dungeonStatParent.SetActive(true);
            if (DungeonManager.instance != null)
                DungeonManager.instance.onEnemyCountChange -= DisplayEnemyCount;
        }
        else //dungeon
        {
            dungeonStatParent.SetActive(true);
            DungeonManager.instance.onEnemyCountChange += DisplayEnemyCount;
            DisplayEnemyCount();
        }
    }

    private void DisplayAmmoCount()
    {
        magCount.text = player.combat.currentWeapon.roundsInMagazine.ToString();
        ammoCount.text = player.combat.WeaponAmmoCount[(int)player.combat.currentWeapon.weapon.Type].ToString();

        for (int i = 0; i < allAmmoCountText.Length; i++)
        {
            if (i == 0) continue;
            allAmmoCountText[i].text = "x" + player.combat.WeaponAmmoCount[i];
        }

        DisplayNoAmmoWarning(PlayerWeaponIsEmpty());
    }

    //switch this over to an event based method
    private void DisplayEnemyCount()
    {
        if (DungeonManager.instance == null) return;

        enemyCount.text = DungeonManager.instance.enemiesRemaining.ToString("000");
    }

    private bool PlayerWeaponIsEmpty()
    {
        if (playerWeapon == null) return false;
        if (playerWeapon.weapon.Type == WeaponType.Sword) return false;
        if (player.combat.WeaponAmmoCount[(int)playerWeapon.weapon.Type] > 0) return false;
        if (playerWeapon.roundsInMagazine > 0) return false;
        return true;
    }

    private void OnHealthChange()
    {
        for (int i = 0; i < healthPips.Length; i++)
        {
            //Change out the sprite between red/yellow/green based on percentage of max health
            if (player.currentHealth <= 5) healthPips[i].sprite = pipStyles[0];
            else healthPips[i].sprite = pipStyles[1];

            //Enable/disable pips to show health
            if (i + 1 > player.currentHealth) healthPips[i].enabled = false;
            else healthPips[i].enabled = true;
        }
    }

    private void OnArmorChange()
    {
        for (int i = 0; i < armorPips.Length; i++)
        {
            //Enable/disable pips to show armor
            if (i + 1 > player.currentArmor) armorPips[i].enabled = false;
            else armorPips[i].enabled = true;
        }
    }

    private void UpdateHealthAndArmorBars()
    {
        for (int i = 0; i < healthPips.Length; i++)
        {
            emptyHealthPips[i].gameObject.SetActive(i < player.MaxHealth);
            healthPips[i].gameObject.SetActive(i < player.MaxHealth);
        }

        for (int i = 0; i < armorPips.Length; i++)
        {
            emptyArmorPips[i].gameObject.SetActive(i < player.MaxArmor);
            armorPips[i].gameObject.SetActive(i < player.MaxArmor);
        }
    }

    private void UpdateWeaponDisplay()
    {
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        realoadDisplay.SetActive(false);

        playerWeapon = player.combat.currentWeapon;
        if (playerWeapon == null) Debug.Log("Weapon null");
        var type = playerWeapon.weapon.Type;
        if (playerWeapon.weapon.Type == WeaponType.Sword)
        {
            magCount.gameObject.SetActive(false);
            ammoCount.gameObject.SetActive(false);
        }
        else if (playerWeapon.weapon.ItemName == "Portal Gun")
        {
            magCount.gameObject.SetActive(false);
            ammoCount.gameObject.SetActive(false);
        }
        else
        {
            magCount.gameObject.SetActive(true);
            ammoCount.gameObject.SetActive(type != WeaponType.MiniGun);
        }
    }

    private void UpdateWeaponSlots()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (player.combat.PlayerWeapons[i].isUnlocked)
            {
                weaponSlots[i].sprite = unlockedWeaponSprites[i];
            }
            else
            {
                weaponSlots[i].sprite = lockedWeaponsSprites[i];
            }
        }
    }

    private void UpdateScoreDisplay()
    {
        playerScore.text = GameManager.instance.playerScore.ToString("0000");
    }
    
    private void UpdateClamsDisplay()
    {
        playerClams.text = GameManager.GetNetClams().ToString("0000");
    }

    #region - Reload Display -
    private void DisplayWeaponReload()
    {
        reloadTimerBar.value = 1;
        realoadDisplay.SetActive(true);

        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(ReloadTimer());
    }

    private IEnumerator ReloadTimer()
    {
        float timeToMove = player.combat.currentWeapon.weapon.ReloadTime;
        float timeElapsed = 0;

        while(timeElapsed < timeToMove)
        {
            var percentage = 1 - (timeElapsed / timeToMove);
            reloadTimerBar.value = (percentage);
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        realoadDisplay.SetActive(false);
    }
    #endregion

    private void DisplayNoAmmoWarning(bool noAmmo)
    {
        if (noAmmo)
        {
            noAmmoWarning.gameObject.SetActive(true);
            warningColor.a = Mathf.PingPong(Time.time, 1);
            noAmmoWarning.color = warningColor;
        }
        else noAmmoWarning.gameObject.SetActive(false);
    }

    public void StageComplete()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        stageClearMenu.SetActive(true);

        bool newHighScore = GameManager.instance.OnSubmitNewScore();
        if (newHighScore)
        {
            highScore.color = Color.green;
            score.color = Color.green;
        }
        else
        {
            highScore.color = Color.white;
            score.color = Color.white;
        }

        highScore.text = GameManager.instance.StageHighScores[DungeonManager.instance.globalStageIndex].ToString("00000");

        score.text = GameManager.instance.playerScore.ToString("00000");

        enemiesCleared.text = DungeonManager.instance.totalEnemyCount.ToString("00000");
    }

    public void OnGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameOverScreen.SetActive(true);
    }

    private void OnReturnToHub()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        PlayerController.instance.ToggleMovement(true);
        pauseMenu.SetActive(false);
        stageClearMenu.SetActive(false);
        gameOverScreen.SetActive(false);
        SceneManager.LoadScene(1);
    }
}
