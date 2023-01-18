using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] private float returnToMainDelay = 10f;
    private float noInputAllowedTimer;

    private void Start()
    {
        noInputAllowedTimer = returnToMainDelay;
    }

    private void Update()
    {
        noInputAllowedTimer -= Time.deltaTime;
        if (noInputAllowedTimer <= 0 && Input.anyKey)
        {
            //return to hub
            SceneManager.LoadScene(1);
        }
        
    }
}
