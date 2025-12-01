using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static FirstEncounterTrialManager;

public class FirstEncounterTrialManager : MonoBehaviour
{
    [System.Serializable]
    public class Trial
    {
        public Transform playerSpawn;
        public Transform bossSpawn;
        public Transform cameraTarget;
    }

    [Header("Trials")]
    [SerializeField] private Trial[] phase1Trials;
    [SerializeField] private Trial[] phase2Trials;
    [Header("Tiempo por Fase")]
    [SerializeField] private float tiempoFase1 = 15f;
    [SerializeField] private float tiempoFase2 = 8f;
    [Header("Teleport")]
    [SerializeField] private FirstEncounterTeleportManager gestorTeleport;
    [Header("Boss Controller")]
    [SerializeField] private FirstEncounterBossController bossController;
    [Header("Ataques durante Trials")]
    [SerializeField] private FirstEncounterBossAttackManager attackManager;
    [Header("Diálogo Fase 2")]
    [SerializeField] private TextManager gestorTexto;
    [SerializeField] private string dialogoFase2;
    [SerializeField] private string[] dialogosFase2;

    [Header("Diálogo Final - Derrota")]
    [SerializeField] private string dialogoFinalDerrota;
    [SerializeField] private string[] dialogosFinalDerrota;
    [Header("HUD")]
    [SerializeField] private TMPro.TextMeshProUGUI hudTimerText;
    [Header("Área Inicial del Combate")]
    [SerializeField] private Transform areaInicialPlayerSpawn;
    [SerializeField] private Transform areaInicialCameraTarget;
    [Header("Música Fase 2")]
    [SerializeField] private AudioClip musicaFase2;
    [SerializeField] private float musicaFase2Volume = 0.5f;

    [Header("Pantalla Negra")]
    [SerializeField] private float blackScreenDuration = 1.5f;
    [Header("Fase 2 - Reducción de tiempo por golpe")]
    [SerializeField] private float hitTimeReduction = 5f;
    [SerializeField] private float directFightThreshold = 3f;

    private int index;
    private float timer;
    private bool running;
    private bool accelerated;
    private bool superAccelerated;
    private BossLife boss;
    private Coroutine timerRoutine;
    private bool phaseTransitionActive;
    private Trial[] currentTrials;
    private bool sequenceActive;
    private bool directFightActive;

    private void Awake()
    {
        if (gestorTexto == null)
        {
            gestorTexto = TextManager.Instance != null ? TextManager.Instance : FindFirstObjectByType<TextManager>();
        }
    }

    public void BeginSequence(BossLife bossLife)
    {
        boss = bossLife;
        if (boss != null)
        {
            boss.trialManager = this;
            boss.trialMode = true;
        }
        accelerated = false;
        currentTrials = phase1Trials;
        index = 0;
        sequenceActive = true;
        StartTrial(index);
    }

    public void SetAccelerated(bool value)
    {
        accelerated = value;
        if (accelerated)
        {
            currentTrials = (phase2Trials != null && phase2Trials.Length > 0) ? phase2Trials : phase1Trials;
            index = 0;
        }
        else
        {
            currentTrials = (phase1Trials != null && phase1Trials.Length > 0) ? phase1Trials : phase2Trials;
            index = 0;
        }
    }

    private float GetTime()
    {
        float t = accelerated ? tiempoFase2 : tiempoFase1;
        if (superAccelerated) t = Mathf.Max(1f, tiempoFase2 * 0.75f);
        return Mathf.Max(1f, t);
    }

