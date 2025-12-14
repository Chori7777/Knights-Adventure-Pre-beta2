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

    [Header("Medallón Ojo")]
    [SerializeField] private bool enableEyeCycle = true;
    [SerializeField] private float eyeOpenDuration = 5f;
    [SerializeField] private float eyeClosedDuration = 5f;
    [SerializeField] private Transform eyeCenter;
    [SerializeField] private GameObject hitCirclePrefab;
    [SerializeField] private float hitCircleFadeDuration = 0.35f;
    [SerializeField] private float hitShakeDuration = 0.15f;
    [SerializeField] private float hitShakeIntensity = 0.08f;
    [SerializeField] private Animator amuletAnimator;
    [SerializeField] private SpriteRenderer collarRenderer;
    [SerializeField] private bool makeCollarBlackOnDamage = true;
    [SerializeField] private float collarBlackDuration = 0.35f;
    [SerializeField] private bool enableCollarDriftBelowHalfHealth = false;
    [SerializeField] private float collarDriftSpeed = 4f;
    [SerializeField] private float collarDriftPadding = 0.5f;
    [SerializeField] private bool bringCollarToFrontOnDrift = true;
    [SerializeField] private string collarSortingLayer = "";
    [SerializeField] private int collarSortingOrder = 9999;
    private bool eyeOpen = true;
    private Coroutine eyeRoutine;
    private bool collarDriftActive = false;
    private Coroutine collarDriftRoutine;

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
    [SerializeField] private bool forceSingleZoneCombat = true;
    
    [Header("Trials - Teleport inicial a mitad de vida")]
    [SerializeField] private bool triggerInitialAreaAtHalfHealth = true;
    private bool initialAreaTeleportDone = false;

    [Header("SCORE - NUEVO")]
    [SerializeField] private int scoreReward = 50; 

    [Header("Drop al morir")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private Vector3 dropSpawnOffset;
    [SerializeField] private bool dropOnDeath = true;

    [Header("Muerte - Diálogo y acciones")]
    [SerializeField] private bool shakeDuringDeath = true;
    [SerializeField] private float deathShakeIntensity = 0.08f;
    [SerializeField] private string deathAnimationStateName = "Death";
    [SerializeField] private string[] deathDialogues;
    [SerializeField] private GameObject[] objectsToDestroyOnDeath;
    [SerializeField] private GameObject[] tilemapsToActivateOnDeath;
    private bool deathAnimationEnded = false;
    [Header("Muerte - Amuleto")]
    [SerializeField] private bool detachAmuletOnDeath = true;
    [SerializeField] private float amuletGravityScale = 1.2f;
    [SerializeField] private float amuletDestroyDelay = 3f;
    [Header("Muerte - Portal de salida")]
    [SerializeField] private bool spawnPortalOnDeath = true;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Vector3 portalSpawnOffset = new Vector3(1.5f, 0f, 0f);
    [SerializeField] private string portalLayerName = "";
    [Header("Muerte - Tiempos")]
    [SerializeField] private float deathAnimMinimumWait = 1.2f;
    [Header("Muerte - Estrella y efectos")]
    [SerializeField] private bool spawnStarOnDeath = false;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private Vector3 starSpawnOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float starSpawnDelay = 2f;
    [SerializeField] private bool applyMapEffectsOnStarSpawn = true;
    private bool starSpawned = false;


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

        if (enableEyeCycle)
        {
            if (eyeRoutine != null) StopCoroutine(eyeRoutine);
            eyeRoutine = StartCoroutine(EyeCycleRoutine());
        }
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
        if (enableEyeCycle && !eyeOpen) return;

        health -= damage;
        if (health < 0) health = 0;

        if (BossHealthUI.Instance != null)
            BossHealthUI.Instance.UpdateHealth(health, maxHealth);

        var feather = GetComponent<FeatherBossController>();
        if (feather != null)
        {
            feather.OnBossHit();
        }
        TryStartCollarDrift();

        if (anim != null)
            anim.SetBool("damage", true);

        if (health <= 0)
            Die();
        else
        {
            StartCoroutine(RecuperarDeKnockback());
            TryPlayHitVibration();
            FlashCollarBlack();
            if (trialMode && trialManager != null && !forceSingleZoneCombat)
            {
                trialManager.OnBossHit();
                if (triggerInitialAreaAtHalfHealth && !initialAreaTeleportDone && health <= maxHealth / 2)
                {
                    initialAreaTeleportDone = true;
                    trialManager.PauseForDialoguePhase2();
                }
            }
            else
            {
            }
            SpawnHitCircle();
        }
    }

    public void RecibeDanio(Vector2 direccionAtaque, int cantDanio)
    {
        if (isDead || recibiendoDanio) return;
        if (enableEyeCycle && !eyeOpen) return;

        health -= cantDanio;
        if (health < 0) health = 0;

        if (BossHealthUI.Instance != null)
            BossHealthUI.Instance.UpdateHealth(health, maxHealth);

        var feather2 = GetComponent<FeatherBossController>();
        if (feather2 != null)
        {
            feather2.OnBossHit();
        }
        TryStartCollarDrift();

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
        TryPlayHitVibration();
        SpawnHitCircle();
        FlashCollarBlack();
        if (trialMode && trialManager != null && !forceSingleZoneCombat)
        {
            trialManager.OnBossHit();
            if (triggerInitialAreaAtHalfHealth && !initialAreaTeleportDone && health <= maxHealth / 2)
            {
                initialAreaTeleportDone = true;
                trialManager.PauseForDialoguePhase2();
            }
        }
        else
        {
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
        StopCollarDrift();
    }


    private void GiveScore()
    {
        if (ControladorDatosJuego.Instance != null)
        {
            ControladorDatosJuego.Instance.AgregarMonedas(scoreReward);
            Debug.Log($"[BossLife] Jefe eliminado. Score +{scoreReward}");
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

        AudioManager.Instance.StopMusic(true);

        if (shakeDuringDeath)
        {
            StartCoroutine(ShakeDuringDeathAnimation());
        }

        if (detachAmuletOnDeath)
        {
            DetachAmuletAndDrop();
        }

        if (deathDialogues != null && deathDialogues.Length > 0 && TextManager.Instance != null)
        {
            yield return TextManager.Instance.PlaySequenceAndWait(deathDialogues);
        }

        yield return WaitForDeathAnimationEndOrTimeout(5f);

        if (objectsToDestroyOnDeath != null)
        {
            for (int i = 0; i < objectsToDestroyOnDeath.Length; i++)
            {
                var obj = objectsToDestroyOnDeath[i];
                if (obj != null) Destroy(obj);
            }
        }
        if (tilemapsToActivateOnDeath != null)
        {
            for (int i = 0; i < tilemapsToActivateOnDeath.Length; i++)
            {
                var tm = tilemapsToActivateOnDeath[i];
                if (tm != null) tm.SetActive(true);
            }
        }
        if (spawnPortalOnDeath)
        {
            SpawnExitPortal();
        }
        if (spawnStarOnDeath)
        {
            yield return StartCoroutine(StarSpawnTimer(starSpawnDelay));
        }
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    public void OnDeathAnimationEnd()
    {
        deathAnimationEnded = true;
    }

    private IEnumerator ShakeDuringDeathAnimation()
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        var t = cam.transform;
        Vector3 origin = t.position;
        while (!deathAnimationEnded && IsDeathAnimationPlaying())
        {
            float x = Random.Range(-1f, 1f) * deathShakeIntensity;
            float y = Random.Range(-1f, 1f) * deathShakeIntensity;
            t.position = new Vector3(origin.x + x, origin.y + y, origin.z);
            yield return null;
        }
        t.position = origin;
    }

    private void DetachAmuletAndDrop()
    {
        GameObject target = null;
        if (amuletAnimator != null) target = amuletAnimator.gameObject;
        else if (collarRenderer != null) target = collarRenderer.gameObject;
        if (target == null) return;
        target.transform.SetParent(null, true);
        var rb2 = target.GetComponent<Rigidbody2D>();
        if (rb2 == null) rb2 = target.AddComponent<Rigidbody2D>();
        rb2.gravityScale = Mathf.Max(0.01f, amuletGravityScale);
        rb2.constraints = RigidbodyConstraints2D.None;
        var col2 = target.GetComponent<Collider2D>();
        if (col2 == null) target.AddComponent<BoxCollider2D>();
        if (amuletDestroyDelay > 0f) Destroy(target, amuletDestroyDelay);
    }

    private bool IsDeathAnimationPlaying()
    {
        if (anim == null) return false;
        if (string.IsNullOrEmpty(deathAnimationStateName)) return !deathAnimationEnded;
        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(deathAnimationStateName))
        {
            return info.normalizedTime < 1f;
        }
        return !deathAnimationEnded;
    }

    private IEnumerator WaitForDeathAnimationEndOrTimeout(float timeout)
    {
        float minElapsed = 0f;
        while (minElapsed < Mathf.Max(0f, deathAnimMinimumWait))
        {
            minElapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        float elapsed = 0f;
        while (elapsed < timeout && !deathAnimationEnded && IsDeathAnimationPlaying())
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void SpawnExitPortal()
    {
        GameObject portalGO = null;
        if (portalPrefab != null)
        {
            portalGO = Instantiate(portalPrefab);
        }
        else
        {
            portalGO = new GameObject("ExitPortal_Auto");
        }
        portalGO.transform.position = transform.position + portalSpawnOffset;
        if (!string.IsNullOrEmpty(portalLayerName))
        {
            int layer = LayerMask.NameToLayer(portalLayerName);
            if (layer >= 0) portalGO.layer = layer;
        }
        var portal = portalGO.GetComponent<InteractableScenePortal>();
        if (portal == null) portal = portalGO.AddComponent<InteractableScenePortal>();
        portal.enlargeOnInteract = true;
        portal.changeLayerOnInteract = false;
        portal.useFade = true;
        portal.showWindowsDialogOnInteract = true;
        portal.goToLastSceneInBuild = true;
    }

    public void TriggerStarSpawnNow()
    {
        if (starSpawned) return;
        if (applyMapEffectsOnStarSpawn)
        {
            var vfx = FindFirstObjectByType<TrueFinalBossVisualEffects>(FindObjectsInactive.Include);
            if (vfx != null)
            {
                vfx.FadeToBlack(0.4f);
                vfx.ShakeCamera(0.6f, 0.12f);
                vfx.ResetEffects(0.8f);
            }
        }
        if (starPrefab != null)
        {
            Vector3 pos = transform.position + starSpawnOffset;
            Instantiate(starPrefab, pos, Quaternion.identity);
        }
        starSpawned = true;
    }

    private IEnumerator StarSpawnTimer(float delay)
    {
        if (starSpawned) yield break;
        float d = Mathf.Max(0f, delay);
        float elapsed = 0f;
        while (elapsed < d)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        TriggerStarSpawnNow();
        yield return null;
    }

    private IEnumerator EyeCycleRoutine()
    {
        while (!isDead)
        {
            eyeOpen = true;
            if (amuletAnimator != null)
            {
                amuletAnimator.SetBool("EyeOpen", true);
                amuletAnimator.SetBool("EyeClosed", false);
            }
            yield return new WaitForSeconds(Mathf.Max(0.01f, eyeOpenDuration));

            eyeOpen = false;
            if (amuletAnimator != null)
            {
                amuletAnimator.SetBool("EyeOpen", false);
                amuletAnimator.SetBool("EyeClosed", true);
            }
            yield return new WaitForSeconds(Mathf.Max(0.01f, eyeClosedDuration));
        }
    }

    private void SpawnHitCircle()
    {
        if (hitCirclePrefab == null) return;
        Transform center = eyeCenter != null ? eyeCenter : transform;
        GameObject circle = Instantiate(hitCirclePrefab, center.position, Quaternion.identity);
        var sr = circle.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color; c.a = 0f; sr.color = c;
            DG.Tweening.DOTween.To(() => sr.color.a, a => { var cc = sr.color; cc.a = a; sr.color = cc; }, 1f, hitCircleFadeDuration);
        }
        Destroy(circle, Mathf.Max(hitCircleFadeDuration + 0.05f, 0.1f));
    }

    private void TryPlayHitVibration()
    {
        var vfx = FindFirstObjectByType<TrueFinalBossVisualEffects>(FindObjectsInactive.Include);
        if (vfx != null)
        {
            vfx.ShakeCamera(hitShakeDuration, hitShakeIntensity);
            return;
        }
        StartCoroutine(LocalCameraShake(hitShakeDuration, hitShakeIntensity));
    }

    private IEnumerator LocalCameraShake(float duration, float intensity)
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        var t = cam.transform;
        Vector3 origin = t.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            t.position = new Vector3(origin.x + x, origin.y + y, origin.z);
            yield return null;
        }
        t.position = origin;
    }

    private void FlashCollarBlack()
    {
        if (!makeCollarBlackOnDamage) return;
        if (collarRenderer == null) return;
        Color original = collarRenderer.color;
        collarRenderer.color = Color.black;
        DG.Tweening.DOTween.To(() => collarRenderer.color, c => collarRenderer.color = c, original, Mathf.Max(0.01f, collarBlackDuration));
    }
    private void TryStartCollarDrift()
    {
        if (!enableCollarDriftBelowHalfHealth) return;
        if (collarDriftActive) return;
        if (collarRenderer == null) return;
        if (health > maxHealth / 2) return;
        if (bringCollarToFrontOnDrift)
        {
            if (!string.IsNullOrEmpty(collarSortingLayer)) collarRenderer.sortingLayerName = collarSortingLayer;
            collarRenderer.sortingOrder = collarSortingOrder;
        }
        if (collarDriftRoutine != null) StopCoroutine(collarDriftRoutine);
        collarDriftRoutine = StartCoroutine(CollarDriftRoutine());
        collarDriftActive = true;
    }
    private void StopCollarDrift()
    {
        if (collarDriftRoutine != null)
        {
            StopCoroutine(collarDriftRoutine);
            collarDriftRoutine = null;
        }
        collarDriftActive = false;
    }
    private IEnumerator CollarDriftRoutine()
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        float pad = Mathf.Max(0f, collarDriftPadding);
        while (!isDead)
        {
            Vector3 c = cam.transform.position;
            float h = cam.orthographicSize;
            float w = h * cam.aspect;
            float minX = c.x - w + pad;
            float maxX = c.x + w - pad;
            float minY = c.y - h + pad;
            float maxY = c.y + h - pad;
            Vector3 target = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), collarRenderer.transform.position.z);
            while (Vector3.Distance(collarRenderer.transform.position, target) > 0.05f && !isDead)
            {
                collarRenderer.transform.position = Vector3.MoveTowards(collarRenderer.transform.position, target, Mathf.Max(0.01f, collarDriftSpeed) * Time.deltaTime);
                yield return null;
            }
            yield return null;
        }
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
