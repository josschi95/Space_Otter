using UnityEngine;
using UnityEngine.UI;

public class DungeonPortal : MonoBehaviour
{
    [SerializeField] private GameObject portalUI;
    [SerializeField] private Button[] stageButtons; //used to select the stage in the dimension
    [SerializeField] private Button panelCloseButton;
    [SerializeField] private GameObject[] stageLockIcons; //indicator that the stage is not yet unlocked

    private void Start()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stage = i;
            stageButtons[stage].onClick.AddListener(delegate { LoadStage(stage); });
        }

        panelCloseButton.onClick.AddListener(ClosePortal);

        ClosePortal();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>()) OnPortalEnter();
    }

    void OnPortalEnter()
    {
        CheckForUnlockedStages();

        portalUI.SetActive(true);
        PlayerController.instance.ToggleMovement(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CheckForUnlockedStages()
    {
        //Buttons are enabled if the stage before them has been cleared
        for (int i = 1; i < stageLockIcons.Length; i++)
        {
            stageLockIcons[i].SetActive(!GameManager.instance.ClearedStages[i -1]);
            stageButtons[i].interactable = GameManager.instance.ClearedStages[i - 1];
        }

        //stage 0 is available from the start
        stageLockIcons[0].SetActive(false);
        stageButtons[0].interactable = true;
    }

    void ClosePortal()
    {
        portalUI.SetActive(false);
        PlayerController.instance.ToggleMovement(true);


        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void LoadStage(int stage)
    {
        int buildIndex = 3; //Dungeon_00
        int stageLevel = 0; //First stage of that dungeon

        if (stage >= 6) 
        {
            buildIndex = 7; //Dungeon_02
            if (stage == 7) stageLevel = 1;
            else if (stage == 8) stageLevel = 2;
        }
        else if (stage >= 3) 
        {
            buildIndex = 5; //Dungeon_01
            if (stage == 4) stageLevel = 1;
            else if (stage == 5) stageLevel = 2;
        }
        else
        {
            stageLevel = stage;
        }

        PlayerController.instance.ToggleMovement(true);
        GameManager.instance.SetStageDifficulty(stageLevel);
        GameManager.LoadScene(buildIndex);
    }
}
