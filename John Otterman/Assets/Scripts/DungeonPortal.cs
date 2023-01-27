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

    private void LoadStage(int index)
    {
        PlayerController.instance.ToggleMovement(true);
        GameManager.instance.SetStageIndex(index);
        GameManager.LoadScene(3);
    }
}
