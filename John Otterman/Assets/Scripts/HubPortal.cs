using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubPortal : MonoBehaviour
{
    [SerializeField] private bool morbiusPortal = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        if (collision.gameObject.GetComponent<PlayerController>())
        {
            if (morbiusPortal)
            {
                SceneManager.LoadScene(5);
                return;
            }
            PlayerController.instance.ToggleMovement(false);
            PlayerController.instance.combat.ToggleCombat(false);
            UIManager.instance.StageComplete();
        }
    }
}
