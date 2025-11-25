using UnityEngine;
using System.Collections;

public class EnemyLife : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField] private AudioClip Dust;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackRecoveryTime = 0.4f;

    [Header("Muerte")]
    [SerializeField] private float deathFallbackDelay = 1.5f; // Solo se usa si falla el Animation Event

    [Header("SCORE")]
    [SerializeField] private int scoreReward = 10;

    private EnemyCore core;
    private bool isDeathSequenceStarted = false;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (core.IsDead || core.IsTakingDamage) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (core.animController != null)
        {
            core.animController.SetDamage(true);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            core.SetTakingDamage(true);
            CancelCurrentAttack();
            StartCoroutine(DamageRecovery());
        }
    }

    public void TakeDamageWithKnockback(Vector2 attackPosition, int damage)
    {
        if (core.IsDead || core.IsTakingDamage) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        core.SetTakingDamage(true);

        if (core.animController != null)
        {
            core.animController.SetDamage(true);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ApplyKnockback(attackPosition);
            CancelCurrentAttack();
            StartCoroutine(DamageRecovery());
        }
    }

    private void ApplyKnockback(Vector2 attackPosition)
    {
        if (core.rb == null) return;

        Vector2 knockbackDir = ((Vector2)transform.position - attackPosition).normalized;
        knockbackDir.y = Mathf.Clamp(knockbackDir.y + 0.5f, 0.5f, 1f);

        core.rb.linearVelocity = Vector2.zero;
        core.rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator DamageRecovery()
    {
        yield return new WaitForSeconds(knockbackRecoveryTime);

        core.SetTakingDamage(false);

        if (core.animController != null)
        {
            core.animController.SetDamage(false);
        }

        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        }
    }

    private void CancelCurrentAttack()
    {
        if (core.meleeAttack != null)
        {
            core.meleeAttack.CancelAttack();
        }
    }

    private void Die()
    {
        if (core.IsDead) return;

        Debug.Log($"💀 [{gameObject.name}] Iniciando secuencia de muerte");

        core.SetDead(true);
        core.SetTakingDamage(false);
        isDeathSequenceStarted = true;

        // ✅ ARREGLADO: Detener física SIN poner en Static (esto congela animaciones)
        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
            core.rb.angularVelocity = 0f;
            core.rb.gravityScale = 0f; // Desactivar gravedad
            core.rb.constraints = RigidbodyConstraints2D.FreezeAll; // Congelar posición y rotación
            // NO usar: core.rb.bodyType = RigidbodyType2D.Static; ← Esto congela animaciones
        }

        // Desactivar colisión
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Desactivar módulos de IA
        DisableModules();

        // Sonido de muerte
        if (AudioManager.Instance != null && Dust != null)
        {
            AudioManager.Instance.PlaySFX(Dust, 0.4f, 1f);
        }

        // Dar puntos
        GiveScore();

        // ✅ Activar animación de muerte ANTES de verificar el Animator
        if (core.animController != null)
        {
            core.animController.SetDamage(false);
            core.animController.SetDeath(true);
            Debug.Log($"🎬 [{gameObject.name}] Animación de muerte activada");

            // ✅ DEBUG: Verificar estado del Animator
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                Debug.Log($"🎬 Animator enabled: {anim.enabled}");
                Debug.Log($"🎬 Animator speed: {anim.speed}");
            }
        }

        // ✅ Fallback: Si el Animation Event no se llama, destruir después del delay
        StartCoroutine(DeathFallback());
    }

    // ✅ MÉTODO LLAMADO POR ANIMATION EVENT
    // Añade este método como evento al FINAL de tu animación de muerte
    public void OnDeathAnimationEnd()
    {
        if (!isDeathSequenceStarted)
        {
            Debug.LogWarning($"⚠️ [{gameObject.name}] OnDeathAnimationEnd llamado pero no hay secuencia activa");
            return;
        }

        Debug.Log($"✅ [{gameObject.name}] Animation Event: OnDeathAnimationEnd llamado");
        DestroyEnemy();
    }

    // ✅ Fallback por si el Animation Event falla
    private IEnumerator DeathFallback()
    {
        yield return new WaitForSeconds(deathFallbackDelay);

        // Solo destruir si todavía existe (el Animation Event podría haberlo destruido ya)
        if (gameObject != null && isDeathSequenceStarted)
        {
            Debug.LogWarning($"⚠️ [{gameObject.name}] Usando fallback - Animation Event no se llamó");
            DestroyEnemy();
        }
    }

    // ✅ Método centralizado para destruir
    private void DestroyEnemy()
    {
        Debug.Log($"💥 [{gameObject.name}] Destruyendo enemigo");
        Destroy(gameObject);
    }

    private void GiveScore()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.AgregarMonedas(scoreReward);
            Debug.Log($"💰 [{gameObject.name}] +{scoreReward} monedas");
        }
    }

    private void DisableModules()
    {
        if (core.movement != null) core.movement.enabled = false;
        if (core.meleeAttack != null) core.meleeAttack.enabled = false;
        if (core.rangedAttack != null) core.rangedAttack.enabled = false;
        if (core.flying != null) core.flying.enabled = false;

        EnemySmartMovement smartMove = GetComponent<EnemySmartMovement>();
        if (smartMove != null) smartMove.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Espada") && !core.IsDead)
        {
            Vector2 attackPosition = new Vector2(collision.transform.position.x, transform.position.y);
            TakeDamageWithKnockback(attackPosition, 1);
        }
    }

    // ✅ Propiedades públicas
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
}