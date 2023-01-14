using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonPortal : MonoBehaviour
{
    [SerializeField] private GameObject[] dimensionPanels;
    [SerializeField] private Button[] dimensionTabs; //used to switch between dimensions

    [SerializeField] private Button[] stageButtons; //used to select the stage in the dimension
    [SerializeField] private GameObject[] stageLockIcons; //indicator that the stage is not yet unlocked
    [Space]
    public GameObject portalUI;
    [SerializeField] private Button panelCloseButton;

    private void Start()
    {
        for (int i = 0; i < dimensionTabs.Length; i++)
        {
            int tab = i;
            dimensionTabs[tab].onClick.AddListener(delegate { ToggleDimensionTabs(tab); });
        }

        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stage = i;
            stageButtons[i].onClick.AddListener(delegate { LoadStage(stage); });
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

        //Open to the tab with the highest available stage
        int currentDimensionTab = 0;
        if (GameManager.instance.ClearedStages[5] == true) currentDimensionTab = 2;
        else if (GameManager.instance.ClearedStages[2] == true) currentDimensionTab = 1;
        ToggleDimensionTabs(currentDimensionTab);

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

    private void ToggleDimensionTabs(int dimension)
    {
        for (int i = 0; i < dimensionPanels.Length; i++)
        {
            dimensionPanels[i].SetActive(i == dimension);
        }
    }

    private void LoadStage(int stage)
    {
        int buildIndex = 2; //Dungeon_00
        int stageLevel = 0; //First stage of that dungeon

        if (stage >= 6)
        {
            buildIndex = 4;
            if (stage == 7) stageLevel = 1;
            else if (stage == 8) stageLevel = 2;
        }
        else if (stage >= 3)
        {
            buildIndex = 3;
            if (stage == 4) stageLevel = 1;
            else if (stage == 5) stageLevel = 2;
        }
        else
        {
            stageLevel = stage;
        }

        PlayerController.instance.ToggleMovement(true);
        //Debug.Log("Loading Stage " + stageLevel + " of scene at build Index " + buildIndex + ": " + SceneManager.GetSceneByBuildIndex(buildIndex).name);
        GameManager.instance.SetStageDifficulty(stageLevel);
        SceneManager.LoadScene(buildIndex);
    }
}
