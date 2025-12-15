using UnityEngine;
using System;
using System.Collections;

public class TrueFinalBossController : MonoBehaviour
{
    [SerializeField] private AudioSource music;
    [SerializeField] private TrueFinalBossMusicSync musicSync;
    [SerializeField] private TrueFinalBossStateMachine stateMachine;
    [SerializeField] private TrueFinalBossZoneManager zoneManager;
    [SerializeField] private TrueFinalBossVisualEffects vfx;
    [SerializeField] private Transform boss;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 introFinalPosition;
    [SerializeField] private float introSpeed = 3f;
    [SerializeField] private TrueFinalBossSnowBridge snowBridge;
    [SerializeField] private TrueFinalBossAlterTownAttackController alterController;
    [SerializeField] private string endCombatDialogue;
    [SerializeField] private bool logDebug = true;
    [SerializeField] private bool autoEnterCombatAfterIntro = false;
    [SerializeField] private float summonFadeDuration = 0.8f;
    [SerializeField] private float summonShakeDuration = 0.8f;
    [SerializeField] private float summonShakeIntensity = 0.2f;
    [SerializeField] private float summonPulseMin = 0.6f;
    [SerializeField] private float summonPulseMax = 0.95f;
    [SerializeField] private float summonPulsePeriod = 2f;
    [SerializeField] private float summonHoldDuration = 1.5f;
    [SerializeField] private float introShakeIntensity = 0.15f;
    [SerializeField] private bool useGlobalFadeForIntro = false;
    [SerializeField] private float introFadeOutWait = 0.2f;
    [SerializeField] private GameObject finalBossDropPrefab;
    [SerializeField] private Vector3 finalBossDropOffset;
    [SerializeField] private float finalBossDropDelay = 0.5f;
    [System.Serializable] public class ZoneAttackSet { public string zoneName; public MonoBehaviour[] patternBehaviours; public bool randomOrder; }
    [SerializeField] private ZoneAttackSet[] zoneAttackSets;
    [SerializeField] private MonoBehaviour[] bossGlobalBehaviours;
    [SerializeField] private bool bossGlobalRandomOrder = false;
    [SerializeField] private bool enableBossGlobalAttacks = false;
    [SerializeField] private bool useZones = false;
    [SerializeField] private bool skipIntroCompletely = true;
    [SerializeField] private bool enableAggressionScaling = true;
    [SerializeField] private float aggressionHighThreshold = 0.75f;
    [SerializeField] private float aggressionLowThreshold = 0.4f;
    [SerializeField] private bool disableIntroMovement = false;
    [SerializeField] private bool enableHorizontalSlide = false;
    [SerializeField] private Transform leftSlidePoint;
    [SerializeField] private Transform rightSlidePoint;
    [SerializeField] private float slideSpeed = 3f;
    [SerializeField] private float slidePause = 0.2f;
    [SerializeField] private SpriteRenderer bossSprite;
    [SerializeField] private SpriteRenderer amuletSprite;
    [SerializeField] private bool setBlackSilhouetteDuringIntro = true;
    [SerializeField] private float spriteFadeToWhiteDuration = 0.4f;
    [SerializeField] private bool useAttackTimeout = false;
    [SerializeField] private float maxAttackDuration = 12f;
    [SerializeField] private float maxBossAttackDuration = 10f;
    [SerializeField] private bool stopAttacksOnZoneChange = false;
    [SerializeField] private bool stopAttacksOnPhaseChange = false;

    private Coroutine introRoutine;
    private Coroutine summonRoutine;
    public event Action OnTransformationComplete;
    private float lastPauseDuration;
    private Coroutine attackTimeoutRoutine;
    private Coroutine bossAttackTimeoutRoutine;
    private Coroutine slideRoutine;
    private IAttackPattern[] currentPatterns;
    private int currentPatternIndex = -1;
    private IAttackPattern currentPattern;
    private string activeZone;
    private IAttackPattern[] bossPatterns;
    private int bossPatternIndex = -1;
    private IAttackPattern bossCurrentPattern;

