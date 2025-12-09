using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de escudo para el jugador
/// </summary>
public class PlayerShield : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject shieldVisual;     // Sprite del escudo
    [SerializeField] private bool useAlternateShield = false;
    [SerializeField] private GameObject alternateShieldVisual;
    [SerializeField] private AudioClip blockSound;        // Sonido al bloquear
    [SerializeField] private AudioClip shieldBreakSound;  // Sonido si se rompe

    [Header("Configuración")]
    [SerializeField] private float blockAngle = 120f;
    [SerializeField] private float staminaCost = 0.2f;
    [SerializeField] private float perfectBlockWindow = 0.2f;

    [Header("Stamina del Escudo")]
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float staminaDrainPerSecond = 0.2f;
    [SerializeField] private float staminaRegenPerSecond = 0.3f;
    [SerializeField] private float minStaminaToBlock = 0.1f;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject blockEffectPrefab; // Efecto al bloquear
    [SerializeField] private float blockEffectDuration = 0.3f;

    [Header("Knockback al Bloquear")]
    [SerializeField] private float blockKnockback = 2f;    // Empuje al bloquear
    [SerializeField] private bool canParry = true;         // ¿Puede hacer parry?

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private bool isBlocking = false;
    private float lastBlockTime = -10f;
    private float currentStamina = 1f;

    public bool IsBlocking => isBlocking;
    public float Stamina01 => Mathf.Clamp01(currentStamina / maxStamina);

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        if (alternateShieldVisual != null)
            alternateShieldVisual.SetActive(false);
    }

    private void Update()
    {
        if (!playerMovement.canBlock) return;
        if (Input.GetKeyDown(KeyCode.X)) OnBlockStart();
        bool inputBlocking = Input.GetKey(KeyCode.X);
        bool canUseShield = currentStamina >= minStaminaToBlock;
        isBlocking = inputBlocking && canUseShield;
        if (isBlocking)
        {
            currentStamina = Mathf.Max(0f, currentStamina - staminaDrainPerSecond * Time.deltaTime);
        }
        else
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenPerSecond * Time.deltaTime);
        }
        GameObject activeVisual = useAlternateShield && alternateShieldVisual != null ? alternateShieldVisual : shieldVisual;
        GameObject inactiveVisual = useAlternateShield && alternateShieldVisual != null ? shieldVisual : alternateShieldVisual;
        if (activeVisual != null) activeVisual.SetActive(isBlocking);
        if (inactiveVisual != null) inactiveVisual.SetActive(false);
    }

    /// <summary>
    /// Intenta bloquear un ataque. Retorna true si lo bloqueó exitosamente.
    /// </summary>
    public bool TryBlockAttack(Vector2 attackDirection, int damage, out bool isPerfectBlock)
    {
        isPerfectBlock = false;

        if (!isBlocking || !playerMovement.canBlock)
            return false;

        // Calcular ángulo del ataque
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(facingDirection, attackDirection);

        // Verificar si el ataque viene de frente
        if (angle <= blockAngle / 2f)
        {
            isPerfectBlock = canParry && (Time.time - lastBlockTime < perfectBlockWindow);

            OnSuccessfulBlock(attackDirection, isPerfectBlock);

            if (!isPerfectBlock && rb != null)
            {
                Vector2 knockbackDir = attackDirection.normalized;
                rb.linearVelocity = knockbackDir * blockKnockback;
            }
            currentStamina = Mathf.Max(0f, currentStamina - staminaCost);
            return true;
        }

        return false;
    }

    private void OnSuccessfulBlock(Vector2 attackDirection, bool isPerfect)
    {
        // Sonido
        if (AudioManager.Instance != null)
        {
            AudioClip soundToPlay = isPerfect ? shieldBreakSound : blockSound;
            if (soundToPlay != null)
            {
                float pitch = isPerfect ? 1.2f : 1f;
                AudioManager.Instance.PlaySFX(soundToPlay, 0.7f, pitch);
            }
        }

        // Efecto visual
        if (blockEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + (Vector3)attackDirection.normalized * 0.5f;
            GameObject effect = Instantiate(blockEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, blockEffectDuration);
        }

        // Log para debugging
        if (isPerfect)
            Debug.Log("[PlayerShield] Bloqueo perfecto (parry)");
        else
            Debug.Log("[PlayerShield] Ataque bloqueado");
    }

    /// <summary>
    /// Llamar cuando el jugador empieza a bloquear
    /// </summary>
    public void OnBlockStart()
    {
        lastBlockTime = Time.time;
    }

    /// <summary>
    /// Reducir velocidad al bloquear (llamar desde PlayerMovement)
    /// </summary>
    public float GetMovementMultiplier()
    {
        return isBlocking ? 0.5f : 1f; // 50% de velocidad bloqueando
    }

    private void OnDrawGizmosSelected()
    {
        if (!isBlocking) return;

        // Visualizar ángulo de bloqueo
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector3 leftBound = Quaternion.Euler(0, 0, blockAngle / 2f) * facingDirection;
        Vector3 rightBound = Quaternion.Euler(0, 0, -blockAngle / 2f) * facingDirection;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBound * 2f);
        Gizmos.DrawLine(transform.position, transform.position + rightBound * 2f);
    }
}

// ========== INTEGRACIÓN CON playerLife.cs ==========
// En playerLife.TakeDamage(), ANTES de aplicar daño:
/*
PlayerShield shield = GetComponent<PlayerShield>();
if (shield != null)
{
    bool isPerfect;
    if (shield.TryBlockAttack(attackerPosition - (Vector2)transform.position, damage, out isPerfect))
    {
        if (isPerfect)
        {
            // Bloqueo perfecto: sin daño, posible contraataque
            return;
        }
        else
        {
            // Bloqueo normal: daño reducido
            damage = Mathf.Max(1, damage / 2);
        }
    }
}
*/
