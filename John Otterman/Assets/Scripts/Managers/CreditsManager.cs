using UnityEngine;

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
            GameManager.LoadScene(2);
        }
        
    }
}
