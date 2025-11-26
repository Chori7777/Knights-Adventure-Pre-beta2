using UnityEngine;
using System.Collections;

public class SimpleNPC : MonoBehaviour
{
    [Header("Di�logo")]
    [SerializeField] private string dialogueText = "Presiona E para continuar...";
    [SerializeField] private AudioClip dialogueSound;

    [Header("Tilemap a destruir")]
    [SerializeField] private GameObject tilemapToDestroy;
    [SerializeField] private string tilemapID = "";

    [Header("Recompensas")]
    [SerializeField] private NPCReward[] rewards;
    [SerializeField] private string npcID = "";
    [SerializeField] private bool recompensaUnaVez = true;

    private Transform player;
    private bool playerInRange = false;
    private bool isInteracting = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (tilemapToDestroy != null && ControladorDatosJuego.Instance != null && !string.IsNullOrEmpty(tilemapID))
        {
            if (ControladorDatosJuego.Instance.EstaObjetoDestruido(tilemapID))
            {
                tilemapToDestroy.SetActive(false);
            }
        }
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
            if (TextManager.Instance != null)
                TextManager.Instance.CloseDialogue();
        }
    }

    private IEnumerator Interact()
    {
        isInteracting = true;

        // Mostrar diálogo
        if (TextManager.Instance != null)
            TextManager.Instance.ShowDialogue(dialogueText);

        if (dialogueSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(dialogueSound, 0.5f);

        yield return new WaitForSeconds(2f);

        // Destruir tilemap
        if (tilemapToDestroy != null)
        {
            tilemapToDestroy.SetActive(false);
            if (ControladorDatosJuego.Instance != null && !string.IsNullOrEmpty(tilemapID))
            {
                ControladorDatosJuego.Instance.MarcarObjetoDestruido(tilemapID);
            }
        }

        // Dar recompensas
        bool puedeDar = true;
        if (recompensaUnaVez && ControladorDatosJuego.Instance != null && !string.IsNullOrEmpty(npcID))
        {
            if (ControladorDatosJuego.Instance.EstaNPCRecompensaEntregada(npcID))
            {
                puedeDar = false;
            }
        }

        if (puedeDar)
        {
            foreach (NPCReward reward in rewards)
            {
                reward.Apply();
                yield return new WaitForSeconds(0.3f);
            }

            if (recompensaUnaVez && ControladorDatosJuego.Instance != null && !string.IsNullOrEmpty(npcID))
            {
                ControladorDatosJuego.Instance.MarcarNPCRecompensaEntregada(npcID);
            }
        }

        if (TextManager.Instance != null)
            TextManager.Instance.CloseDialogue();

        yield return new WaitForSeconds(0.5f);
        isInteracting = false;
    }
}
