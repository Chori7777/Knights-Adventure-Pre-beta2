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

        public GameObject[] enableOnEnterObjects;
        public MonoBehaviour[] enableOnEnterComponents;
        public GameObject[] disableOnExitObjects;
        public MonoBehaviour[] disableOnExitComponents;
    }

    [Header("Trials")]
    [SerializeField] private Trial[] phase1Trials;
    [SerializeField] private Trial[] phase2Trials;
    [Header("Tiempo por Fase")]
    [SerializeField] private float tiempoFase1 = 15f;
    
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
    [SerializeField] private Transform areaInicialBossSpawn;
    [Header("Música Fase 2")]
    [SerializeField] private AudioClip musicaFase2;
    [SerializeField] private float musicaFase2Volume = 0.5f;

    [Header("Pantalla Negra")]
    [SerializeField] private float blackScreenDuration = 1.5f;
    [Header("Fase 2 - Reducción de tiempo por golpe")]
    
    [Header("Transición Fase 2")]
    [SerializeField] private float phase2DialogueExtraDelay = 8f;
    [SerializeField] private bool enableDebugLogs = true;
    

    private int index;
    private float timer;
    private bool running;
    
    private bool superAccelerated;
    private bool phase2Active;
    private bool ignoreHitAdvance;
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
        if (enableDebugLogs) Debug.Log("[TrialManager] BeginSequence");
        boss = bossLife;
        if (boss != null)
        {
            boss.trialManager = this;
            boss.trialMode = true;
        }
        phase2Active = false;
        currentTrials = phase1Trials;
        index = 0;
        sequenceActive = true;
        if (enableDebugLogs) Debug.Log("[TrialManager] Starting trial index=" + index);
        StartTrial(index);
    }

    public void SetAccelerated(bool value)
    {
        // Desactivado. No cambia lista de trials.
    }

    private float GetTime()
    {
        float t = tiempoFase1;
        return Mathf.Max(1f, t);
    }

    private void StartTrial(int trialIndex)
    {
        if (enableDebugLogs) Debug.Log("[TrialManager] StartTrial trialIndex=" + trialIndex);
        if (phaseTransitionActive) return;
        if (!sequenceActive) return;
        if (directFightActive) return;
        if (currentTrials == null || currentTrials.Length == 0)
        {
            EndSequenceVictory();
            return;
        }
        if (trialIndex < 0 || trialIndex >= currentTrials.Length)
        {
            EndSequenceVictory();
            return;
        }

        Trial trial = currentTrials[trialIndex];
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = true;
        timer = GetTime();

        // Verificar que gestorTeleport no sea null
        if (gestorTeleport == null)
        {
            Debug.LogError("[TrialManager] gestorTeleport es NULL");
            return;
        }

        // Asegurar que el jefe este activo antes de teletransportar
        if (boss != null && !boss.gameObject.activeSelf)
        {
            boss.gameObject.SetActive(true);
            Debug.Log("[TrialManager] Jefe activado para trial");
        }

        StartCoroutine(TeleportWithBlackScreen(trial.playerSpawn, trial.cameraTarget));

        if (boss != null && trial.bossSpawn != null)
        {
            boss.transform.position = trial.bossSpawn.position;
            if (bossController != null)
            {
                bossController.OnTrialSuccess(trial.bossSpawn.position);
            }
            Debug.Log($"[TrialManager] Jefe teletransportado a: {trial.bossSpawn.position}");
        }

        EnableTrialBehaviours(trial);
        timerRoutine = StartCoroutine(TimerRoutine());

        if (attackManager == null)
            attackManager = FindFirstObjectByType<FirstEncounterBossAttackManager>(FindObjectsInactive.Include);
        if (attackManager != null)
        {
            if (!attackManager.enabled) attackManager.enabled = true;
            attackManager.StartTrialAttacks(phase2Active);
            if (enableDebugLogs) Debug.Log("[TrialManager] AttackManager StartTrialAttacks phase2=" + phase2Active);
        }
    }

    private IEnumerator TeleportWithBlackScreen(Transform playerSpawn, Transform cameraTarget)
    {
        if (enableDebugLogs) Debug.Log("[TrialManager] TeleportWithBlackScreen enter");
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
        if (enableDebugLogs) Debug.Log("[TrialManager] TeleportWithBlackScreen exit");
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
        Trial trial = currentTrials[index];

        if (boss != null)
        {
            boss.transform.position = trial.bossSpawn.position;

            // Asegurar que este activo
            if (!boss.gameObject.activeSelf)
            {
                boss.gameObject.SetActive(true);
            }

                Debug.Log($"[TrialManager] Boss aparece en: {trial.bossSpawn.position}");
        }
    }

    public void TrialFailed()
    {
        if (phaseTransitionActive || !sequenceActive) return;
        if (directFightActive) return;
        running = false;
        DisableAndResetTrialBehaviours(currentTrials[index]);
        if (attackManager != null)
        {
            attackManager.ClearSpawnedObjects();
        }
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
        if (attackManager != null)
        {
            attackManager.ClearSpawnedObjects();
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
        Debug.Log("[TrialManager] Combate finalizado. El jugador puede continuar.");
    }

    public void PauseForDialoguePhase2()
    {
        StartCoroutine(TransitionToPhase2WithBlackScreen());
    }

    private IEnumerator TransitionToPhase2WithBlackScreen()
    {
        if (enableDebugLogs) Debug.Log("[TrialManager] TransitionToPhase2WithBlackScreen start");
        phaseTransitionActive = true;
        ignoreHitAdvance = true;
        if (attackManager == null)
            attackManager = FindFirstObjectByType<FirstEncounterBossAttackManager>(FindObjectsInactive.Include);
        if (attackManager != null)
        {
            attackManager.StopAttacks();
            attackManager.ClearSpawnedObjects();
            attackManager.enabled = false;
            if (enableDebugLogs) Debug.Log("[TrialManager] AttackManager stopped and disabled");
        }
        var bossObjForAttacks = bossController != null ? bossController.gameObject : null;
        if (bossObjForAttacks == null && boss != null) bossObjForAttacks = boss.gameObject;
        if (bossObjForAttacks != null)
        {
            var generalAttackA = bossObjForAttacks.GetComponentInChildren<TheTrueKnightAttackManager>(true);
            if (generalAttackA != null) generalAttackA.enabled = false;
            var generalAttackB = bossObjForAttacks.GetComponentInChildren<TrueKnightBoss>(true);
            if (generalAttackB != null) generalAttackB.enabled = false;
        }
        DisableAndResetTrialBehaviours(currentTrials[index]);
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(false);
            yield return new WaitForSeconds(0.2f);
        }
        if (boss == null && bossController != null)
            boss = bossController.GetComponent<BossLife>();
        if (boss != null) boss.SetAttackEnabled(false);
        // Teleport al área inicial antes de activar la fase 2
        if (gestorTeleport != null && areaInicialPlayerSpawn != null)
        {
            yield return TeleportWithBlackScreen(areaInicialPlayerSpawn, areaInicialCameraTarget);
            if (enableDebugLogs) Debug.Log("[TrialManager] Teleported to initial area for phase2 dialogue");
        }

        // Congelar jugador durante el diálogo de fase 2
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement pm = playerObj != null ? playerObj.GetComponent<PlayerMovement>() : null;
        Rigidbody2D prb = playerObj != null ? playerObj.GetComponent<Rigidbody2D>() : null;
        if (pm != null) pm.SetControlsEnabled(false);
        if (prb != null)
        {
            prb.linearVelocity = Vector2.zero;
            prb.simulated = false;
        }

        // Asegurar referencia al jefe y posicionarlo en el spawn inicial del área
        if (boss == null && bossController != null)
            boss = bossController.GetComponent<BossLife>();
        if (boss == null)
        {
            var bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj != null) boss = bossObj.GetComponent<BossLife>();
        }
        if (boss != null && areaInicialBossSpawn != null)
        {
            if (!boss.gameObject.activeSelf) boss.gameObject.SetActive(true);
            boss.transform.position = areaInicialBossSpawn.position;
            boss.SetAttackEnabled(false);
            if (enableDebugLogs) Debug.Log("[TrialManager] Boss positioned at initial area and attacks disabled");
        }

        if (bossController != null && areaInicialBossSpawn != null)
        {
            bossController.OnTrialSuccess(areaInicialBossSpawn.position);
            if (enableDebugLogs) Debug.Log("[TrialManager] BossController OnTrialSuccess at initial area");
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
        while (TextManager.IsOpen)
        {
            yield return null;
        }
        // Espera extra antes de activar la fase 2
        if (phase2DialogueExtraDelay > 0f)
        {
            yield return new WaitForSeconds(phase2DialogueExtraDelay);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }
        if (AudioManager.Instance != null && musicaFase2 != null)
        {
            AudioManager.Instance.PlayMusic(musicaFase2, musicaFase2Volume, true, true);
        }
        // Rehabilitar controles del jugador al terminar el diálogo
        if (prb != null) prb.simulated = true;
        if (pm != null) pm.SetControlsEnabled(true);
        // Activar Fase 2 y empezar en el primer trial
        phase2Active = true;
        currentTrials = (phase2Trials != null && phase2Trials.Length > 0) ? phase2Trials : phase1Trials;
        index = 0;
        phaseTransitionActive = false;
        ignoreHitAdvance = false;
        if (enableDebugLogs) Debug.Log("[TrialManager] TransitionToPhase2WithBlackScreen end, starting trial0");
        StartTrial(index);
    }

    public void TransitionToPhase2Immediate()
    {
        // No cambiar trials
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
            StartCoroutine(TeleportWithBlackScreen(areaInicialPlayerSpawn, areaInicialCameraTarget));
        }
    }

    public void SetSuperAccelerated(bool value)
    {
        superAccelerated = value;
    }

    

    // Llamado por BossLife cuando el jefe recibe un golpe en fase 2 de trials
    public void OnBossHit()
    {
        if (!sequenceActive) return;
        if (directFightActive) return;
        if (phaseTransitionActive || ignoreHitAdvance) return;
        StartCoroutine(AdvanceToNextTrialWithBlackScreen());
    }

    private void AdvanceToNextTrialImmediate()
    {
        if (currentTrials == null || currentTrials.Length == 0) return;
        DisableAndResetTrialBehaviours(currentTrials[index]);
        if (attackManager != null)
        {
            attackManager.ClearSpawnedObjects();
        }
        index = (index + 1) % currentTrials.Length;
        StartTrialImmediate(index);
    }

    private IEnumerator AdvanceToNextTrialWithBlackScreen()
    {
        if (enableDebugLogs) Debug.Log("[TrialManager] AdvanceToNextTrialWithBlackScreen from index=" + index);
        if (currentTrials == null || currentTrials.Length == 0) yield break;
        DisableAndResetTrialBehaviours(currentTrials[index]);
        if (attackManager != null)
        {
            attackManager.ClearSpawnedObjects();
        }
        index = (index + 1) % currentTrials.Length;
        int trialIndex = index;

        if (!sequenceActive) yield break;
        if (directFightActive) yield break;
        if (currentTrials == null || currentTrials.Length == 0) { EndSequenceVictory(); yield break; }
        if (trialIndex < 0 || trialIndex >= currentTrials.Length) { EndSequenceVictory(); yield break; }

        Trial trial = currentTrials[trialIndex];
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = true;
        timer = GetTime();

        if (gestorTeleport == null)
        {
            Debug.LogError("[TrialManager] gestorTeleport es NULL");
            yield break;
        }

        if (boss != null && !boss.gameObject.activeSelf)
        {
            boss.gameObject.SetActive(true);
        }

        yield return TeleportWithBlackScreen(trial.playerSpawn, trial.cameraTarget);
        if (enableDebugLogs) Debug.Log("[TrialManager] Teleported to next trial trialIndex=" + trialIndex);

        if (boss != null && trial.bossSpawn != null)
        {
            boss.transform.position = trial.bossSpawn.position;
            if (bossController != null)
            {
                bossController.OnTrialSuccess(trial.bossSpawn.position);
            }
        }

        EnableTrialBehaviours(trial);
        timerRoutine = StartCoroutine(TimerRoutine());
        if (attackManager == null)
            attackManager = FindFirstObjectByType<FirstEncounterBossAttackManager>(FindObjectsInactive.Include);
        if (attackManager != null)
        {
            if (!attackManager.enabled) attackManager.enabled = true;
            attackManager.StartTrialAttacks(phase2Active);
            if (enableDebugLogs) Debug.Log("[TrialManager] AttackManager StartTrialAttacks next trial");
        }
    }

    private void StartTrialImmediate(int trialIndex)
    {
        if (enableDebugLogs) Debug.Log("[TrialManager] StartTrialImmediate trialIndex=" + trialIndex);
        if (!sequenceActive) return;
        if (directFightActive) return;
        if (currentTrials == null || currentTrials.Length == 0) { EndSequenceVictory(); return; }
        if (trialIndex < 0 || trialIndex >= currentTrials.Length) { EndSequenceVictory(); return; }

        Trial trial = currentTrials[trialIndex];
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = true;
        timer = GetTime();

        if (gestorTeleport == null)
        {
            Debug.LogError("[TrialManager] gestorTeleport es NULL");
            return;
        }

        if (boss != null && !boss.gameObject.activeSelf)
        {
            boss.gameObject.SetActive(true);
        }

        gestorTeleport.TeleportRaw(trial.playerSpawn, trial.cameraTarget);

        if (boss != null && trial.bossSpawn != null)
        {
            boss.transform.position = trial.bossSpawn.position;
            if (bossController != null)
            {
                bossController.OnTrialSuccess(trial.bossSpawn.position);
            }
        }

        EnableTrialBehaviours(trial);
        timerRoutine = StartCoroutine(TimerRoutine());
        if (attackManager == null)
            attackManager = FindFirstObjectByType<FirstEncounterBossAttackManager>(FindObjectsInactive.Include);
        if (attackManager != null)
        {
            if (!attackManager.enabled) attackManager.enabled = true;
            attackManager.StartTrialAttacks(phase2Active);
            if (enableDebugLogs) Debug.Log("[TrialManager] AttackManager StartTrialAttacks immediate");
        }
    }

    

    public float RemainingTime => timer;

    private void EnableTrialBehaviours(Trial trial)
    {
        if (trial == null) return;
        if (trial.enableOnEnterObjects != null)
        {
            for (int i = 0; i < trial.enableOnEnterObjects.Length; i++)
            {
                var go = trial.enableOnEnterObjects[i];
                if (go != null) go.SetActive(true);
            }
        }
        if (trial.enableOnEnterComponents != null)
        {
            for (int i = 0; i < trial.enableOnEnterComponents.Length; i++)
            {
                var comp = trial.enableOnEnterComponents[i];
                if (comp != null) comp.enabled = true;
            }
        }
    }

    private void DisableAndResetTrialBehaviours(Trial trial)
    {
        if (trial == null) return;
        if (trial.disableOnExitObjects != null)
        {
            for (int i = 0; i < trial.disableOnExitObjects.Length; i++)
            {
                var go = trial.disableOnExitObjects[i];
                if (go != null) go.SetActive(false);
            }
        }
        if (trial.disableOnExitComponents != null)
        {
            for (int i = 0; i < trial.disableOnExitComponents.Length; i++)
            {
                var comp = trial.disableOnExitComponents[i];
                if (comp != null) comp.enabled = false;
            }
        }

        if (trial.enableOnEnterComponents != null)
        {
            for (int i = 0; i < trial.enableOnEnterComponents.Length; i++)
            {
                var comp = trial.enableOnEnterComponents[i];
                if (comp == null) continue;
                comp.enabled = false;
                comp.enabled = true;
            }
        }
    }
}