    private void StartTrial(int i)
    {
        if (phaseTransitionActive) return;
        if (!sequenceActive) return;
        if (directFightActive) return;
        if (currentTrials == null || currentTrials.Length == 0)
        {
            EndSequenceVictory();
            return;
        }
        if (i < 0 || i >= currentTrials.Length)
        {
            EndSequenceVictory();
            return;
        }

        Trial t = currentTrials[i];
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = true;
        timer = GetTime();

        // ✅ Verificar que gestorTeleport no sea null
        if (gestorTeleport == null)
        {
            Debug.LogError("❌ gestorTeleport es NULL en FirstEncounterTrialManager!");
            return;
        }

        // ✅ ASEGURARSE que el jefe esté ACTIVO antes de teletransportar
        if (boss != null && !boss.gameObject.activeSelf)
        {
            boss.gameObject.SetActive(true);
            Debug.Log("✅ Jefe activado para trial");
        }

        StartCoroutine(TeleportWithBlackScreen(t.playerSpawn, t.cameraTarget));

        if (boss != null && t.bossSpawn != null)
        {
            boss.transform.position = t.bossSpawn.position;
            if (bossController != null)
            {
                bossController.OnTrialSuccess(t.bossSpawn.position);
            }
            Debug.Log($"✅ Jefe teletransportado a: {t.bossSpawn.position}");
        }

        timerRoutine = StartCoroutine(TimerRoutine());

        if (attackManager != null)
        {
            attackManager.StartTrialAttacks(accelerated);
        }
    }

    private IEnumerator TeleportWithBlackScreen(Transform playerSpawn, Transform cameraTarget)
    {
        // Mostrar pantalla negra
        if (BlackScreenManager.Instance != null)
        {
            BlackScreenManager.Instance.ShowBlackScreen();
        }
        
        // Esperar un frame para asegurar que la pantalla negra se muestra
        yield return null;
        
        // Realizar el teleport
        gestorTeleport.TeleportRaw(playerSpawn, cameraTarget);
        
        // Esperar la duración de la pantalla negra
        yield return new WaitForSeconds(blackScreenDuration);
        
        // Ocultar pantalla negra
        if (BlackScreenManager.Instance != null)
        {
            BlackScreenManager.Instance.HideBlackScreen();
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (running)
        {
            timer -= Time.deltaTime;
            if (hudTimerText != null)
            {
                hudTimerText.text = Mathf.CeilToInt(timer).ToString();
            }
            if (timer <= 0f)
            {
                TrialFailed();
                yield break;
            }
            yield return null;
        }
    }

    public void TrialCompleted()
    {
        if (!running || !sequenceActive) return;
        running = false;
        if (currentTrials == null || currentTrials.Length == 0) return;
        Trial t = currentTrials[index];

        if (boss != null)
        {
            boss.transform.position = t.bossSpawn.position;

            // ✅ ASEGURARSE que esté activo
            if (!boss.gameObject.activeSelf)
            {
                boss.gameObject.SetActive(true);
            }

            Debug.Log($"✅ Jefe aparece en: {t.bossSpawn.position}");
        }
    }

    public void TrialFailed()
    {
        if (phaseTransitionActive || !sequenceActive) return;
        if (directFightActive) return;
        running = false;
        NextTrial();
    }

    public void NextTrial()
    {
        if (phaseTransitionActive || !sequenceActive) return;
        if (directFightActive) return;
        if (currentTrials == null || currentTrials.Length == 0)
        {
            EndSequenceVictory();
            return;
        }
        index = (index + 1) % currentTrials.Length;
        StartTrial(index);
    }

    public void EndSequenceVictory()
    {
        sequenceActive = false;
        running = false;
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        if (attackManager != null)
        {
            attackManager.StopAttacks();
        }
        
        StartCoroutine(ShowFinalDefeatDialogue());
    }

    private IEnumerator ShowFinalDefeatDialogue()
    {
        // Teleport al área inicial para el diálogo final
        if (gestorTeleport != null && areaInicialPlayerSpawn != null)
        {
            gestorTeleport.TeleportRaw(areaInicialPlayerSpawn, areaInicialCameraTarget);
            yield return new WaitForSeconds(0.5f);
        }
        
        // Mostrar diálogo final
        if (gestorTexto != null)
        {
            if (dialogosFinalDerrota != null && dialogosFinalDerrota.Length > 0)
            {
                for (int i = 0; i < dialogosFinalDerrota.Length; i++)
                {
                    gestorTexto.ShowDialogue(dialogosFinalDerrota[i]);
                    yield return new WaitForSeconds(3f);
                }
                gestorTexto.CloseDialogue();
            }
            else if (!string.IsNullOrEmpty(dialogoFinalDerrota))
            {
                gestorTexto.ShowDialogue(dialogoFinalDerrota);
                yield return new WaitForSeconds(4f);
                gestorTexto.CloseDialogue();
            }
        }
        
        // Permitir que el jugador continúe después del diálogo
        Debug.Log("Combate finalizado. El jugador puede continuar.");
    }

    public void PauseForDialoguePhase2()
    {
        StartCoroutine(Phase2PauseRoutine());
    }

    private IEnumerator Phase2PauseRoutine()
    {
        phaseTransitionActive = true;
        if (attackManager != null)
        {
            attackManager.StopAttacks();
        }
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = false;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(true); // Fade out suave
            yield return new WaitForSeconds(0.5f); // Esperar un poco para el fade out
        }
        
        if (gestorTeleport != null && areaInicialPlayerSpawn != null)
        {
            gestorTeleport.TeleportRaw(areaInicialPlayerSpawn, areaInicialCameraTarget);
            yield return new WaitForSeconds(0.2f);
        }
        if (gestorTexto != null)
        {
            if (dialogosFase2 != null && dialogosFase2.Length > 0)
            {
                for (int i = 0; i < dialogosFase2.Length; i++)
                {
                    gestorTexto.ShowDialogue(dialogosFase2[i]);
                    yield return new WaitForSeconds(2f);
                }
                gestorTexto.CloseDialogue();
            }
            else if (!string.IsNullOrEmpty(dialogoFase2))
            {
                gestorTexto.ShowDialogue(dialogoFase2);
                yield return new WaitForSeconds(3f);
                gestorTexto.CloseDialogue();
            }
        }
        yield return new WaitForSeconds(1.5f);

        SetAccelerated(true);

        if (AudioManager.Instance != null && musicaFase2 != null)
        {
            AudioManager.Instance.PlayMusic(musicaFase2, musicaFase2Volume, true, true);
        }
        phaseTransitionActive = false;
        StartTrial(index);
    }

