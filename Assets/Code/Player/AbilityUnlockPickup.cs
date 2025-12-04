using UnityEngine;

/// <summary>
/// Pickup para desbloquear habilidades + animación flotante
/// </summary>
public class AbilityUnlockPickup : MonoBehaviour
{
    // ===========================
    //  CONFIGURACIÓN PRINCIPAL
    // ===========================

    [Header("Tipo de Habilidad")]
    [SerializeField] private AbilityType abilityType;

    [Header("Información del Tutorial")]
    [SerializeField] private Sprite abilityIcon;
    [SerializeField] private string abilityName;
    [TextArea(3, 6)]
    [SerializeField] private string abilityDescription;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float effectDuration = 2f;

    // ===========================
    //  ANIMACIÓN (copiado del pickup de vida)
    // ===========================
    [Header("Animación")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmount = 0.5f;
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotateSpeed = 50f;

    private Vector3 startPosition;

    public enum AbilityType
    {
        DoubleJump,
        Dash,
        WallCling,
        Shield,
        RangedAttack,
        Custom
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // --- Animación flotante ---
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // --- Rotación opcional ---
        if (rotateObject)
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UnlockAbility(collision.gameObject);
        }
    }

    private void UnlockAbility(GameObject player)
    {
        Debug.Log($"🔓 Desbloqueando: {abilityName}");

        // ========== PASO 1: DESBLOQUEAR EN PLAYER ==========
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            switch (abilityType)
            {
                case AbilityType.DoubleJump: pm.canDoubleJump = true; break;
                case AbilityType.Dash: pm.canDash = true; break;
                case AbilityType.WallCling: pm.canWallCling = true; break;
                case AbilityType.Shield: pm.canBlock = true; break;
                case AbilityType.RangedAttack: pm.canThrowProjectile = true; break;
            }
        }

        // ========== PASO 2: GUARDAR EN DATOS ==========
        if (ControladorDatosJuego.Instance != null)
        {
            var datos = ControladorDatosJuego.Instance.datosjuego;

            switch (abilityType)
            {
                case AbilityType.DoubleJump: datos.hasDoubleJump = true; break;
                case AbilityType.Dash: datos.hasDash = true; break;
                case AbilityType.WallCling: datos.hasWallCling = true; break;
                case AbilityType.Shield: datos.hasShield = true; break;
                case AbilityType.RangedAttack: datos.hasRangedAttack = true; break;
            }

            ControladorDatosJuego.Instance.GuardarDatos(false);
            Debug.Log("💾 Habilidad guardada");
        }

        // ========== PASO 3: MOSTRAR TUTORIAL ==========
        if (UnlockTutorialUI.Instance != null)
        {
            UnlockTutorialUI.Instance.ShowUnlock(
                abilityIcon,
                abilityName,
                abilityDescription
            );
        }
        else
        {
            Debug.LogWarning("⚠ UnlockTutorialUI.Instance es NULL");
        }

        // ========== PASO 4: SFX + EFECTO ==========
        if (pickupSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(pickupSound, 0.7f);

        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // ========== PASO 5: DESTRUIR ==========
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
