using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de portal bidireccional con cooldown anti-bug
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("Configuración de Portal")]
    [SerializeField] private Transform destination; // Punto de destino
    [SerializeField] private Portal pairedPortal;   // Portal conectado (opcional)
    [SerializeField] private bool autoUnlockPairedOnEnter = true;

    [Header("Cooldown")]
    [SerializeField] private float teleportCooldown = 1f;
    [SerializeField] private float playerImmunityTime = 0.5f; // Tiempo de inmunidad tras teletransporte

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject teleportEffect;
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private Color portalColor = Color.cyan;

    [Header("Audio")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private AudioClip enterSound;

    [Header("Animación")]
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private string activateTrigger = "Activate";

    private bool canTeleport = true;
    private float lastTeleportTime = -10f;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = portalColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!canTeleport) return;
        if (Time.time < lastTeleportTime + teleportCooldown) return;

        StartCoroutine(TeleportPlayer(collision.gameObject));
    }

    private IEnumerator TeleportPlayer(GameObject player)
    {
        canTeleport = false;
        lastTeleportTime = Time.time;

        // Desactivar controles
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
            playerMovement.canJump = false;
            playerMovement.canAttack = false;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        // Sonido de entrada
        if (enterSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(enterSound, 0.7f);

        // Animación del portal
        if (portalAnimator != null && !string.IsNullOrEmpty(activateTrigger))
            portalAnimator.SetTrigger(activateTrigger);

        // Efecto visual en origen
        if (teleportEffect != null)
        {
            GameObject effect = Instantiate(teleportEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // Fade out (opcional)
        if (FadeController.Instance != null)
            FadeController.Instance.ActivarFadeOut();

        yield return new WaitForSeconds(0.3f);

        // Teletransportar
        if (destination != null)
        {
            player.transform.position = destination.position;
            Debug.Log($"[Portal] Jugador teletransportado a {destination.name}");
        }
        else
        {
            Debug.LogWarning("[Portal] No hay destino asignado al portal");
        }

        if (pairedPortal != null && autoUnlockPairedOnEnter)
        {
            pairedPortal.Unlock();
        }

        // Sonido de salida
        if (teleportSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(teleportSound, 0.7f);

        // Efecto visual en destino
        if (teleportEffect != null && destination != null)
        {
            GameObject effect = Instantiate(teleportEffect, destination.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // Fade in
        if (FadeController.Instance != null)
            FadeController.Instance.ActivarFadeIn();

        yield return new WaitForSeconds(0.2f);

        // Restaurar controles
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
            playerMovement.canJump = true;
            playerMovement.canAttack = true;
        }

        // Inmunidad temporal en el portal destino
        if (pairedPortal != null)
        {
            pairedPortal.SetImmunity(playerImmunityTime);
        }

        yield return new WaitForSeconds(teleportCooldown - 0.5f);
        canTeleport = true;
    }

    /// <summary>
    /// Otorga inmunidad temporal al portal (evita teletransporte instantáneo de vuelta)
    /// </summary>
    public void SetImmunity(float duration)
    {
        StartCoroutine(ImmunityCoroutine(duration));
    }

    public void Unlock()
    {
        var go = gameObject;
        if (!go.activeSelf) go.SetActive(true);
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        canTeleport = true;
    }

    private IEnumerator ImmunityCoroutine(float duration)
    {
        canTeleport = false;
        yield return new WaitForSeconds(duration);
        canTeleport = true;
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar conexión al destino
        if (destination != null)
        {
            Gizmos.color = portalColor;
            Gizmos.DrawLine(transform.position, destination.position);
            Gizmos.DrawWireSphere(destination.position, 0.5f);
        }

        // Dibujar área de activación
        Gizmos.color = new Color(portalColor.r, portalColor.g, portalColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
