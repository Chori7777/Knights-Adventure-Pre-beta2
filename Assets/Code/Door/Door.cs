using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Configuración de puerta")]
    public bool isUpDoor;
    public Transform destination;
    public Animator doorAnimator;
    public float transitionDuration = 1.5f;
    public float fadeInDurationExtra = 0.8f; // 🔹 tiempo extra para hacer el FadeIn más suave

    private bool playerInRange = false;
    private GameObject player;
    private bool isUsingDoor = false; // 🔹 evita que se dispare varias veces seguidas

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerInRange = true;
            Debug.Log($"✅ [Door:{name}] Jugador detectado: {player.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log($"⚪ [Door:{name}] Jugador salió del rango");
        }
    }

    private void Update()
    {
        if (playerInRange && !isUsingDoor && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"🟠 [Door:{name}] Tecla E presionada, iniciando UseDoor()");
            StartCoroutine(UseDoor());
        }
    }

    private IEnumerator UseDoor()
    {
        isUsingDoor = true; // 🔹 bloquea múltiples activaciones
        Debug.Log($"🚪 [Door:{name}] UseDoor iniciado");

        if (player == null)
        {
            Debug.LogError($"❌ [Door:{name}] player es NULL — No se puede continuar.");
            isUsingDoor = false;
            yield break;
        }

        if (destination == null)
        {
            Debug.LogError($"❌ [Door:{name}] destination es NULL — Asigná un destino en el Inspector.");
            isUsingDoor = false;
            yield break;
        }

        // --- 1. Fade out ---
        if (FadeController.Instance != null)
        {
            Debug.Log($"🌑 [Door:{name}] Activando FadeOut...");
            FadeController.Instance.ActivarFadeOut();
        }

        // --- 2. Ocultar jugador inmediatamente ---
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
            Debug.Log($"🙈 [Door:{name}] Jugador ocultado");
        }

        // --- 3. Animar puerta ---
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
            Debug.Log($"🎞️ [Door:{name}] Animación de puerta activada");
        }

        // Esperar la duración del fade/animación
        yield return new WaitForSeconds(transitionDuration);

        // --- 4. Mover jugador ---
        player.transform.position = destination.position;
        Debug.Log($"➡️ [Door:{name}] Jugador movido a {destination.name}");

        // --- 5. Fade in ---
        if (FadeController.Instance != null)
        {
            Debug.Log($"🌕 [Door:{name}] Activando FadeIn...");
            FadeController.Instance.ActivarFadeIn();
        }

        // Espera extra para que el fade-in sea más largo
        yield return new WaitForSeconds(fadeInDurationExtra);

        // --- 6. Mostrar jugador nuevamente ---
        if (sr != null)
        {
            sr.enabled = true;
            Debug.Log($"👀 [Door:{name}] Jugador visible nuevamente");
        }

        Debug.Log($"✅ [Door:{name}] UseDoor completado");
        isUsingDoor = false;
    }
}
