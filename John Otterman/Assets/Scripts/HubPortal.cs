using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubPortal : MonoBehaviour
{
    [SerializeField] private bool bossPortal = false;
    [SerializeField] private int bossSceneIndex = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (bossPortal)
            {
                SceneManager.LoadScene(bossSceneIndex);
                return;
            }
            PlayerController.instance.ToggleMovement(false);
            PlayerController.instance.combat.ToggleCombat(false);
            UIManager.instance.StageComplete();
        }
    }
}
