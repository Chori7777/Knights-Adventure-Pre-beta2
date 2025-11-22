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

    [Header("Opciones")]
    [SerializeField] private float cooldownTiempo = 1f;
    [SerializeField] private GameObject player;

    [Header("Intro del Jefe")]
    [SerializeField] private float introDuracion = 3f;
    [SerializeField] private AudioClip musicaJefe;
    [SerializeField] private AudioClip sfxInicioBatalla;
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private string nombreJefe;

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

        Vector3 spawnPosition = bossSpawnPoint != null
            ? bossSpawnPoint.position
            : transform.position + new Vector3(2f, 0, 0);

        GameObject bossObj = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        spawnedBoss = bossObj.GetComponent<BossLife>();

        if (spawnedBoss != null)
            spawnedBoss.SetBossTrigger(this);
        else
            Debug.LogError("❌ BossLife no encontrado en el prefab del jefe!");

        bossObj.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("BossIntro");

        if (BossNameUI.Instance != null)
            BossNameUI.Instance.MostrarNombre(nombreJefe);

        yield return new WaitForSeconds(introDuracion);

        bossObj.SetActive(true);

        if (AudioManager.Instance != null && sfxInicioBatalla != null)
        {
            AudioManager.Instance.PlaySFX(sfxInicioBatalla);
            yield return new WaitForSeconds(0.5f);
        }

        if (AudioManager.Instance != null && musicaJefe != null)
            StartCoroutine(FadeInMusic(musicaJefe, 0.4f));

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
