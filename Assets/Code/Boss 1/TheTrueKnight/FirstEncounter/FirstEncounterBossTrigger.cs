using UnityEngine;
using System.Collections;

public class FirstEncounterBossTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FirstEncounterBossController bossController;
    [Header("Diálogo de Intro")]
    [SerializeField] private string[] introLines;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        if (TextManager.Instance != null && introLines != null && introLines.Length > 0)
        {
            for (int i = 0; i < introLines.Length; i++)
            {
                TextManager.Instance.ShowDialogue(introLines[i]);
                yield return new WaitForSeconds(2f);
            }
            TextManager.Instance.CloseDialogue();
        }
        if (bossController != null)
        {
            bossController.StartFight();
        }
    }
}