    public void TransitionToPhase2Immediate()
    {
        accelerated = true;
        currentTrials = (phase2Trials != null && phase2Trials.Length > 0) ? phase2Trials : phase1Trials;
        index = 0;
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = false;
        phaseTransitionActive = false;
        StartTrial(index);
    }

    public void TeleportToInitialArea()
    {
        if (gestorTeleport != null && areaInicialPlayerSpawn != null)
        {
            gestorTeleport.TeleportInstant(areaInicialPlayerSpawn, areaInicialCameraTarget, 1.2f);
        }
    }

    public void SetSuperAccelerated(bool value)
    {
        superAccelerated = value;
    }

    // ✅ Llamado por BossLife cuando el jefe recibe un golpe en fase 2 de trials
    public void OnBossHit()
    {
        if (!sequenceActive) return;
        if (directFightActive) return;
        timer = Mathf.Max(0f, timer - hitTimeReduction);
        if (hudTimerText != null)
        {
            hudTimerText.text = Mathf.CeilToInt(timer).ToString();
        }
        if (accelerated && timer <= directFightThreshold)
        {
            directFightActive = true;
            sequenceActive = false;
            running = false;
            if (timerRoutine != null)
            {
                StopCoroutine(timerRoutine);
                timerRoutine = null;
            }
            if (attackManager != null)
            {
                attackManager.StopAttacks();
            }
            if (gestorTeleport != null && areaInicialPlayerSpawn != null)
            {
                StartCoroutine(TeleportToInitialAndResume());
            }
        }
    }

    private IEnumerator TeleportToInitialAndResume()
    {
        yield return TeleportWithBlackScreen(areaInicialPlayerSpawn, areaInicialCameraTarget);
        directFightActive = false;
        sequenceActive = true;
        accelerated = true;
        currentTrials = (phase2Trials != null && phase2Trials.Length > 0) ? phase2Trials : phase1Trials;
        index = 0;
        StartTrial(index);
    }

    public float RemainingTime => timer;
}