    public void Init()
    {
        if (musicSync != null)
        {
            musicSync.OnZoneEvent += OnZoneEvent;
            musicSync.OnStateEvent += OnStateEvent;
            musicSync.OnMusicPaused += OnMusicPaused;
            musicSync.OnMusicResumed += OnMusicResumed;
            musicSync.OnPauseBegin += OnPauseBegin;
        }
        if (stateMachine != null)
        {
            stateMachine.OnStateEnter += OnEnterState;
            stateMachine.OnStateExit += OnExitState;
        }
        var bl = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
        if (bl != null && finalBossDropPrefab != null)
        {
            bl.SetDropOnDeath(finalBossDropPrefab, finalBossDropOffset, finalBossDropDelay);
        }
        if (logDebug) Debug.Log("[TrueFinalBossController] Inicializado y eventos conectados");
    }

    public void BeginFight()
    {
        if (skipIntroCompletely) { EnterCombat(); return; }
        StartIntro();
        if (logDebug) Debug.Log("[TrueFinalBossController] Comenzó la intro de la pelea");
    }
    public void SetZoneChangesDisabled(bool value)
    {
        if (zoneManager != null) zoneManager.SetZoneChangesDisabled(value);
    }
    public void SetBossGlobalAttacksEnabled(bool value)
    {
        enableBossGlobalAttacks = value;
    }

    private void OnZoneEvent(string name)
    {
        if (!useZones)
        {
            if (enableBossGlobalAttacks && bossPatterns == null)
            {
                StartBossGlobalAttacks();
            }
            return;
        }
        if (stopAttacksOnZoneChange)
        {
            StopZoneAttacks();
            CleanupTransientBossObjects();
        }
        if (zoneManager != null) zoneManager.ActivateZone(name);
        if (logDebug) Debug.Log($"[TrueFinalBossController] Zona activada: {name}");
        activeZone = name;
        if (enableBossGlobalAttacks && bossPatterns == null)
        {
            StartBossGlobalAttacks();
        }
        if (name == "Snow")
        {
            if (snowBridge != null) snowBridge.ActivateByNumber(1);
            if (alterController != null) alterController.StopZone();
            if (logDebug) Debug.Log("[TrueFinalBossController] Ataques Snow activados, AlterTown desactivado");
        }
        else if (name == "AlterTown")
        {
            if (alterController != null) alterController.StartZone();
            if (snowBridge != null) snowBridge.StopAll();
            if (logDebug) Debug.Log("[TrueFinalBossController] Ataques AlterTown activados, Snow desactivado");
        }
        else
        {
            if (snowBridge != null) snowBridge.StopAll();
            if (alterController != null) alterController.StopZone();
            StartZoneAttacks(name);
        }
    }

    private void OnStateEvent(TrueFinalBossStateMachine.BossState s)
    {
        if (stateMachine != null) stateMachine.ChangeState(s);
        if (logDebug) Debug.Log($"[TrueFinalBossController] Solicitud de cambio de estado por música: {s}");
    }

    private void OnMusicPaused() { }
    private void OnPauseBegin(float duration) { lastPauseDuration = duration; if (vfx != null) vfx.FadeToBlack(Mathf.Max(0.1f, duration)); }
    private void OnMusicResumed() { }

    private void OnEnterState(TrueFinalBossStateMachine.BossState s)
    {
        if (logDebug) Debug.Log($"[TrueFinalBossController] Entró estado: {s}");
        if (s == TrueFinalBossStateMachine.BossState.Intro)
        {
            // Efecto de advertencia en Intro (shake) y rely on pause to fade
            if (vfx != null) vfx.ShakeCamera(0.8f, 0.15f);
            StartIntro();
        }
        else if (s == TrueFinalBossStateMachine.BossState.Reflection)
        {
            if (snowBridge != null) snowBridge.StopAll();
            if (alterController != null) alterController.StopZone();
            if (stopAttacksOnPhaseChange)
            {
                StopZoneAttacks();
                StopBossGlobalAttacks();
            }
            ShowEndDialogueIfAny();
        }
        else if (s == TrueFinalBossStateMachine.BossState.AutoDefeat)
        {
            if (snowBridge != null) snowBridge.StopAll();
            if (alterController != null) alterController.StopZone();
            if (stopAttacksOnPhaseChange)
            {
                StopZoneAttacks();
                StopBossGlobalAttacks();
            }
            ShowEndDialogueIfAny();
        }
    }

