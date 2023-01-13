using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] private float returnToMainDelay = 10f;
    private float noInputAllowedTimer;

    private void Awake()
    {
        if (GameManager.instance != null) Destroy(GameManager.instance.gameObject);
        if (PlayerController.instance != null) Destroy(PlayerController.instance.gameObject);
        if (UIManager.instance != null) Destroy(UIManager.instance.gameObject);
    }

    private void Start()
    {
        noInputAllowedTimer = returnToMainDelay;
    }

    private void Update()
    {
        noInputAllowedTimer -= Time.deltaTime;
        if (noInputAllowedTimer <= 0 && Input.anyKey)
        {
            //return to main menu
            SceneManager.LoadScene(0);
        }
        
    }
}
