using UnityEngine;

public class FirstEncounterBossController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 6;
    [Header("Referencias")]
    [SerializeField] private FirstEncounterTrialManager gestorTrials;
    [SerializeField] private FirstEncounterTeleportManager gestorTeleport;
    [SerializeField] private Animator animadorJefe;
    [Header("Música")]
    [SerializeField] private AudioClip musicaFase1;
    [SerializeField] private AudioClip musicaFase2;
    [SerializeField] private AudioClip musicaAcelerada;
    [Header("Modelo")]
    [SerializeField] private GameObject modeloJefe;
    [SerializeField] private BossLife vidaJefe;

    private int currentHealth;
    private bool accelerated;
    private bool active;
    private bool superAcceleratedMusic;

    private void Awake()
    {
        currentHealth = vidaMaxima;
        if (modeloJefe != null) modeloJefe.SetActive(false);
        if (vidaJefe == null)
            vidaJefe = GetComponent<BossLife>();
    }

    public void StartFight()
    {
        active = true;
        accelerated = false;
        PlayMusicPhase1();
        if (gestorTrials != null && vidaJefe != null)
            gestorTrials.BeginSequence(vidaJefe);
    }

    public void OnTrialSuccess(Vector3 bossAppearPosition)
    {
        if (!active) return;
        if (modeloJefe != null)
        {
            modeloJefe.transform.position = bossAppearPosition;
            modeloJefe.SetActive(true);
        }
        if (animadorJefe != null) animadorJefe.SetTrigger("Appear");
    }

    public void ReceiveHit()
    {
        if (!active) return;
        currentHealth = Mathf.Max(0, currentHealth - 1);

        // ✅ NUEVO: Sincronizar con BossLife
        if (vidaJefe != null)
        {
            vidaJefe.health = currentHealth;
        }

        if (animadorJefe != null) animadorJefe.SetTrigger("Hit");
        if (currentHealth <= 0)
        {
            Defeat();
            return;
        }
        if (!accelerated && currentHealth <= vidaMaxima / 2)
        {
            accelerated = true;
            if (gestorTrials != null)
            {
                gestorTrials.PauseForDialoguePhase2();
            }
        }
        if (!superAcceleratedMusic && currentHealth <= vidaMaxima / 4)
        {
            superAcceleratedMusic = true;
            PlayMusicPhase3();
        }
        if (modeloJefe != null) modeloJefe.SetActive(false);

        if (gestorTrials != null)
        {
            gestorTrials.OnBossHit();
        }
        // El avance al siguiente trial lo maneja BossLife en modo trial
    }

    private void Defeat()
    {
        active = false;
        if (animadorJefe != null) animadorJefe.SetTrigger("Death");
        if (modeloJefe != null) modeloJefe.SetActive(false);
        gestorTrials.EndSequenceVictory();
    }

    private void PlayMusicPhase1()
    {
        if (AudioManager.Instance != null && musicaFase1 != null)
        {
            AudioManager.Instance.PlayMusic(musicaFase1, 1f);
        }
    }

    private void PlayMusicPhase2()
    {
        if (AudioManager.Instance != null && musicaFase2 != null)
        {
            AudioManager.Instance.PlayMusic(musicaFase2, 1f);
        }
    }

    private void PlayMusicPhase3()
    {
        if (AudioManager.Instance != null && musicaAcelerada != null)
        {
            AudioManager.Instance.PlayMusic(musicaAcelerada, 1f);
        }
    }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => vidaMaxima;
    public bool IsAccelerated => accelerated;
}
