using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Text dialogueField;
    int dialogueIndex = 0; //The current line of displayed dialogue
    private string[] tutorialText =
    {
        "Use 'WASD' or Arrow Keys to move.",
        "Use the mouse to aim.",
        "Attack using LMB. Reload with 'R'",
        "Swap weapons using mouse wheel or '1-6'",
        "Use your portal gun (RMB) to move between dimensions."
    };


    private void Start()
    {
        dialogueIndex = 0;
        DisplayNextDialogue();
    }

    private void DisplayNextDialogue()
    {
        if (dialogueIndex > tutorialText.Length - 1)
        {
            dialogueBox.SetActive(false);
            return;
        }
        dialogueField.text = tutorialText[dialogueIndex];
        dialogueIndex++;

        StartCoroutine(DialogueUpdateDelay());
    }

    private IEnumerator DialogueUpdateDelay()
    {
        yield return new WaitForSeconds(3.5f);
        DisplayNextDialogue();
    }
}
