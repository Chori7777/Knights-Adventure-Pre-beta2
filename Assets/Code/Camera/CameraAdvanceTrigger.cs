using System.Collections;
using UnityEngine;

public class CameraAdvanceTrigger : MonoBehaviour
{
    [SerializeField] private bool triggerDialogue = true;
    [SerializeField] private string dialogueText = "";
    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        if (triggerDialogue && !string.IsNullOrEmpty(dialogueText) && TextManager.Instance != null)
        {
            TextManager.Instance.ShowDialogue(dialogueText);
        }
    }
}
