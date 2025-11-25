using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class playerLife : MonoBehaviour
{
    private PlayerMovement controller;
    private PlayerHealthUI healthUI;
    private PlayerAnimationController animController;
    private Animator fallbackAnimator;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Pociones")]
    [SerializeField] private int maxPotions = 5;
    [SerializeField] private int currentPotions = 3;
    [SerializeField] private int potionHealAmount = 1;
    [SerializeField] private float potionCooldown = 0.5f;
    [SerializeField] private CooldownIndicator potionCooldownIndicator;
    private float lastPotionTime = -10f;
    public int Potions => currentPotions;
    public int MaxPotions => maxPotions;

    [Header("Sistema de Daño")]
    [SerializeField] private float invincibilityDuration = 1f;
    private float lastDamageTime = -10f;
    private bool isTakingDamage = false;
    public bool IsTakingDamage => isTakingDamage;

    [Header("Controles")]
    [SerializeField] private KeyCode usePotionKey = KeyCode.R;

    private bool isDead = false;

    [Header("Muerte")]
    [SerializeField] private string deathAnimationName = "Death";
    [SerializeField] private float deathFallbackDuration = 1f;

    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        controller = GetComponent<PlayerMovement>();
        animController = GetComponent<PlayerAnimationController>();
        fallbackAnimator = GetComponent<Animator>();

        if (animController != null && controller != null)
            animController.Initialize(controller);

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private IEnumerator Start()
    {
        // ✅ Esperar a que el HUD esté disponible
        float timeout = 2.0f;
        float elapsed = 0f;

        while (PlayerHealthUI.Instance == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (PlayerHealthUI.Instance != null)
        {
            healthUI = PlayerHealthUI.Instance;

            // ✅ El HUD se encarga de reconectar, solo inicializamos
            healthUI.Initialize(this);

            Debug.Log("✅ [playerLife] Conectado al HUD");
        }
        else
        {
            Debug.LogWarning("⚠️ [playerLife] No se encontró PlayerHealthUI después de timeout");
        }

        // ✅ Cargar datos guardados
        LoadGameData();

        IsInitialized = true;
    }

    private void LoadGameData()
    {
        if (ControladorDatosJuego.Instance == null) return;

        var datos = ControladorDatosJuego.Instance.datosjuego;

        // Cargar vida máxima
        if (datos.vidaMaxima > 0)
        {
            SetMaxHealth(datos.vidaMaxima);
            Debug.Log($"💾 Vida máxima cargada: {datos.vidaMaxima}");
        }

        // Cargar vida actual
        if (datos.vidaActual > 0)
        {
            SetHealth(datos.vidaActual);
            Debug.Log($"💾 Vida actual cargada: {datos.vidaActual}");
        }
        else
        {
            SetHealth(maxHealth);
        }

        // Cargar pociones
        if (datos.maxPotions > 0)
        {
            SetMaxPotions(datos.maxPotions);
            SetPotions(datos.cantidadpociones);
            Debug.Log($"💾 Pociones cargadas: {datos.cantidadpociones}/{datos.maxPotions}");
        }

        // ✅ Actualizar HUD final
        UpdateUI();
    }

    private void Update()
    {
        HandlePotionInput();
    }

    private void HandlePotionInput()
    {
        if (Input.GetKeyDown(usePotionKey))
            TryUsePotion();
    }

    private void TryUsePotion()
    {
        if (!CanUsePotion()) return;

        currentPotions--;
        currentHealth = Mathf.Min(currentHealth + potionHealAmount, maxHealth);
        lastPotionTime = Time.time;

        if (potionCooldownIndicator != null)
        {
            potionCooldownIndicator.StartCooldown(potionCooldown);
        }

        UpdateUI();
    }

    private bool CanUsePotion()
    {
        if (Time.time - lastPotionTime < potionCooldown) return false;
        if (currentPotions <= 0) return false;
        if (currentHealth >= maxHealth) return false;
        return true;
    }

    public void AddPotion(int amount = 1)
    {
        currentPotions = Mathf.Min(currentPotions + amount, maxPotions);
        UpdateUI();
    }

    public void TakeDamage(Vector2 attackerPosition, int damage)
    {
        if (!CanTakeDamage()) return;

        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateUI();

        if (controller != null)
        {
            controller.TakeDamage(attackerPosition);
        }

        if (animController != null)
            animController.TriggerDamage();
        else if (fallbackAnimator != null)
            fallbackAnimator.SetBool("damage", true);

        isTakingDamage = true;

        if (currentHealth <= 0)
            StartCoroutine(HandleDeathSequence());
    }

    private bool CanTakeDamage()
    {
        if (isDead) return false;
        if (currentHealth <= 0) return false;
        if (Time.time - lastDamageTime < invincibilityDuration) return false;
        return true;
    }

    public void StopDamageAnimation()
    {
        if (animController != null)
            animController.StopDamage();
        else if (fallbackAnimator != null)
            fallbackAnimator.SetBool("damage", false);

        isTakingDamage = false;
    }

    private IEnumerator HandleDeathSequence()
    {
        if (isDead) yield break;
        isDead = true;

        DisableAllControls();
        StopDamageAnimation();
        yield return null;

        if (animController != null)
        {
            animController.TriggerDeath();
        }
        else if (fallbackAnimator != null)
        {
            fallbackAnimator.ResetTrigger("DoubleJump");
            fallbackAnimator.ResetTrigger("Throw");
            fallbackAnimator.SetBool("damage", false);
            fallbackAnimator.SetTrigger("Death");

            float clipLength = deathFallbackDuration;
            var rac = fallbackAnimator.runtimeAnimatorController;
            if (rac != null)
            {
                foreach (var clip in rac.animationClips)
                {
                    if (clip.name == deathAnimationName)
                    {
                        clipLength = clip.length;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(Mathf.Max(0.01f, clipLength));
        }
        else
        {
            yield return new WaitForSeconds(deathFallbackDuration);
        }

        OnDeathComplete();
    }

    public void OnDeathAnimationEnd()
    {
        if (!isDead) isDead = true;
        OnDeathComplete();
    }

    private void OnDeathComplete()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusicImmediately();

        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.datosjuego.escenaActual = SceneManager.GetActiveScene().name;
            // No sobreescribir la posición del último checkpoint al morir
            ControladorDatosJuego.Instance.GuardarDatos(false);
            SceneManager.LoadScene("GameOver");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void DisableAllControls()
    {
        if (controller == null) return;

        controller.canMove = false;
        controller.canJump = false;
        controller.canAttack = false;
        controller.canDash = false;
        controller.canWallCling = false;
        controller.canBlock = false;
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void HealFull()
    {
        if (isDead) return;
        currentHealth = maxHealth;
        UpdateUI();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HealthPotion"))
        {
            AddPotion();
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Consumable"))
        {
            Destroy(collision.gameObject);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (healthUI != null)
            healthUI.UpdateDisplay();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateUI();
    }

    public void SetMaxHealth(int max)
    {
        maxHealth = max;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // ✅ Forzar actualización de segmentos
        if (healthUI != null)
        {
            healthUI.ForceRefresh();
        }

        UpdateUI();
    }

    public void SetPotions(int potions)
    {
        currentPotions = Mathf.Clamp(potions, 0, maxPotions);
        UpdateUI();
    }

    public void SetMaxPotions(int max)
    {
        maxPotions = max;
        currentPotions = Mathf.Min(currentPotions, maxPotions);
        UpdateUI();
    }
}
