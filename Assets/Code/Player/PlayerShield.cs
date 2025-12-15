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

    [Header("Modo Mago - Vida Temporal")]
    [SerializeField] private bool useTemporaryHealthShieldForMage = true;
    [SerializeField] private int tempShieldAmount = 2;
    [SerializeField] private float tempShieldDuration = 8f;
    [SerializeField] private int shieldChargesMax = 2;
    [SerializeField] private float shieldChargeCooldown = 15f;
    private int shieldCharges;
    private bool restoringCharge;

    [Header("Visuales - Sorting")]
    [SerializeField] private bool matchShieldSortingToPlayer = true;
    [SerializeField] private int shieldSortingOffset = 1;

    [Header("Mage Overshield Bar (World)")]
    [SerializeField] private Transform mageShieldBarRoot;
    [SerializeField] private UnityEngine.UI.Image mageShieldBarImage;
    [SerializeField] private Vector2 mageShieldBarOffset = new Vector2(0f, 1.5f);
    [SerializeField] private string mageShieldBarObjectName = "Shield Image";
    [SerializeField] private AudioClip shieldHitWhileActiveSound;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private bool isBlocking = false;
    private float lastBlockTime = -10f;
    private float currentStamina = 1f;
    private int prevTempShield = 0;

    public bool IsBlocking => isBlocking;
    public float Stamina01 => Mathf.Clamp01(currentStamina / maxStamina);

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        shieldCharges = shieldChargesMax;
        EnsureShieldVisualReferences();
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        if (alternateShieldVisual != null)
            alternateShieldVisual.SetActive(false);
        var life = GetComponent<playerLife>();
        if (life != null) prevTempShield = life.TempShield;
        EnsureMageShieldBarReferences();
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
        if (activeVisual == null && (shieldVisual == null && alternateShieldVisual == null))
        {
            EnsureShieldVisualReferences();
            activeVisual = useAlternateShield && alternateShieldVisual != null ? alternateShieldVisual : shieldVisual;
            inactiveVisual = useAlternateShield && alternateShieldVisual != null ? shieldVisual : alternateShieldVisual;
        }
        if (activeVisual != null) activeVisual.SetActive(isBlocking);
        if (inactiveVisual != null) inactiveVisual.SetActive(false);
        if (matchShieldSortingToPlayer && activeVisual != null)
        {
            var srShield = activeVisual.GetComponent<SpriteRenderer>();
            var srPlayer = GetComponent<SpriteRenderer>();
            if (srShield != null && srPlayer != null)
            {
                srShield.sortingLayerID = srPlayer.sortingLayerID;
                srShield.sortingOrder = srPlayer.sortingOrder + shieldSortingOffset;
            }
        }

        UpdateMageShieldBarWorld();
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
        if (blockEffectPrefab != null)
        {
            Vector3 effectPos = transform.position;
            GameObject effect = Instantiate(blockEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, blockEffectDuration);
        }
        var life = GetComponent<playerLife>();
        if (life != null && life.IsSecondCharacterMage && useTemporaryHealthShieldForMage)
        {
            if (shieldCharges > 0)
            {
                life.AddTemporaryShieldHealth(tempShieldAmount, tempShieldDuration);
                shieldCharges = Mathf.Max(0, shieldCharges - 1);
                if (!restoringCharge) StartCoroutine(RestoreShieldChargeAfterCooldown());
                Debug.Log($"[PlayerShield] Charge usada. Quedan {shieldCharges}/{shieldChargesMax}");
            }
            else
            {
                Debug.Log("[PlayerShield] No hay charges para overshield");
            }
        }
    }

    /// <summary>
    /// Reducir velocidad al bloquear (llamar desde PlayerMovement)
    /// </summary>
    public float GetMovementMultiplier()
    {
        return isBlocking ? 0.5f : 1f; // 50% de velocidad bloqueando
    }

    public int ShieldCharges => shieldCharges;
    public int ShieldChargesMax => shieldChargesMax;

    private IEnumerator RestoreShieldChargeAfterCooldown()
    {
        restoringCharge = true;
        yield return new WaitForSeconds(shieldChargeCooldown);
        shieldCharges = Mathf.Min(shieldCharges + 1, shieldChargesMax);
        restoringCharge = false;
    }

    private void EnsureShieldVisualReferences()
    {
        if (useAlternateShield && alternateShieldVisual == null && shieldVisual != null)
        {
            useAlternateShield = false;
        }
        if (shieldVisual == null && alternateShieldVisual == null)
        {
            Transform t = transform.Find("Shield Image");
            if (t == null) t = transform.Find("Shield");
            if (t == null) t = transform.Find("ShieldAura");
            if (t != null)
            {
                shieldVisual = t.gameObject;
            }
        }
    }

    private void EnsureMageShieldBarReferences()
    {
        if (mageShieldBarRoot == null)
        {
            var t = transform.Find(mageShieldBarObjectName);
            if (t != null) mageShieldBarRoot = t;
        }
        if (mageShieldBarImage == null && mageShieldBarRoot != null)
        {
            mageShieldBarImage = mageShieldBarRoot.GetComponent<UnityEngine.UI.Image>();
        }
        if (mageShieldBarRoot != null) mageShieldBarRoot.gameObject.SetActive(false);
    }

    private void UpdateMageShieldBarWorld()
    {
        var life = GetComponent<playerLife>();
        if (life == null || !life.IsSecondCharacterMage) return;
        if (mageShieldBarRoot == null && mageShieldBarImage == null) EnsureMageShieldBarReferences();
        if (mageShieldBarRoot == null && mageShieldBarImage == null) return;
        int tsMax = Mathf.Max(1, life.TempShieldMax);
        int ts = Mathf.Clamp(life.TempShield, 0, tsMax);
        bool active = ts > 0;
        Transform root = mageShieldBarRoot != null ? mageShieldBarRoot : mageShieldBarImage != null ? mageShieldBarImage.transform : null;
        if (root == null) return;
        if (root.gameObject.activeSelf != active) root.gameObject.SetActive(active);
        if (active)
        {
            Vector3 pos = transform.position + new Vector3(mageShieldBarOffset.x, mageShieldBarOffset.y, 0f);
            root.position = pos;
            float ratio = Mathf.Clamp01(ts / (float)tsMax);
            if (mageShieldBarImage != null)
            {
                mageShieldBarImage.fillAmount = ratio;
            }
            else
            {
                root.localScale = new Vector3(Mathf.Max(0.0001f, ratio), root.localScale.y, root.localScale.z);
            }
            if (prevTempShield > ts)
            {
                if (shieldHitWhileActiveSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(shieldHitWhileActiveSound, 0.8f);
                }
            }
        }
        prevTempShield = ts;
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
