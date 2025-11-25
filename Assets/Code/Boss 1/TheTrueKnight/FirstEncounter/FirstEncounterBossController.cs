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
    [Header("Modelo")]
    [SerializeField] private GameObject modeloJefe;
    [SerializeField] private BossLife vidaJefe;

    private int currentHealth;
    private bool accelerated;
    private bool active;

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
        if (animadorJefe != null) animadorJefe.SetTrigger("Hit");
        if (currentHealth <= 0)
        {
            Defeat();
            return;
        }
        if (!accelerated && currentHealth <= vidaMaxima / 2)
        {
            accelerated = true;
            gestorTrials.SetAccelerated(true);
            PlayMusicPhase2();
            gestorTrials.PauseForDialoguePhase2();
        }
        if (modeloJefe != null) modeloJefe.SetActive(false);
        gestorTrials.NextTrial();
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

    public int CurrentHealth => currentHealth;
    public int MaxHealth => vidaMaxima;
    public bool IsAccelerated => accelerated;
}
