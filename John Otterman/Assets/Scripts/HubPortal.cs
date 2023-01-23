using UnityEngine;

public class HubPortal : MonoBehaviour
{
    [SerializeField] private bool bossPortal = false;
    private int bossStageIndex;
    public void SetBossStageIndex(int index)
    {
        bossStageIndex = index;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (bossPortal)
            {
                GameManager.LoadScene(bossStageIndex);
                return;
            }
            PlayerController.instance.ToggleMovement(false);
            PlayerController.instance.combat.ToggleCombat(false);
            UIManager.instance.StageComplete();
        }
    }
}
