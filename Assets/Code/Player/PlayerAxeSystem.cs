using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de cargas de hachas para ataque a distancia
/// </summary>
public class PlayerAxeSystem : MonoBehaviour
{
    [Header("Proyectil")]
    [SerializeField] private GameObject axePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private float throwCooldown = 0.5f;

    [Header("Munición")]
    [SerializeField] private int currentAxes = 3;
    [SerializeField] private int maxAxes = 3;

    [Header("Auto Recarga")]
    [SerializeField] private bool enableAutoRecharge = true;
    [SerializeField] private float autoRechargeInterval = 5f;
    [SerializeField] private int autoRechargeAmount = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip emptySound;

    private float lastThrowTime = -10f;
    private bool facingRight = true;

    public int CurrentAxes => currentAxes;
    public int MaxAxes => maxAxes;

    private void Awake()
    {
        // Cargar desde datos guardados
        if (ControladorDatosJuego.Instance != null)
        {
            currentAxes = ControladorDatosJuego.Instance.datosjuego.cantidadHachas;
            maxAxes = ControladorDatosJuego.Instance.datosjuego.maxHachas;
        }

        CreateThrowPointIfNeeded();
        pm = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        UpdateUI();
        if (enableAutoRecharge && autoRechargeRoutine == null)
            autoRechargeRoutine = StartCoroutine(AutoRechargeRoutine());
    }

    private void CreateThrowPointIfNeeded()
    {
        if (throwPoint == null)
        {
            GameObject pointObj = new GameObject("AxeThrowPoint");
            pointObj.transform.SetParent(transform);
            pointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
            throwPoint = pointObj.transform;
        }
    }

    private PlayerMovement pm;
    private Coroutine autoRechargeRoutine;

    private void Update()
    {
        // Actualizar dirección
        facingRight = transform.localScale.x > 0;

        // Detectar input de lanzamiento
        if (pm != null && !pm.canThrowProjectile) return;
        if (Input.GetKeyDown(KeyCode.C))
        {
            TryThrowAxe();
        }
    }

    private IEnumerator AutoRechargeRoutine()
    {
        while (enableAutoRecharge)
        {
            if (currentAxes < maxAxes)
            {
                currentAxes = Mathf.Min(currentAxes + autoRechargeAmount, maxAxes);
                UpdateUI();
                SaveAxeCount();
            }
            yield return new WaitForSeconds(autoRechargeInterval);
        }
        autoRechargeRoutine = null;
    }

    public void SetAutoRechargeEnabled(bool value)
    {
        enableAutoRecharge = value;
        if (enableAutoRecharge && autoRechargeRoutine == null)
            autoRechargeRoutine = StartCoroutine(AutoRechargeRoutine());
        if (!enableAutoRecharge && autoRechargeRoutine != null)
        {
            StopCoroutine(autoRechargeRoutine);
            autoRechargeRoutine = null;
        }
    }

    /// <summary>
    /// Intenta lanzar un hacha
    /// </summary>
    public bool TryThrowAxe()
    {
        // Verificar cooldown
        if (Time.time < lastThrowTime + throwCooldown)
            return false;

        // Verificar munición
        if (currentAxes <= 0)
        {
            PlayEmptySound();
            Debug.Log("🪓 Sin hachas!");
            return false;
        }

        // Lanzar hacha
        ThrowAxe();
        return true;
    }

    private void ThrowAxe()
    {
        // Consumir munición
        currentAxes--;
        lastThrowTime = Time.time;

        // Crear proyectil
        if (axePrefab != null && throwPoint != null)
        {
            GameObject axe = Instantiate(axePrefab, throwPoint.position, Quaternion.identity);

            // Configurar velocidad
            Rigidbody2D axeRb = axe.GetComponent<Rigidbody2D>();
            if (axeRb != null)
            {
                float direction = facingRight ? 1f : -1f;
                axeRb.linearVelocity = new Vector2(direction * throwSpeed, 0);
            }

            // Rotar sprite
            float rotation = facingRight ? 0f : 180f;
            axe.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        // Sonido
        if (throwSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(throwSound, 0.5f);
        }

        // Actualizar UI y guardar
        UpdateUI();
        SaveAxeCount();

        Debug.Log($"🪓 Hacha lanzada! Restantes: {currentAxes}/{maxAxes}");
    }

    private void PlayEmptySound()
    {
        if (emptySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(emptySound, 0.3f);
        }
    }

    /// <summary>
    /// Añadir hachas (recoger del mapa)
    /// </summary>
    public void AddAxes(int amount)
    {
        currentAxes = Mathf.Min(currentAxes + amount, maxAxes);
        UpdateUI();
        SaveAxeCount();
        Debug.Log($"🪓 +{amount} hachas! Total: {currentAxes}/{maxAxes}");
    }

    /// <summary>
    /// Rellenar todas las hachas
    /// </summary>
    public void RefillAxes()
    {
        currentAxes = maxAxes;
        UpdateUI();
        SaveAxeCount();
        Debug.Log($"🪓 Hachas recargadas! {currentAxes}/{maxAxes}");
    }

    /// <summary>
    /// Aumentar capacidad máxima
    /// </summary>
    public void IncreaseMaxAxes(int amount = 1)
    {
        maxAxes += amount;
        currentAxes = Mathf.Min(currentAxes, maxAxes); // No exceder el nuevo máximo
        UpdateUI();
        SaveAxeCount();
        Debug.Log($"🪓 Capacidad aumentada! Nuevo máximo: {maxAxes}");
    }

    private void UpdateUI()
    {
        if (PlayerHealthUI.Instance != null)
        {
            PlayerHealthUI.Instance.ActualizarHachas(currentAxes);
        }
    }

    private void SaveAxeCount()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.datosjuego.cantidadHachas = currentAxes;
            ControladorDatosJuego.Instance.datosjuego.maxHachas = maxAxes;
            ControladorDatosJuego.Instance.GuardarDatos(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (throwPoint == null) return;

        Gizmos.color = Color.yellow;
        Vector3 direction = facingRight ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(throwPoint.position, throwPoint.position + direction * 3f);
        Gizmos.DrawWireSphere(throwPoint.position, 0.2f);
    }
}

// ========== PICKUP DE HACHAS ==========
// Crea este script para objetos recolectables:
/*
public class AxePickup : MonoBehaviour
{
    [SerializeField] private int axeAmount = 1;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerAxeSystem axeSystem = collision.GetComponent<PlayerAxeSystem>();
            if (axeSystem != null)
            {
                axeSystem.AddAxes(axeAmount);
                
                if (pickupSound != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(pickupSound, 0.5f);
                
                Destroy(gameObject);
            }
        }
    }
}
*/
