using UnityEngine;
using System.Collections;

public class BossLife : MonoBehaviour
{
    [Header("Identificación")]
    public string bossID = "Boss1";

    [Header("Vida")]
    public int health = 10;
    public int maxHealth = 10;
    private bool isDead = false;

    [Header("Componentes")]
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Checkpoint")]
    [SerializeField] private GameObject savePointPrefab;
    [SerializeField] private Vector3 savePointSpawnPosition;
    [SerializeField] private bool spawnSavePointOnDeath = true;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackRecoveryTime = 0.4f;
    private bool recibiendoDanio = false;

    [Header("Script de ataque del jefe")]
    [SerializeField] private MonoBehaviour scriptAtaque;
    private BossTrigger bossTrigger;
    public FirstEncounterTrialManager trialManager;
    public bool trialMode = false;
    private bool acceleratedApplied = false;
    private bool superAcceleratedApplied = false;
    private bool phase2PulseActivated = false;
    [SerializeField] private bool enablePhase2VignettePulse = false;
    [SerializeField] private Color phase2PulseColor = Color.red;
    [SerializeField] private float phase2PulseMin = 0.2f;
    [SerializeField] private float phase2PulseMax = 0.5f;
    [SerializeField] private float phase2PulsePeriod = 1.5f;
    [Header("Trials - Teleport inicial a mitad de vida")]
    [SerializeField] private bool triggerInitialAreaAtHalfHealth = true;
    private bool initialAreaTeleportDone = false;

    [Header("SCORE - NUEVO")]
    [SerializeField] private int scoreReward = 50; 

    [Header("Drop al morir")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private Vector3 dropSpawnOffset;
    [SerializeField] private bool dropOnDeath = true;


    private void Awake()
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        // Permitir empuje por contacto: no congelar ejes

        if (savePointSpawnPosition == Vector3.zero)
            savePointSpawnPosition = transform.position;

        if (BossHealthUI.Instance != null)
            BossHealthUI.Instance.UpdateHealth(health, maxHealth);
    }

    public void AssignAttackScript(MonoBehaviour attackScript)
    {
        scriptAtaque = attackScript;
    }

    public void SetAttackEnabled(bool enabled)
    {
        if (scriptAtaque != null)
        {
            scriptAtaque.enabled = enabled;
        }
    }

    public void SetBossTrigger(BossTrigger trigger)
    {
        bossTrigger = trigger;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || recibiendoDanio) return;

        health -= damage;
        if (health < 0) health = 0;

        if (BossHealthUI.Instance != null)
            BossHealthUI.Instance.UpdateHealth(health, maxHealth);

        var feather = GetComponent<FeatherBossController>();
        if (feather != null)
        {
            feather.OnBossHit();
        }

        if (anim != null)
            anim.SetBool("damage", true);

        if (health <= 0)
            Die();
        else
        {
            StartCoroutine(RecuperarDeKnockback());
            if (trialMode && trialManager != null)
            {
                trialManager.OnBossHit();
                if (triggerInitialAreaAtHalfHealth && !initialAreaTeleportDone && health <= maxHealth / 2)
                {
                    initialAreaTeleportDone = true;
                    trialManager.TeleportToInitialArea();
                }
            }
            else
            {
                if (enablePhase2VignettePulse && !phase2PulseActivated && health <= maxHealth / 2)
                {
                    phase2PulseActivated = true;
                    var fx = FindFirstObjectByType<CameraEffectsController>(FindObjectsInactive.Include);
                    if (fx != null)
                    {
                        fx.StartVignettePulse(phase2PulseColor, phase2PulseMin, phase2PulseMax, phase2PulsePeriod);
                    }
                }
            }
        }
    }

    public void RecibeDanio(Vector2 direccionAtaque, int cantDanio)
    {
        if (isDead || recibiendoDanio) return;

        health -= cantDanio;
        if (health < 0) health = 0;

        if (BossHealthUI.Instance != null)
            BossHealthUI.Instance.UpdateHealth(health, maxHealth);

        var feather2 = GetComponent<FeatherBossController>();
        if (feather2 != null)
        {
            feather2.OnBossHit();
        }

        recibiendoDanio = true;

        if (anim != null)
            anim.SetBool("damage", true);

        if (health <= 0)
        {
            Die();
            return;
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // liberar X temporalmente
            Vector2 knockDir = ((Vector2)transform.position - direccionAtaque).normalized;
            knockDir.y = Mathf.Clamp(knockDir.y + 0.5f, 0.5f, 1f);
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }

        StartCoroutine(RecuperarDeKnockback());
        if (trialMode && trialManager != null)
        {
            trialManager.OnBossHit();
            if (triggerInitialAreaAtHalfHealth && !initialAreaTeleportDone && health <= maxHealth / 2)
            {
                initialAreaTeleportDone = true;
                trialManager.TeleportToInitialArea();
            }
        }
        else
        {
            if (enablePhase2VignettePulse && !phase2PulseActivated && health <= maxHealth / 2)
            {
                phase2PulseActivated = true;
                var fx = FindFirstObjectByType<CameraEffectsController>(FindObjectsInactive.Include);
                if (fx != null)
                {
                    fx.StartVignettePulse(phase2PulseColor, phase2PulseMin, phase2PulseMax, phase2PulsePeriod);
                }
            }
        }
    }

    private IEnumerator RecuperarDeKnockback()
    {
        yield return new WaitForSeconds(knockbackRecoveryTime);
        recibiendoDanio = false;

        if (anim != null)
            anim.SetBool("damage", false);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        recibiendoDanio = false;

        if (scriptAtaque != null)
        {
            scriptAtaque.StopAllCoroutines();
            scriptAtaque.enabled = false;
        }

        if (anim != null)
        {
            anim.SetBool("damage", false);
            anim.SetBool("Death", true);
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

 
        GiveScore();

        if (trialManager != null)
        {
            trialManager.EndSequenceVictory();
        }

        if (BossHealthUI.Instance != null)
            BossHealthUI.Instance.Hide();

        var fxStop = FindFirstObjectByType<CameraEffectsController>(FindObjectsInactive.Include);
        if (fxStop != null)
            fxStop.StopVignettePulse();

        StartCoroutine(DeathSequence());
    }


    private void GiveScore()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.AgregarMonedas(scoreReward);
            Debug.Log($"👑 ¡JEFE ELIMINADO! Score +{scoreReward}");
        }
    }

    public void StopDmg()
    {
        anim.SetBool("damage", false);
    }

    private IEnumerator DeathSequence()
    {
        if (bossTrigger != null)
            bossTrigger.JefeDerrotado();

        if (spawnSavePointOnDeath && savePointPrefab != null)
        {
            Vector3 spawnPos = (savePointSpawnPosition == Vector3.zero)
                               ? transform.position
                               : savePointSpawnPosition;
            Instantiate(savePointPrefab, spawnPos, Quaternion.identity);
        }

        if (dropOnDeath && dropPrefab != null)
        {
            Vector3 dropPos = transform.position + dropSpawnOffset;
            Instantiate(dropPrefab, dropPos, Quaternion.identity);
        }

        AudioManager.Instance.StopMusicImmediately();
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = (savePointSpawnPosition == Vector3.zero)
                          ? transform.position
                          : savePointSpawnPosition;
        Gizmos.DrawWireSphere(spawnPos, 1f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 2f);
    }
}
