using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossTrigger : MonoBehaviour
{
    [Header("Configuración del Jefe")]
    [SerializeField] private string bossID = "Boss1";
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private BossDoor puertaEntrada;
    [SerializeField] private BossDoor[] puertasArena;

    [Header("Trial Mode - Jefe en Escena")]
    [SerializeField] private BossLife bossInScene; // Nuevo: Referencia directa

    [Header("Opciones")]
    [SerializeField] private float cooldownTiempo = 1f;
    [SerializeField] private GameObject player;

    [Header("Intro del Jefe")]
    [SerializeField] private float introDuracion = 3f;
    [SerializeField] private AudioClip musicaJefe;
    [SerializeField] private AudioClip sfxInicioBatalla;
    [SerializeField] private string[] introLines;
    [SerializeField] private TheTrueKnightMusicTimeline musicTimeline;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private Button skipButton;

    [Header("Modo Trial")]
    [SerializeField] private bool useTrialMode = true;
    [SerializeField] private FirstEncounterTrialManager trialManager;
    [SerializeField] private FirstEncounterTeleportManager teleportManager;
    [SerializeField] private bool forceSingleZoneCombat = true;
    [SerializeField] private bool enableAllAttackScripts = true;
    [SerializeField] private TrueFinalBossZoneManager finalBossZoneManager;
    [SerializeField] private bool disableZones = true;

    [Header("Control de Ataques")]
    [SerializeField] private MonoBehaviour attackScriptToControl;

    private bool enCooldown = false;
    private bool enPelea = false;
    private BossLife spawnedBoss;
    private bool skipRequested = false;

    void Start()
    {
        if (ControladorDatosJuego.Instance != null &&
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Contains(bossID))
        {
            gameObject.SetActive(false);
            return;
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(RequestSkip);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !enCooldown && !enPelea)
        {
            if (puertaEntrada != null)
                puertaEntrada.CerrarPuerta();

            StartCoroutine(IniciarSecuenciaJefe());
            StartCoroutine(ActivarCooldown());
        }
    }

    private IEnumerator IniciarSecuenciaJefe()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var pm = player != null ? player.GetComponent<PlayerMovement>() : null;
        if (pm != null) pm.SetControlsEnabled(false);
        enPelea = true;

        var playerController = player.GetComponent<PlayerMovement>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Animator playerAnim = player.GetComponent<Animator>();

        if (playerController != null)
            playerController.canMove = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.gravityScale = 0;
        }

        if (playerAnim != null)
        {
            playerAnim.SetFloat("Horizontal", 0);
            playerAnim.SetFloat("Vertical", 0);
            playerAnim.SetFloat("Velocidad", 0);
        }

        foreach (BossDoor puerta in puertasArena)
        {
            if (puerta != null)
                puerta.CerrarPuerta();
        }

        // Nuevo: decidir entre jefe en escena o instanciar uno nuevo
        if (useTrialMode)
        {
            // Modo Trial: Usar el jefe que ya está en la escena
            if (bossInScene != null)
            {
                spawnedBoss = bossInScene;
                Debug.Log("Usando jefe de la escena para Trial Mode");
            }
            else
            {
                Debug.LogError("Trial Mode activado pero no hay jefe asignado en 'Boss In Scene'");
                yield break;
            }
        }
        else
        {
            // Modo Normal: Instanciar el prefab
            Vector3 spawnPosition = bossSpawnPoint != null
                ? bossSpawnPoint.position
                : transform.position + new Vector3(2f, 0, 0);

            GameObject bossObj = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            spawnedBoss = bossObj.GetComponent<BossLife>();

            if (spawnedBoss == null)
            {
                Debug.LogError("BossLife no encontrado en el prefab del jefe!");
                yield break;
            }
        }

        spawnedBoss.SetBossTrigger(this);
        if (attackScriptToControl != null)
            spawnedBoss.AssignAttackScript(attackScriptToControl);

        // Activar jefe y HUD inmediatamente y pausar ataques durante los diálogos
        spawnedBoss.gameObject.SetActive(true);
        if (enableAllAttackScripts)
        {
            SetAllBossAttackScriptsEnabled(spawnedBoss, false);
            spawnedBoss.SetAttackEnabled(false);
        }
        else
        {
            SetAllBossAttackScriptsEnabled(spawnedBoss, false);
            spawnedBoss.SetAttackEnabled(false);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();


        if (introLines != null && introLines.Length > 0 && TextManager.Instance != null)
        {
            float totalIntroTime = 0f;
            float typeSpeed = 0.05f;
            if (TextManager.Instance != null)
            {
                typeSpeed = TextManager.Instance.GetTypeSpeed();
            }

            for (int i = 0; i < introLines.Length; i++)
            {
                string line = introLines[i];
                TextManager.Instance.ShowDialogue(line);

                int dotCount = 0;
                int commaCount = 0;
                for (int c = 0; c < line.Length; c++)
                {
                    char ch = line[c];
                    if (ch == '.') dotCount++;
                    else if (ch == ',') commaCount++;
                }

                float baseTime = line.Length * typeSpeed;
                float punctuationTime = dotCount * 0.5f + commaCount * 0.25f;
                float displayTime = baseTime + punctuationTime;
                if (displayTime < 1.0f) displayTime = 1.0f;
                float elapsed = 0f;
                while (elapsed < displayTime)
                {
                    if (skipRequested || Input.GetKeyDown(skipKey)) break;
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                totalIntroTime += displayTime;
                if (skipRequested) break;
            }
            TextManager.Instance.CloseDialogue();
            if (!skipRequested)
            {
                yield return new WaitForSeconds(1f);
                totalIntroTime += 1f;
            }

            if (!skipRequested && totalIntroTime < introDuracion)
            {
                yield return new WaitForSeconds(introDuracion - totalIntroTime);
            }
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < introDuracion)
            {
                if (skipRequested || Input.GetKeyDown(skipKey)) break;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (AudioManager.Instance != null && sfxInicioBatalla != null)
        {
            AudioManager.Instance.PlaySFX(sfxInicioBatalla);
            yield return new WaitForSeconds(0.5f);
        }

        if (AudioManager.Instance != null && musicaJefe != null)
            StartCoroutine(FadeInMusic(musicaJefe, 0.4f));

        if (musicTimeline != null)
        {
            musicTimeline.ResetTimeline();
            musicTimeline.StartTimeline();
        }

        if (finalBossZoneManager != null) finalBossZoneManager.SetZoneChangesDisabled(disableZones);
        if (enableAllAttackScripts)
        {
            SetAllBossAttackScriptsEnabled(spawnedBoss, true);
            spawnedBoss.SetAttackEnabled(true);
        }
        else
        {
            SetAllBossAttackScriptsEnabled(spawnedBoss, false);
            spawnedBoss.SetAttackEnabled(true);
        }
        var bossUI = BossHealthUI.Instance != null 
            ? BossHealthUI.Instance 
            : FindFirstObjectByType<BossHealthUI>(FindObjectsInactive.Include);
        if (bossUI != null)
        {
            if (!bossUI.gameObject.activeSelf)
                bossUI.gameObject.SetActive(true);
            bossUI.ShowForBoss(spawnedBoss);
        }
        else
        {
            Debug.LogError("[BossTrigger] BossHealthUI no encontrado en la escena. Agrega el HUD del jefe al Canvas.");
        }

        // Iniciar Trial Mode
        if (useTrialMode && trialManager != null && !forceSingleZoneCombat)
        {
            spawnedBoss.trialManager = trialManager;
            spawnedBoss.trialMode = true;
            trialManager.BeginSequence(spawnedBoss);
        }
        if (pm != null) pm.SetControlsEnabled(true);

        if (playerController != null)
            playerController.canMove = true;

        if (playerRb != null)
            playerRb.gravityScale = 1;
    }

    public void RequestSkip()
    {
        skipRequested = true;
    }

    private IEnumerator FadeInMusic(AudioClip music, float duration)
    {
        if (AudioManager.Instance == null) yield break;

        AudioManager.Instance.PlayMusic(music);
        AudioSource musicSource = AudioManager.Instance.GetComponent<AudioSource>();
        if (musicSource == null) yield break;

        float elapsedTime = 0f;
        musicSource.volume = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        musicSource.volume = 0.5f;
    }

    public void JefeDerrotado()
    {
        enPelea = false;

        foreach (BossDoor puerta in puertasArena)
        {
            if (puerta != null)
                puerta.AbrirPuerta();
        }

        if (ControladorDatosJuego.Instance != null &&
            !ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Contains(bossID))
        {
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Add(bossID);
            ControladorDatosJuego.Instance.GuardarDatos();
        }

        if (trialManager != null && !forceSingleZoneCombat)
        {
            trialManager.TeleportToInitialArea();
        }

        gameObject.SetActive(false);
    }

    private void SetAllBossAttackScriptsEnabled(BossLife boss, bool enabled)
    {
        if (boss == null) return;
        var go = boss.gameObject;
        var a1 = go.GetComponent<BossScriptAttacks>();
        if (a1 != null) a1.enabled = enabled;
        var a2 = go.GetComponent<TheTrueKnightAttackManager>();
        if (a2 != null) a2.enabled = enabled;
        var a3 = go.GetComponent<TrueFinalBossController>();
        if (a3 != null) a3.enabled = enabled;
    }

    private IEnumerator ActivarCooldown()
    {
        enCooldown = true;
        yield return new WaitForSeconds(cooldownTiempo);
        enCooldown = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 spawnPos = bossSpawnPoint != null
            ? bossSpawnPoint.position
            : transform.position + new Vector3(2f, 0, 0);

        Gizmos.DrawWireSphere(spawnPos, 1f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 3f);
    }
}