    private void OnExitState(TrueFinalBossStateMachine.BossState s) { }

    public void StartIntro()
    {
        if (introRoutine != null) StopCoroutine(introRoutine);
        introRoutine = StartCoroutine(IntroCinematic());
    }

    public void PlaySummonEffect()
    {
        if (vfx != null) vfx.ShakeCamera(summonShakeDuration, summonShakeIntensity);
        if (logDebug) Debug.Log("[TrueFinalBossController] Shake de spawn ejecutado");
    }

    public void StartPulse()
    {
        if (vfx != null)
        {
            vfx.StartBlackPulse(summonPulseMin, summonPulseMax, summonPulsePeriod);
            if (summonHoldDuration > 0f)
            {
                if (summonRoutine != null) StopCoroutine(summonRoutine);
                summonRoutine = StartCoroutine(StopPulseAfterDelay(summonHoldDuration));
            }
        }
    }

    private IEnumerator StopPulseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StopPulse();
        summonRoutine = null;
    }

    public void StartTransformationSequence()
    {
        StartCoroutine(TransformationRoutine());
    }

    private IEnumerator IntroCinematic()
    {
        if (!useGlobalFadeForIntro)
        {
            if (vfx != null) vfx.FadeToBlack(0.8f);
        }
        else
        {
            if (FadeController.Instance != null)
            {
                FadeController.Instance.ActivarFadeOut();
                if (introFadeOutWait > 0f) yield return new WaitForSecondsRealtime(introFadeOutWait);
            }
        }
        if (boss == null)
        {
            var bl = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
            if (bl != null) boss = bl.transform;
        }
        if (setBlackSilhouetteDuringIntro)
        {
            if (bossSprite == null && boss != null)
                bossSprite = boss.GetComponentInChildren<SpriteRenderer>(true);
            SetAllSpritesColor(Color.black);
            if (amuletSprite != null) amuletSprite.color = Color.black;
        }
        if (boss != null)
        {
            SetAllAnimatorsEnabled(false);
            if (!disableIntroMovement)
            {
                Vector3 target = introFinalPosition;
                float movementDuration = introSpeed > 0f ? Vector3.Distance(boss.position, target) / introSpeed : 0f;
                if (movementDuration > 0f && vfx != null) vfx.ShakeCamera(movementDuration, introShakeIntensity);
                while (Vector3.Distance(boss.position, target) > 0.01f)
                {
                    boss.position = Vector3.MoveTowards(boss.position, target, introSpeed * Time.deltaTime);
                    yield return null;
                }
            }
            if (bossSprite != null && setBlackSilhouetteDuringIntro)
            {
                float t = 0f;
                Color from = Color.black;
                Color to = Color.white;
                while (t < spriteFadeToWhiteDuration)
                {
                    t += Time.deltaTime;
                    float k = spriteFadeToWhiteDuration > 0f ? Mathf.Clamp01(t / spriteFadeToWhiteDuration) : 1f;
                    if (bossSprite != null) bossSprite.color = Color.Lerp(from, to, k);
                    if (amuletSprite != null)
                    {
                        amuletSprite.color = Color.Lerp(from, to, k);
                    }
                    yield return null;
                }
                bossSprite.color = Color.white;
                if (amuletSprite != null) amuletSprite.color = Color.white;
            }
            SetAllAnimatorsEnabled(true);
        }
        if (!useGlobalFadeForIntro)
        {
            if (vfx != null) vfx.ActivateRedBackground();
        }
        else
        {
            if (FadeController.Instance != null) FadeController.Instance.ActivarFadeIn();
        }
        SetAllSpritesColor(Color.white);
        EnsureBossSpritesActive();
        if (autoEnterCombatAfterIntro && stateMachine != null)
        {
            stateMachine.ChangeState(TrueFinalBossStateMachine.BossState.Combat);
            if (logDebug) Debug.Log("[TrueFinalBossController] Intro finalizada, estado Combat (autoEnter=true)");
        }
        else
        {
            if (logDebug) Debug.Log("[TrueFinalBossController] Intro finalizada, esperando evento externo para entrar a Combat");
        }
        introRoutine = null;
    }

    // Secuencia de spawn simplificada a solo shake; sin fade ni overlay

    private IEnumerator TransformationRoutine()
    {
        if (stateMachine != null)
        {
            stateMachine.ChangeState(TrueFinalBossStateMachine.BossState.Transformation);
            if (logDebug) Debug.Log("[TrueFinalBossController] Estado Transformation iniciado");
        }
        float wait = stateMachine != null ? stateMachine.GetStateDuration(TrueFinalBossStateMachine.BossState.Transformation) : 2f;
        if (vfx != null)
        {
            vfx.FadeToBlack(Mathf.Max(0.1f, summonFadeDuration));
            vfx.ShakeCamera(Mathf.Max(0.1f, summonShakeDuration), summonShakeIntensity);
        }
        float t = 0f;
        while (t < wait)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (vfx != null) vfx.ActivateRedBackground();
        if (logDebug) Debug.Log("[TrueFinalBossController] Transformation finalizada");
        OnTransformationComplete?.Invoke();
    }

    public void EnterCombat()
    {
        if (stateMachine != null)
        {
            EnsureBossSpritesActive();
            stateMachine.ChangeState(TrueFinalBossStateMachine.BossState.Combat);
            if (logDebug) Debug.Log("[TrueFinalBossController] Estado Combat iniciado manualmente");
            if (enableHorizontalSlide && slideRoutine == null && boss != null && leftSlidePoint != null && rightSlidePoint != null)
                slideRoutine = StartCoroutine(HorizontalSlideLoop());
            return;
        }
        if (logDebug) Debug.LogWarning("[TrueFinalBossController] stateMachine no asignado, iniciando combate sin máquina de estados");
        EnsureBossSpritesActive();
        enableBossGlobalAttacks = true;
        StartBossGlobalAttacks();
        if (enableHorizontalSlide && slideRoutine == null && boss != null && leftSlidePoint != null && rightSlidePoint != null)
            slideRoutine = StartCoroutine(HorizontalSlideLoop());
    }

    public void StopPulse()
    {
        if (vfx != null) vfx.StopPulse();
    }

    private ZoneAttackSet GetZoneAttackSet(string name)
    {
        if (zoneAttackSets == null) return null;
        for (int i = 0; i < zoneAttackSets.Length; i++)
        {
            var z = zoneAttackSets[i];
            if (z != null && z.zoneName == name) return z;
        }
        return null;
    }

    private void StartZoneAttacks(string zoneName)
    {
        var set = GetZoneAttackSet(zoneName);
        if (set == null || set.patternBehaviours == null || set.patternBehaviours.Length == 0)
        {
            if (logDebug) Debug.Log($"[TrueFinalBossController] No hay ataques configurados para la zona {zoneName}");
            return;
        }
        currentPatterns = new IAttackPattern[set.patternBehaviours.Length];
        for (int i = 0; i < set.patternBehaviours.Length; i++)
        {
            var mb = set.patternBehaviours[i];
            if (mb == null) { currentPatterns[i] = null; continue; }
            if (!mb.gameObject.activeSelf) mb.gameObject.SetActive(true);
            // Mantener deshabilitados aquí para evitar auto-inicio en OnEnable
            if (mb.enabled) mb.enabled = false;
            currentPatterns[i] = mb as IAttackPattern;
            if (currentPatterns[i] == null && logDebug)
                Debug.LogWarning($"[TrueFinalBossController] patternBehaviours[{i}] no implementa IAttackPattern en zona {zoneName}");
        }
        currentPatternIndex = -1;
        PickAndStartNextPattern(set.randomOrder);
    }

    private void StopZoneAttacks()
    {
        if (currentPattern != null)
        {
            currentPattern.OnFinished -= OnPatternFinished;
            currentPattern.StopAttack();
            currentPattern = null;
        }
        if (zoneAttackSets != null)
        {
            for (int s = 0; s < zoneAttackSets.Length; s++)
            {
                var set = zoneAttackSets[s];
                if (set == null || set.patternBehaviours == null) continue;
                for (int i = 0; i < set.patternBehaviours.Length; i++)
                {
                    var mb = set.patternBehaviours[i];
                    if (mb == null) continue;
                    if (mb.enabled) mb.enabled = false;
                }
            }
        }
        CleanupTransientBossObjects();
        currentPatterns = null;
        currentPatternIndex = -1;
    }

    private void PickAndStartNextPattern(bool randomOrder)
    {
        if (currentPatterns == null || currentPatterns.Length == 0) return;
        if (currentPattern != null)
        {
            currentPattern.OnFinished -= OnPatternFinished;
            currentPattern.StopAttack();
            var prevMb = currentPattern as MonoBehaviour;
            if (prevMb != null && prevMb.enabled) prevMb.enabled = false;
            currentPattern = null;
        }
        if (randomOrder)
        {
            currentPatternIndex = UnityEngine.Random.Range(0, currentPatterns.Length);
        }
        else
        {
            currentPatternIndex = (currentPatternIndex + 1) % currentPatterns.Length;
        }
        int tries = 0;
        while (tries < currentPatterns.Length)
        {
            currentPattern = currentPatterns[currentPatternIndex];
            if (currentPattern != null) break;
            if (randomOrder)
                currentPatternIndex = UnityEngine.Random.Range(0, currentPatterns.Length);
            else
                currentPatternIndex = (currentPatternIndex + 1) % currentPatterns.Length;
            tries++;
        }
        if (currentPattern == null)
        {
            if (logDebug) Debug.LogWarning($"[TrueFinalBossController] No hay patrones válidos en zona {activeZone}");
            return;
        }
        var nextMb = currentPattern as MonoBehaviour;
        if (nextMb != null)
        {
            if (!nextMb.gameObject.activeSelf) nextMb.gameObject.SetActive(true);
            if (!nextMb.enabled) nextMb.enabled = true;
        }
        currentPattern.OnFinished += OnPatternFinished;
        currentPattern.StartAttack();
        if (logDebug) Debug.Log($"[TrueFinalBossController] Ataque iniciado en zona {activeZone} patrónIndex={currentPatternIndex}");
        if (useAttackTimeout)
        {
            if (attackTimeoutRoutine != null) { StopCoroutine(attackTimeoutRoutine); attackTimeoutRoutine = null; }
            attackTimeoutRoutine = StartCoroutine(ZoneAttackTimeout());
        }
    }

    private void OnPatternFinished()
    {
        if (attackTimeoutRoutine != null) { StopCoroutine(attackTimeoutRoutine); attackTimeoutRoutine = null; }
        var set = GetZoneAttackSet(activeZone);
        bool randomOrder = set != null && set.randomOrder;
        PickAndStartNextPattern(randomOrder);
    }

    private void StartBossGlobalAttacks()
    {
        if (!enableBossGlobalAttacks || bossGlobalBehaviours == null || bossGlobalBehaviours.Length == 0) return;
        ApplyAggressionScaling();
        bossPatterns = new IAttackPattern[bossGlobalBehaviours.Length];
        for (int i = 0; i < bossGlobalBehaviours.Length; i++)
        {
            var mb = bossGlobalBehaviours[i];
            if (mb == null) { bossPatterns[i] = null; continue; }
            if (!mb.gameObject.activeSelf) mb.gameObject.SetActive(true);
            if (mb.enabled) mb.enabled = false;
            bossPatterns[i] = mb as IAttackPattern;
        }
        bossPatternIndex = -1;
        PickAndStartNextBossPattern(bossGlobalRandomOrder);
    }

    private void ApplyAggressionScaling()
    {
        if (!enableAggressionScaling) return;
        var playerLifeComp = player != null ? player.GetComponent<playerLife>() : FindFirstObjectByType<playerLife>(FindObjectsInactive.Include);
        if (playerLifeComp == null) return;
        float ratio = playerLifeComp.MaxHealth > 0 ? (float)playerLifeComp.Health / (float)playerLifeComp.MaxHealth : 1f;
        // Más agresivo con más vida del jugador: reducir duración de patrones
        float minDur = 5f;
        float maxDur = 12f;
        float t = Mathf.Clamp01(ratio); // 0 jugador sin vida, 1 jugador con vida llena
        maxBossAttackDuration = Mathf.Lerp(maxDur, minDur, t);
        useAttackTimeout = true;
        // Orden aleatorio cuando la vida del jugador es alta
        bossGlobalRandomOrder = ratio >= aggressionHighThreshold;
    }

    private void StopBossGlobalAttacks()
    {
        if (bossCurrentPattern != null)
        {
            bossCurrentPattern.OnFinished -= OnBossPatternFinished;
            bossCurrentPattern.StopAttack();
            var mb = bossCurrentPattern as MonoBehaviour;
            if (mb != null && mb.enabled) mb.enabled = false;
            bossCurrentPattern = null;
        }
        if (bossGlobalBehaviours != null)
        {
            for (int i = 0; i < bossGlobalBehaviours.Length; i++)
            {
                var mb = bossGlobalBehaviours[i];
                if (mb == null) continue;
                if (mb.enabled) mb.enabled = false;
            }
        }
        bossPatterns = null;
        bossPatternIndex = -1;
    }

    private void PickAndStartNextBossPattern(bool randomOrder)
    {
        if (bossPatterns == null || bossPatterns.Length == 0) return;
        if (bossCurrentPattern != null)
        {
            bossCurrentPattern.OnFinished -= OnBossPatternFinished;
            bossCurrentPattern.StopAttack();
            var prevMb = bossCurrentPattern as MonoBehaviour;
            if (prevMb != null && prevMb.enabled) prevMb.enabled = false;
            bossCurrentPattern = null;
        }
        if (randomOrder)
        {
            bossPatternIndex = UnityEngine.Random.Range(0, bossPatterns.Length);
        }
        else
        {
            bossPatternIndex = (bossPatternIndex + 1) % bossPatterns.Length;
        }
        int tries = 0;
        while (tries < bossPatterns.Length)
        {
            bossCurrentPattern = bossPatterns[bossPatternIndex];
            if (bossCurrentPattern != null) break;
            if (randomOrder)
                bossPatternIndex = UnityEngine.Random.Range(0, bossPatterns.Length);
            else
                bossPatternIndex = (bossPatternIndex + 1) % bossPatterns.Length;
            tries++;
        }
        if (bossCurrentPattern == null) return;
        var nextMb = bossCurrentPattern as MonoBehaviour;
        if (nextMb != null)
        {
            if (!nextMb.gameObject.activeSelf) nextMb.gameObject.SetActive(true);
            if (!nextMb.enabled) nextMb.enabled = true;
        }
        bossCurrentPattern.OnFinished += OnBossPatternFinished;
        bossCurrentPattern.StartAttack();
        if (useAttackTimeout)
        {
            if (bossAttackTimeoutRoutine != null) { StopCoroutine(bossAttackTimeoutRoutine); bossAttackTimeoutRoutine = null; }
            bossAttackTimeoutRoutine = StartCoroutine(BossAttackTimeout());
        }
    }

    private void OnBossPatternFinished()
    {
        if (!this) return;
        if (bossAttackTimeoutRoutine != null) { StopCoroutine(bossAttackTimeoutRoutine); bossAttackTimeoutRoutine = null; }
        ApplyAggressionScaling();
        PickAndStartNextBossPattern(bossGlobalRandomOrder);
    }

    private void CleanupTransientBossObjects()
    {
        var movers = FindObjectsByType<UniversalProjectileMover>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < movers.Length; i++)
        {
            var m = movers[i];
            if (m != null) Destroy(m.gameObject);
        }
        var bullets = FindObjectsByType<BulletScript>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < bullets.Length; i++)
        {
            var b = bullets[i];
            if (b != null) Destroy(b.gameObject);
        }
        var parab = FindObjectsByType<ParabolicBulletScript>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < parab.Length; i++)
        {
            var p = parab[i];
            if (p != null) Destroy(p.gameObject);
        }
        var bossSpawned = FindObjectsByType<BossSpawnedAutoCleanup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < bossSpawned.Length; i++)
        {
            var t = bossSpawned[i];
            if (t != null) Destroy(t.gameObject);
        }
    }
    private IEnumerator ZoneAttackTimeout()
    {
        float t = 0f;
        while (t < maxAttackDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        attackTimeoutRoutine = null;
        if (currentPattern != null)
        {
            currentPattern.StopAttack();
        }
        OnPatternFinished();
    }
    private IEnumerator BossAttackTimeout()
    {
        float t = 0f;
        while (t < maxBossAttackDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        bossAttackTimeoutRoutine = null;
        if (bossCurrentPattern != null)
        {
            bossCurrentPattern.StopAttack();
        }
        OnBossPatternFinished();
    }
    private void ShowEndDialogueIfAny()
    {
        if (!string.IsNullOrEmpty(endCombatDialogue) && TextManager.Instance != null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.StopMusic(true);
            TextManager.Instance.ShowDialogue(endCombatDialogue);
            if (logDebug) Debug.Log("[TrueFinalBossController] Diálogo de fin de combate mostrado");
        }
    }

    private void EnsureBossSpritesActive()
    {
        if (bossSprite == null && boss != null)
            bossSprite = boss.GetComponentInChildren<SpriteRenderer>(true);
        if (bossSprite == null)
        {
            var bl = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
            if (bl != null) bossSprite = bl.GetComponentInChildren<SpriteRenderer>(true);
        }
        if (bossSprite != null)
        {
            bossSprite.enabled = true;
            bossSprite.color = Color.white;
        }
        if (amuletSprite != null)
        {
            amuletSprite.enabled = true;
            amuletSprite.color = Color.white;
        }
        SetAllAnimatorsEnabled(true);
        SetAllSpritesColor(Color.white);
    }

    private void SetAllSpritesColor(Color color)
    {
        if (boss != null)
        {
            var srs = boss.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < srs.Length; i++)
            {
                var sr = srs[i];
                if (sr != null) sr.color = color;
            }
        }
        else
        {
            var bl = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
            if (bl != null)
            {
                var srs = bl.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < srs.Length; i++)
                {
                    var sr = srs[i];
                    if (sr != null) sr.color = color;
                }
            }
        }
    }

    private void SetAllAnimatorsEnabled(bool enabled)
    {
        if (boss != null)
        {
            var anims = boss.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < anims.Length; i++)
            {
                var a = anims[i];
                if (a != null) a.enabled = enabled;
            }
        }
        else
        {
            var bl = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
            if (bl != null)
            {
                var anims = bl.GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < anims.Length; i++)
                {
                    var a = anims[i];
                    if (a != null) a.enabled = enabled;
                }
            }
        }
    }
    public void TriggerHorizontalSlideStep()
    {
        if (!enableHorizontalSlide || boss == null || leftSlidePoint == null || rightSlidePoint == null) return;
        if (slideRoutine != null) return;
        slideRoutine = StartCoroutine(SlideOnce());
    }

    private bool slideToRight = true;
    private IEnumerator SlideOnce()
    {
        Vector3 target = slideToRight ? rightSlidePoint.position : leftSlidePoint.position;
        while (Vector3.Distance(boss.position, target) > 0.01f)
        {
            boss.position = Vector3.MoveTowards(boss.position, target, Mathf.Max(0.01f, slideSpeed) * Time.unscaledDeltaTime);
            yield return null;
        }
        slideToRight = !slideToRight;
        if (slidePause > 0f) yield return new WaitForSecondsRealtime(slidePause);
        slideRoutine = null;
    }

    public void ForceStopAllBossAttacks()
    {
        if (attackTimeoutRoutine != null) { StopCoroutine(attackTimeoutRoutine); attackTimeoutRoutine = null; }
        if (bossAttackTimeoutRoutine != null) { StopCoroutine(bossAttackTimeoutRoutine); bossAttackTimeoutRoutine = null; }
        StopZoneAttacks();
        StopBossGlobalAttacks();
        CleanupTransientBossObjects();
    }

    private IEnumerator HorizontalSlideLoop()
    {
        if (!enableHorizontalSlide || boss == null || leftSlidePoint == null || rightSlidePoint == null)
        {
            yield break;
        }
        bool toRight = true;
        while (true)
        {
            if (!enableHorizontalSlide) { slideRoutine = null; yield break; }
            Vector3 target = toRight ? rightSlidePoint.position : leftSlidePoint.position;
            while (Vector3.Distance(boss.position, target) > 0.01f)
            {
                if (!enableHorizontalSlide) { slideRoutine = null; yield break; }
                boss.position = Vector3.MoveTowards(boss.position, target, Mathf.Max(0.01f, slideSpeed) * Time.deltaTime);
                yield return null;
            }
            if (slidePause > 0f) yield return new WaitForSeconds(slidePause);
            toRight = !toRight;
        }
    }

    public void SetHorizontalSlideEnabled(bool enabled)
    {
        enableHorizontalSlide = enabled;
        if (!enabled && slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
            slideRoutine = null;
        }
    }

    // ========= PATRONES DEL FINALBOSS ORIGINAL (ADAPTADOS) =========
    // Se pueden añadir estos componentes al boss y referenciarlos en zoneAttackSets
}

