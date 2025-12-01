using UnityEngine;
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
    [SerializeField] private BossLife bossInScene; // ✅ NUEVO: Referencia directa

    [Header("Opciones")]
    [SerializeField] private float cooldownTiempo = 1f;
    [SerializeField] private GameObject player;

    [Header("Intro del Jefe")]
    [SerializeField] private float introDuracion = 3f;
    [SerializeField] private AudioClip musicaJefe;
    [SerializeField] private AudioClip sfxInicioBatalla;
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private string nombreJefe;
    [SerializeField] private string[] introLines;
    [SerializeField] private TheTrueKnightMusicTimeline musicTimeline;

    [Header("Modo Trial")]
    [SerializeField] private bool useTrialMode = true;
    [SerializeField] private FirstEncounterTrialManager trialManager;
    [SerializeField] private FirstEncounterTeleportManager teleportManager;

    private bool enCooldown = false;
    private bool enPelea = false;
    private BossLife spawnedBoss;

    void Start()
    {
        if (ControladorDatosJuego.Instance != null &&
            ControladorDatosJuego.Instance.datosjuego.jefesDerrotados.Contains(bossID))
        {
            gameObject.SetActive(false);
            return;
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

        // ✅ NUEVO: Decidir entre usar jefe en escena o instanciar uno nuevo
        if (useTrialMode)
        {
            // Modo Trial: Usar el jefe que ya está en la escena
            if (bossInScene != null)
            {
                spawnedBoss = bossInScene;
                Debug.Log("✅ Usando jefe de la escena para Trial Mode");
            }
            else
            {
                Debug.LogError("❌ Trial Mode activado pero no hay jefe asignado en 'Boss In Scene'");
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
                Debug.LogError("❌ BossLife no encontrado en el prefab del jefe!");
                yield break;
            }
        }

        spawnedBoss.SetBossTrigger(this);

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("BossIntro");

        if (BossNameUI.Instance != null)
            BossNameUI.Instance.MostrarNombre(nombreJefe);

        if (introLines != null && introLines.Length > 0 && TextManager.Instance != null)
        {
            for (int i = 0; i < introLines.Length; i++)
            {
                TextManager.Instance.ShowDialogue(introLines[i]);
                yield return new WaitForSeconds(2f);
            }
            TextManager.Instance.CloseDialogue();
        }
        else
        {
            yield return new WaitForSeconds(introDuracion);
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

        // ✅ Iniciar Trial Mode
        if (useTrialMode && trialManager != null)
        {
            spawnedBoss.gameObject.SetActive(true); // Asegurar que esté activo
            spawnedBoss.trialManager = trialManager;
            spawnedBoss.trialMode = true;
            trialManager.BeginSequence(spawnedBoss);
        }
        else
        {
            spawnedBoss.gameObject.SetActive(true);
        }

        if (playerController != null)
            playerController.canMove = true;

        if (playerRb != null)
            playerRb.gravityScale = 1;
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

        if (trialManager != null)
        {
            trialManager.TeleportToInitialArea();
        }

        gameObject.SetActive(false);
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
