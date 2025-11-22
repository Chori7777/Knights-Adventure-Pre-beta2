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
    [SerializeField] private float deathDelay = 1.5f;

    [Header("SCORE - NUEVO")]
    [SerializeField] private int scoreReward = 10; 

    private EnemyCore core;

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

        core.SetDead(true);
        core.SetTakingDamage(false);

        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
            core.rb.angularVelocity = 0f;
            core.rb.bodyType = RigidbodyType2D.Static;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        DisableModules();

        if (AudioManager.Instance != null && Dust != null)
        {
            AudioManager.Instance.PlaySFX(Dust, 0.4f, 1f);
        }

        if (core.animController != null)
        {
            core.animController.SetDamage(false);
            core.animController.SetDeath(true);
        }

        GiveScore();

        StartCoroutine(DeathSequence());
    }

    private void GiveScore()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.AgregarMonedas(scoreReward);

        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
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

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
}