public class FinalBossMeteorRainAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;
    [SerializeField] private GameObject meteorSphere;
    [SerializeField] private Transform meteorSpawnPoint;
    [SerializeField] private float spawnWidth = 10f;
    [SerializeField] private int meteorCount = 12;
    [SerializeField] private float spawnInterval = 0.25f;
    [SerializeField] private float meteorFallSpeed = 8f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float meteorLifetime = 4f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly System.Collections.Generic.List<GameObject> spawned = new System.Collections.Generic.List<GameObject>();

    private void OnEnable() { if (autoStartOnEnable) StartAttack(); }
    private void OnDisable() { StopAttack(); }
    public void StartAttack()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }
    public void StopAttack()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
        for (int i = 0; i < spawned.Count; i++)
        {
            var go = spawned[i];
            if (go != null) Destroy(go);
        }
        spawned.Clear();
    }
    private IEnumerator Run()
    {
        if (meteorSphere == null || meteorSpawnPoint == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector3 top = meteorSpawnPoint.position;
        for (int i = 0; i < meteorCount; i++)
        {
            float x = top.x + UnityEngine.Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f);
            Vector3 pos = new Vector3(x, top.y, 0f);
            GameObject m = Instantiate(meteorSphere, pos, Quaternion.identity);
            spawned.Add(m);
            var rb = m.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.down * meteorFallSpeed;
            Destroy(m, meteorLifetime);
            yield return new WaitForSeconds(spawnInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}

public class FinalBossPyramidAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;
    [SerializeField] private GameObject pyramidSphere;
    [SerializeField] private Transform leftPyramidSpawn;
    [SerializeField] private Transform rightPyramidSpawn;
    [SerializeField] private int rows = 5;
    [SerializeField] private float rowInterval = 0.4f;
    [SerializeField] private float pyramidSphereSpeed = 4f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly System.Collections.Generic.List<GameObject> spawned = new System.Collections.Generic.List<GameObject>();

    private void OnEnable() { if (autoStartOnEnable) StartAttack(); }
    private void OnDisable() { StopAttack(); }
    public void StartAttack()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }
    public void StopAttack()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
        for (int i = 0; i < spawned.Count; i++)
        {
            var go = spawned[i];
            if (go != null) Destroy(go);
        }
        spawned.Clear();
    }
    private IEnumerator Run()
    {
        if (pyramidSphere == null || leftPyramidSpawn == null || rightPyramidSpawn == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        for (int row = 0; row < rows; row++)
        {
            GameObject leftSphere = Instantiate(pyramidSphere, leftPyramidSpawn.position, Quaternion.identity);
            GameObject rightSphere = Instantiate(pyramidSphere, rightPyramidSpawn.position, Quaternion.identity);
            spawned.Add(leftSphere); spawned.Add(rightSphere);
            var leftRb = leftSphere.GetComponent<Rigidbody2D>();
            var rightRb = rightSphere.GetComponent<Rigidbody2D>();
            if (leftRb != null)
            {
                float angle = 60f + (row * 10f);
                Vector2 leftDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                leftRb.linearVelocity = leftDir * pyramidSphereSpeed;
            }
            if (rightRb != null)
            {
                float angle = 120f - (row * 10f);
                Vector2 rightDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                rightRb.linearVelocity = rightDir * pyramidSphereSpeed;
            }
            yield return new WaitForSeconds(rowInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
