using UnityEngine;
using System.Collections;

public class SimpleNPC : MonoBehaviour
{
    [Header("Diálogo")]
    [SerializeField] private string dialogueText = "Presiona E para continuar...";
    [SerializeField] private AudioClip dialogueSound;

    [Header("Tilemap a destruir")]
    [SerializeField] private GameObject tilemapToDestroy;

    [Header("Recompensas")]
    [SerializeField] private NPCReward[] rewards;

    private Transform player;
    private bool playerInRange = false;
    private bool isInteracting = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartCoroutine(Interact());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.CloseDialogue();
        }
    }

    private IEnumerator Interact()
    {
        isInteracting = true;

        // Mostrar diálogo
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ShowDialogue(dialogueText);

        if (dialogueSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(dialogueSound, 0.5f);

        yield return new WaitForSeconds(2f);

        // Destruir tilemap
        if (tilemapToDestroy != null)
        {
            Destroy(tilemapToDestroy);
            Debug.Log("Tilemap destruido");
        }

        // Dar recompensas
        foreach (NPCReward reward in rewards)
        {
            reward.Apply();
            yield return new WaitForSeconds(0.3f);
        }

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.CloseDialogue();

        yield return new WaitForSeconds(0.5f);
        isInteracting = false;
    }
}
