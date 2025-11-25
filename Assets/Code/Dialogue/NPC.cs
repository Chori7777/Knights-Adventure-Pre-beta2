using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private string npcID = "";
    [SerializeField] private bool recompensaUnaVez = true;
    [Header("Diálogo")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private string[] dialogueLines;

    [Header("Recompensas")]
    [SerializeField] private bool giveRewards = false;
    [SerializeField] private NPCReward[] rewards;

    [Header("Tilemap")]
    [SerializeField] private bool destroyTilemap = false;
    [SerializeField] private GameObject tilemapToDestroy;
    [SerializeField] private string tilemapID = "";

    private Transform player;
    private int currentDialogueLine = 0;
    private bool dialogueActive = false;
    private bool isInRange = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        if (destroyTilemap && tilemapToDestroy != null && ControladorDatosJuego.Instance != null)
        {
            if (!string.IsNullOrEmpty(tilemapID) && ControladorDatosJuego.Instance.EstaObjetoDestruido(tilemapID))
            {
                tilemapToDestroy.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        isInRange = distance <= interactionDistance;

        if (isInRange && Input.GetKeyDown(KeyCode.E) && !dialogueActive)
            StartDialogue();

        if (dialogueActive && Input.GetKeyDown(KeyCode.E))
            NextDialogue();

        FlipTowardsPlayer();

        if (!isInRange && dialogueActive)
            EndDialogue();
    }

    private void FlipTowardsPlayer()
    {
        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void StartDialogue()
    {
        dialogueActive = true;
        currentDialogueLine = 0;
        ShowCurrentLine();
    }

    private void NextDialogue()
    {
        currentDialogueLine++;

        if (currentDialogueLine < dialogueLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowCurrentLine()
    {
        TextManager.Instance.ShowDialogue(dialogueLines[currentDialogueLine]);
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        currentDialogueLine = 0;

        TextManager.Instance.CloseDialogue();

        // --- ACCIONES POST DIÁLOGO ---

        // 1. Dar recompensas (si está activado)
        if (giveRewards && rewards != null)
        {
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
                foreach (var r in rewards)
                    r.Apply();

                if (recompensaUnaVez && ControladorDatosJuego.Instance != null && !string.IsNullOrEmpty(npcID))
                {
                    ControladorDatosJuego.Instance.MarcarNPCRecompensaEntregada(npcID);
                }
            }
        }

        // 2. Destruir tilemap (si está activado)
        if (destroyTilemap && tilemapToDestroy != null)
        {
            tilemapToDestroy.SetActive(false);
            if (ControladorDatosJuego.Instance != null && !string.IsNullOrEmpty(tilemapID))
            {
                ControladorDatosJuego.Instance.MarcarObjetoDestruido(tilemapID);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
