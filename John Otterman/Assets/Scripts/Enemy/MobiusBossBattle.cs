using UnityEngine;
using UnityEngine.UI;

public class MobiusBossBattle : MonoBehaviour
{
    private PlayerController player;

    [SerializeField] private EnemyController mobius;
    [SerializeField] private Transform[] teleportationPositions;
    [Space]

    private float mobiusTeleportDelay = 12;
    private float teleportCooldownTimer;
    private int battlePhaseIndex = 0; //The current boss battle phase, triggers events and determines behavior changes

    [SerializeField] private GameObject[] enemyPrefabs;
    [Space]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Text dialogueField;
    [SerializeField] private Button dialogueButton;
    int dialogueIndex = 0; //The current line of displayed dialogue
    private string[] dialogueLines = 
    {
        "It ends here Morbius!",
        "We shall see Otterman"
    };

    private void Start()
    {
        mobius.PauseCharacter();
        mobius.transform.position = new Vector3(0, 9, 0);
        
        player = PlayerController.instance;
        player.ToggleMovement(false);
        player.combat.ToggleCombat(false);
        
        mobius.onDamageTaken += CheckForNextPhase;
        mobius.onEnemyDeath += delegate { OnPlayerVictory(); };
        dialogueButton.onClick.AddListener(DisplayDialogue);

        DisplayDialogue();
        UIManager.ToggleHUD(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (dialogueBox.activeSelf && Input.GetMouseButtonDown(0))
        {
            DisplayDialogue();
        }

        teleportCooldownTimer -= Time.deltaTime;
        if (teleportCooldownTimer <= 0) OnTeleportMobius();
    }

    //Cycle through the on-screen dialogue at the start of the battle
    private void DisplayDialogue()
    {
        if (dialogueIndex > dialogueLines.Length - 1)
        {
            OnCombatStart();
            return;
        }
        dialogueField.text = dialogueLines[dialogueIndex];
        dialogueIndex++;
    }

    //Re-enables movement and combat for player and mobius
    private void OnCombatStart()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        dialogueBox.SetActive(false);
        player.ToggleMovement(true);
        player.combat.ToggleCombat(true);
        mobius.PauseCharacter();
        UIManager.ToggleHUD(true);

        teleportCooldownTimer = mobiusTeleportDelay;
    }

    #region - Boss Phases -
    //Called each time mobius takes damage to determine battle phase
    private void CheckForNextPhase()
    {
        float percent = mobius.currentHealth / mobius.MaxHealth;
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
        battlePhaseIndex = 1;
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);

        OnSpawnMinions();
    }

    //Mobius health decreased to 50% of its max
    private void OnEnterSecondPhase()
    {
        battlePhaseIndex = 2;
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);
        //Switch to minigun from AR
    }

    //Mobius health decreased to 35% of its max
    private void OnEnterThirdPhase()
    {
        battlePhaseIndex = 3;
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);

        OnSpawnMinions();
    }

    //Mobius health decreased to 20% of its max
    private void OnEnterFourthPhase()
    {
        battlePhaseIndex = 4;
        //Debug.Log("Entering battle phase: " + battlePhaseIndex);
        
        mobiusTeleportDelay = 6f;
    }
    #endregion

    private void OnTeleportMobius()
    {
        if (!mobius.IsAlive) return;
        Transform locationToTeleport = teleportationPositions[0];
        float dist = Vector2.Distance(locationToTeleport.position, player.transform.position);

        for (int i = 1; i < teleportationPositions.Length; i++)
        {
            var newDist = Vector2.Distance(teleportationPositions[i].position, player.transform.position);
            if (newDist > dist)
            {
                locationToTeleport = teleportationPositions[i];
                dist = newDist;
            }
        }

        mobius.transform.position = locationToTeleport.position;
        teleportCooldownTimer = mobiusTeleportDelay;
    }

    private void OnSpawnMinions()
    {
        //Debug.Log("Spawning Minions");
        for (int i = 1; i < teleportationPositions.Length; i++)
        {
            int index = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[index], teleportationPositions[i].position, Quaternion.identity);
        }
    }

    private void OnPlayerVictory()
    {
        teleportCooldownTimer = 50;
        player.SetInvincible(true);
        GameManager.instance.OnStageClear(8); //Player successfully cleared the final stage
        Invoke("RollCredits", 5f);

        var remainingEnemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < remainingEnemies.Length; i++)
        {
            remainingEnemies[i].OnDamageEnemy(500);
        }
    }

    //Invoked upon player victory, after short delay
    private void RollCredits()
    {
        GameManager.LoadScene(GameManager.instance.GetSceneIndex() + 1);
    }
}
