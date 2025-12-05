using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Sistema de movimiento universal para proyectiles, huesos y ataques del boss
/// Súper flexible y configurable desde el Inspector
/// </summary>
public class UniversalProjectileMover : MonoBehaviour
{
    public enum MovementDirection
    {
        Right,          // →
        Left,           // ←
        Up,             // ↑
        Down,           // ↓
        TowardsPlayer,  // 🎯 Persigue al jugador
        Custom          // Vector2 personalizado
    }

    [Header("═══════════════════════════════")]
    [Header("Oscilación")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool enableOscillation = false;
    [SerializeField] private OscillationAxis oscillationAxis = OscillationAxis.Vertical;

    public enum OscillationAxis
    {
        Vertical,   // Oscila en Y mientras avanza
        Horizontal, // Oscila en X mientras avanza
        Both        // Oscila en ambos ejes (movimiento circular)
    }

    [SerializeField] private float oscillationAmplitude = 2f;
    [SerializeField] private float oscillationFrequency = 2f;
    [SerializeField] private bool randomizeOscillation = false;

    [Header("═══════════════════════════════")]
    [Header("Velocidad")]
    [Header("═══════════════════════════════")]

    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private MovementDirection mainDirection = MovementDirection.Right;
    [SerializeField] private Vector2 customDirection = Vector2.right;
    [SerializeField] private bool randomizeDirection = false;
    [SerializeField] private bool useBPM = false;
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float unitsPerBeat = 4f;
    [SerializeField] private bool randomizeSpeed = false;
    [SerializeField] private Vector2 speedRange = new Vector2(3f, 8f);
    [SerializeField] private bool useTimeSteps = false;
    [SerializeField] private float stepIntervalSeconds = 0.2f;
    [SerializeField] private float stepDistanceUnits = 1f;
    [SerializeField] private bool useStepTween = false;
    [SerializeField] private Ease stepTweenEase = Ease.Linear;

    [Header("Aceleración")]
    [SerializeField] private bool enableAcceleration = false;
    [SerializeField] private float accelerationRate = 1f; // unidades/s²
    [SerializeField] private float maxSpeed = 15f;

    [Header("═══════════════════════════════")]
    [Header("EFECTO BOOMERANG")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool enableBoomerang = false;
    [SerializeField] private float boomerangDistance = 10f;
    [SerializeField] private float returnSpeedMultiplier = 1.2f;
    [SerializeField] private bool destroyOnReturn = true;

    [Header("═══════════════════════════════")]
    [Header("DOTWEEN (Movimiento y Oscilación)")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool useDOTweenMovement = false;
    [SerializeField] private Ease easeType = Ease.Linear;
    [SerializeField] private float tweenDuration = 2f;
    [SerializeField] private bool useBPMForTween = false;
    [SerializeField] private float tweenBeats = 4f;
    [SerializeField] private Vector2 tweenTargetOffset = new Vector2(10f, 0f);
    [SerializeField] private bool destroyOnTweenComplete = true;
    [SerializeField] private bool useTweenForMovement = false;
    [SerializeField] private float movementTweenDistance = 2f;
    [SerializeField] private float movementTweenDuration = 0.4f;
    [SerializeField] private Ease movementTweenEase = Ease.InOutSine;
    [SerializeField] private bool useTweenForOscillation = false;
    [SerializeField] private float oscillationTweenDuration = 0.5f;
    [SerializeField] private Ease oscillationTweenEase = Ease.InOutSine;
    [Header("MOVIMIENTO - ACTIVACIÓN")]
    [SerializeField] private bool useMoveActivationTimer = false;
    [SerializeField] private float moveInterval = 0.5f;
    [SerializeField] private float moveActiveDuration = 0.2f;
    [SerializeField] private bool fadeBeforeMoveActivation = false;
    [SerializeField] private float movePreFadeLead = 0.1f;
    [Header("OSCILACIÓN - ACTIVACIÓN")]
    [SerializeField] private bool useOscActivationTimer = false;
    [SerializeField] private float oscInterval = 5f;
    [SerializeField] private float oscActiveDuration = 0.6f;
    [SerializeField] private bool fadeBeforeOscActivation = false;
    [SerializeField] private float oscPreFadeLead = 0.2f;
    [SerializeField] private bool oscUseVerticalAxis = true;
    [SerializeField] private bool fadeAfterActivation = false;
    [SerializeField] private float postFadeAlpha = 0.6f;
    [SerializeField] private float postFadeDuration = 0.3f;

    [Header("═══════════════════════════════")]
    [Header("ROTACIÓN")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool rotateTowardsMovement = true;
    [SerializeField] private bool enableSpin = false;
    [SerializeField] private float spinSpeed = 360f;

    [Header("═══════════════════════════════")]
    [Header("MOVIMIENTO ESPIRAL")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool enableSpiral = false;
    [SerializeField] private SpiralType spiralType = SpiralType.Expanding;

    public enum SpiralType
    {
        Expanding,   // Se aleja del centro en espiral
        Contracting, // Se acerca al centro en espiral
        Orbit        // Órbita circular (espiral con radio constante)
    }

    [SerializeField] private float spiralRadius = 5f;
    [SerializeField] private float spiralSpeed = 2f; // Velocidad angular
    [SerializeField] private float spiralGrowthRate = 0.5f; // Qué tan rápido crece/decrece
    [SerializeField] private Vector2 spiralCenter = Vector2.zero;
    [SerializeField] private bool useSelfAsCenter = true;

    [Header("═══════════════════════════════")]
    [Header("REBOTE (BOUNCE)")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool enableBounce = false;
    [SerializeField] private BounceMode bounceMode = BounceMode.Walls;

    public enum BounceMode
    {
        Walls,      // Rebota en paredes (pantalla)
        Objects,    // Rebota en objetos con Layer específico
        Both        // Rebota en ambos
    }

    [SerializeField] private LayerMask bounceLayer;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float bounceSpeedMultiplier = 0.9f; // Pierde velocidad al rebotar
    [SerializeField] private bool destroyAfterMaxBounces = true;

    [Header("═══════════════════════════════")]
    [Header("DIVISIÓN (SPLIT)")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool enableSplit = false;
    [SerializeField] private GameObject splitProjectilePrefab;
    [SerializeField] private SplitTrigger splitTrigger = SplitTrigger.Distance;

    public enum SplitTrigger
    {
        Distance,    // Se divide al recorrer cierta distancia
        Time,        // Se divide después de cierto tiempo
        OnHit,       // Se divide al impactar algo
        Manual       // Se divide al llamar Split() manualmente
    }

    [SerializeField] private float splitDistance = 5f;
    [SerializeField] private float splitTime = 2f;
    [SerializeField] private bool useBPMForSplit = false;
    [SerializeField] private float splitBeats = 2f;
    [SerializeField] private int splitCount = 3; // Cuántos proyectiles genera
    [SerializeField] private float splitAngleSpread = 60f; // Ángulo de dispersión
    [SerializeField] private bool destroyOnSplit = true;

    [Header("═══════════════════════════════")]
    [Header("HOMING (PERSECUCIÓN)")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool enableHoming = false;
    [SerializeField] private float homingStrength = 2f; // Qué tan fuerte gira hacia el jugador
    [SerializeField] private float homingDelay = 0.5f; // Delay antes de empezar a perseguir
    [SerializeField] private float homingDuration = 5f; // Cuánto tiempo persigue
    [SerializeField] private bool useBPMForHoming = false;
    [SerializeField] private float homingDelayBeats = 1f;
    [SerializeField] private float homingDurationBeats = 4f;

    [Header("═══════════════════════════════")]
    [Header("VIDA ÚTIL")]
    [Header("═══════════════════════════════")]

    [SerializeField] private float lifetime = 10f;
    [SerializeField] private bool destroyOffScreen = true;
    [SerializeField] private bool autoDestroyByLifetime = true;
    [SerializeField] private bool wrapScreenEdges = false;
    [SerializeField] private float wrapMargin = 0.1f;
    [SerializeField] private bool spawnOffscreen = false;
    [SerializeField] private float entranceDuration = 0.6f;
    [SerializeField] private Ease entranceEase = Ease.OutQuad;
    [SerializeField] private bool fadeInOnSpawn = false;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float entranceMargin = 0.5f;

    // ═══════════════════════════════════════════════════
    // VARIABLES INTERNAS
    // ═══════════════════════════════════════════════════

    private Vector2 moveDirection;
    private Vector2 startPosition;
    private float currentSpeed;
    private bool isReturning = false;
    private Transform playerTransform;
    private Tween activeTween;
    private float oscillationPhase;
    private float stepTimer;
    private Tween movementTween;
    private Tween oscillationTween;
    private Tween stepTween;
    private bool entranceActive;
    private SpriteRenderer sr;
    private float moveActivationTimer;
    private bool moveActive;
    private float moveActiveEndTime;
    private bool movePreFaded;
    private float oscActivationTimer;
    private bool oscActive;
    private float oscActiveEndTime;
    private bool oscPreFaded;
    [SerializeField] private bool firstMoveActivationImmediate = false;
    [SerializeField] private bool firstOscActivationImmediate = false;

    // Spiral
    private float spiralAngle = 0f;
    private float currentSpiralRadius;
    private Vector2 actualSpiralCenter;

    // Bounce
    private int bounceCount = 0;
    private Camera mainCamera;

    // Split
    private bool hasSplit = false;
    private float splitTimer = 0f;

    // Homing
    private float homingTimer = 0f;
    private bool isHoming;

    // ═══════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ═══════════════════════════════════════════════════

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        startPosition = transform.position;
        mainCamera = Camera.main;
        sr = GetComponent<SpriteRenderer>();

        // Buscar jugador si es necesario
        if (mainDirection == MovementDirection.TowardsPlayer || enableHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        // Configurar dirección
        SetupDirection();

        // Configurar velocidad
        SetupSpeed();

        // Configurar oscilación
        SetupOscillation();

        // Configurar espiral
        if (enableSpiral)
        {
            SetupSpiral();
        }

        // Configurar movimiento con DOTween
        if (useDOTweenMovement)
        {
            SetupDOTweenMovement();
        }

        if (spawnOffscreen || fadeInOnSpawn)
        {
            SetupEntrance();
        }
        else
        {
            if (!useMoveActivationTimer) StartTweenMovementIfEnabled();
            if (!useOscActivationTimer) StartTweenOscillationIfEnabled();
            if (useMoveActivationTimer && firstMoveActivationImmediate) StartMovementPulse();
            if (useOscActivationTimer && firstOscActivationImmediate) StartOscillationPulse();
        }

        if (autoDestroyByLifetime && lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    // ═══════════════════════════════════════════════════
    // SETUP
    // ═══════════════════════════════════════════════════

    private void SetupDirection()
    {
        if (randomizeDirection)
        {
            moveDirection = Random.insideUnitCircle.normalized;
            return;
        }

        switch (mainDirection)
        {
            case MovementDirection.Right:
                moveDirection = Vector2.right;
                break;
            case MovementDirection.Left:
                moveDirection = Vector2.left;
                break;
            case MovementDirection.Up:
                moveDirection = Vector2.up;
                break;
            case MovementDirection.Down:
                moveDirection = Vector2.down;
                break;
            case MovementDirection.TowardsPlayer:
                if (playerTransform != null)
                    moveDirection = (playerTransform.position - transform.position).normalized;
                else
                    moveDirection = Vector2.right;
                break;
            case MovementDirection.Custom:
                moveDirection = customDirection.normalized;
                break;
        }
    }

    private void SetupSpeed()
    {
        if (useBPM)
        {
            currentSpeed = unitsPerBeat * (bpm / 60f);
        }
        else if (randomizeSpeed)
        {
            currentSpeed = Random.Range(speedRange.x, speedRange.y);
        }
        else
        {
            currentSpeed = baseSpeed;
        }
    }

    private void SetupOscillation()
    {
        if (randomizeOscillation)
        {
            oscillationPhase = Random.Range(0f, Mathf.PI * 2f);
            oscillationAmplitude = Random.Range(oscillationAmplitude * 0.5f, oscillationAmplitude * 1.5f);
        }
    }

    private void SetupSpiral()
    {
        // Establecer centro de la espiral
        if (useSelfAsCenter)
        {
            actualSpiralCenter = transform.position;
        }
        else
        {
            actualSpiralCenter = spiralCenter;
        }

        // Radio inicial
        currentSpiralRadius = spiralRadius;

        // Ángulo inicial aleatorio
        spiralAngle = Random.Range(0f, Mathf.PI * 2f);
    }

    private void SetupDOTweenMovement()
    {
        Vector3 targetPos = transform.position + (Vector3)tweenTargetOffset;
        float duration = useBPMForTween ? (tweenBeats * (60f / bpm)) : tweenDuration;

        activeTween = transform.DOMove(targetPos, duration)
            .SetEase(easeType)
            .OnComplete(() => {
                if (enableBoomerang && !isReturning)
                {
                    ReturnToOrigin();
                }
                else if (destroyOnTweenComplete)
                {
                    Destroy(gameObject);
                }
            });
    }

    private void StartTweenMovementIfEnabled()
    {
        if (!useTweenForMovement) return;
        movementTween?.Kill();
        movementTween = transform.DOMove(transform.position + (Vector3)moveDirection * movementTweenDistance, movementTweenDuration)
            .SetEase(movementTweenEase)
            .SetLoops(-1, LoopType.Incremental);
    }

    private void StartTweenOscillationIfEnabled()
    {
        if (!useTweenForOscillation) return;
        oscillationTween?.Kill();
        Vector3 axis = Vector3.up;
        if (oscillationAxis == OscillationAxis.Horizontal) axis = Vector3.right;
        else if (oscillationAxis == OscillationAxis.Both) axis = Vector3.up;
        var t1 = transform.DOBlendableMoveBy(axis * oscillationAmplitude, oscillationTweenDuration).SetEase(oscillationTweenEase).SetLoops(-1, LoopType.Yoyo);
        if (oscillationAxis == OscillationAxis.Both)
        {
            var t2 = transform.DOBlendableMoveBy(Vector3.right * oscillationAmplitude, oscillationTweenDuration).SetEase(oscillationTweenEase).SetLoops(-1, LoopType.Yoyo);
            oscillationTween = t2;
        }
        else
        {
            oscillationTween = t1;
        }
    }

    private void StartMovementPulse()
    {
        moveActive = true;
        moveActiveEndTime = Time.time + moveActiveDuration;
        moveActivationTimer = 0f;
        movePreFaded = false;
        if (useTweenForMovement)
        {
            movementTween?.Kill();
            float dist = movementTweenDistance > 0f ? movementTweenDistance : (currentSpeed * moveActiveDuration);
            movementTween = transform.DOBlendableMoveBy((Vector3)moveDirection * dist, moveActiveDuration).SetEase(movementTweenEase);
        }
        else if (useTimeSteps)
        {
            // Fuerza un primer paso inmediato para evitar sensación de espera
            if (useStepTween)
            {
                stepTween?.Kill();
                stepTween = transform.DOBlendableMoveBy((Vector3)moveDirection * stepDistanceUnits, stepIntervalSeconds).SetEase(stepTweenEase);
            }
            else
            {
                transform.position += (Vector3)moveDirection * stepDistanceUnits;
            }
            stepTimer = 0f;
        }
    }

    private void StopMovementPulse()
    {
        moveActive = false;
        movementTween?.Kill();
        if (fadeAfterActivation && sr != null)
        {
            sr.DOFade(postFadeAlpha, postFadeDuration);
        }
    }

    private void StartOscillationPulse()
    {
        oscActive = true;
        oscActiveEndTime = Time.time + oscActiveDuration;
        oscActivationTimer = 0f;
        oscPreFaded = false;
        if (useTweenForOscillation)
        {
            oscillationTween?.Kill();
            Vector3 axis = oscUseVerticalAxis ? Vector3.up : Vector3.right;
            Sequence s = DOTween.Sequence();
            s.Append(transform.DOBlendableMoveBy(axis * oscillationAmplitude, oscActiveDuration * 0.5f).SetEase(oscillationTweenEase));
            s.Append(transform.DOBlendableMoveBy(-axis * oscillationAmplitude, oscActiveDuration * 0.5f).SetEase(oscillationTweenEase));
            oscillationTween = s;
        }
    }

    private void StopOscillationPulse()
    {
        oscActive = false;
        oscillationTween?.Kill();
        if (fadeAfterActivation && sr != null)
        {
            sr.DOFade(postFadeAlpha, postFadeDuration);
        }
    }

    private void SetupEntrance()
    {
        entranceActive = true;
        Vector3 finalPos = transform.position;
        if (spawnOffscreen && mainCamera != null)
        {
            float halfH = mainCamera.orthographicSize;
            float halfW = halfH * mainCamera.aspect;
            Vector3 camPos = mainCamera.transform.position;
            Vector3 off = finalPos;
            Vector2 dir = moveDirection == Vector2.zero ? Vector2.right : moveDirection;
            if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            {
                float x = dir.x > 0 ? camPos.x - halfW - entranceMargin : camPos.x + halfW + entranceMargin;
                off = new Vector3(x, finalPos.y, finalPos.z);
            }
            else
            {
                float y = dir.y > 0 ? camPos.y - halfH - entranceMargin : camPos.y + halfH + entranceMargin;
                off = new Vector3(finalPos.x, y, finalPos.z);
            }
            transform.position = off;
        }
        if (fadeInOnSpawn && sr != null)
        {
            Color c = sr.color; c.a = 0f; sr.color = c;
            sr.DOFade(1f, fadeInDuration);
        }
        transform.DOMove(finalPos, entranceDuration)
            .SetEase(entranceEase)
            .OnComplete(() => {
                entranceActive = false;
                if (!useMoveActivationTimer) StartTweenMovementIfEnabled();
                if (!useOscActivationTimer) StartTweenOscillationIfEnabled();
            });
    }

    // ═══════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════

    private void Update()
    {
        if (entranceActive) return;
        // Si usa DOTween, no mover manualmente
        if (useDOTweenMovement) return;

        // Timers de activación de movimiento
        if (useMoveActivationTimer)
        {
            moveActivationTimer += Time.deltaTime;
            if (!moveActive)
            {
                if (fadeBeforeMoveActivation && !movePreFaded && sr != null && moveActivationTimer >= Mathf.Max(0f, moveInterval - movePreFadeLead))
                {
                    sr.DOFade(1f, movePreFadeLead);
                    movePreFaded = true;
                }
                if (moveActivationTimer >= moveInterval)
                {
                    StartMovementPulse();
                }
            }
            else if (Time.time >= moveActiveEndTime)
            {
                StopMovementPulse();
            }
        }

        // Timers de activación de oscilación
        if (useOscActivationTimer)
        {
            oscActivationTimer += Time.deltaTime;
            if (!oscActive)
            {
                if (fadeBeforeOscActivation && !oscPreFaded && sr != null && oscActivationTimer >= Mathf.Max(0f, oscInterval - oscPreFadeLead))
                {
                    sr.DOFade(1f, oscPreFadeLead);
                    oscPreFaded = true;
                }
                if (oscActivationTimer >= oscInterval)
                {
                    StartOscillationPulse();
                }
            }
            else if (Time.time >= oscActiveEndTime)
            {
                StopOscillationPulse();
            }
        }

        // Movimiento espiral (prioridad sobre movimiento normal)
        if (enableSpiral)
        {
            MoveSpiralProjectile();
        }
        else
        {
            // Movimiento principal
            if (useTimeSteps)
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepIntervalSeconds)
                {
                    stepTimer = 0f;
                    if (!useMoveActivationTimer || moveActive)
                    {
                        if (useStepTween)
                        {
                            stepTween?.Kill();
                            stepTween = transform.DOBlendableMoveBy((Vector3)moveDirection * stepDistanceUnits, stepIntervalSeconds).SetEase(stepTweenEase);
                        }
                        else
                        {
                            transform.position += (Vector3)moveDirection * stepDistanceUnits;
                        }
                    }
                }
            }
            else
            {
                if (!useTweenForMovement && (!useMoveActivationTimer || moveActive))
                    MoveProjectile();
            }

            // Aplicar oscilación
            if (enableOscillation && !useTweenForOscillation && (!useOscActivationTimer || oscActive))
            {
                ApplyOscillation();
            }
        }

        // Homing (persecución)
        if (enableHoming)
        {
            ApplyHoming();
        }

        // Aceleración
        if (enableAcceleration)
        {
            ApplyAcceleration();
        }

        // Verificar boomerang
        if (enableBoomerang && !isReturning)
        {
            CheckBoomerangDistance();
        }

        // Verificar rebote
        if (enableBounce)
        {
            CheckBounce();
        }

        // Verificar split
        if (enableSplit && !hasSplit)
        {
            CheckSplit();
        }

        // Rotación
        UpdateRotation();

        // Destruir si sale de pantalla
        if (destroyOffScreen)
        {
            CheckOffScreen();
        }
    }

    // ═══════════════════════════════════════════════════
    // MOVIMIENTO
    // ═══════════════════════════════════════════════════

    private void MoveProjectile()
    {
        transform.position += (Vector3)moveDirection * currentSpeed * Time.deltaTime;
    }

    private void ApplyOscillation()
    {
        float freq = useBPM ? ((bpm / 60f) * Mathf.Max(0.0001f, oscillationFrequency)) : oscillationFrequency;
        float oscillation = Mathf.Sin(Time.time * freq + oscillationPhase) * oscillationAmplitude;

        Vector3 offset = Vector3.zero;

        switch (oscillationAxis)
        {
            case OscillationAxis.Vertical:
                offset = Vector3.up * oscillation * Time.deltaTime;
                break;
            case OscillationAxis.Horizontal:
                offset = Vector3.right * oscillation * Time.deltaTime;
                break;
            case OscillationAxis.Both:
                offset = new Vector3(
                    Mathf.Cos(Time.time * freq) * oscillationAmplitude * Time.deltaTime,
                    oscillation * Time.deltaTime,
                    0
                );
                break;
        }

        transform.position += offset;
    }

    private void ApplyAcceleration()
    {
        currentSpeed += accelerationRate * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
    }

    // ═══════════════════════════════════════════════════
    // MOVIMIENTO ESPIRAL
    // ═══════════════════════════════════════════════════

    private void MoveSpiralProjectile()
    {
        // Incrementar ángulo (velocidad angular)
        spiralAngle += spiralSpeed * Time.deltaTime;

        // Ajustar radio según tipo
        switch (spiralType)
        {
            case SpiralType.Expanding:
                currentSpiralRadius += spiralGrowthRate * Time.deltaTime;
                break;
            case SpiralType.Contracting:
                currentSpiralRadius -= spiralGrowthRate * Time.deltaTime;
                currentSpiralRadius = Mathf.Max(0.1f, currentSpiralRadius);

                // Destruir si llega al centro
                if (currentSpiralRadius <= 0.2f)
                {
                    Destroy(gameObject);
                    return;
                }
                break;
            case SpiralType.Orbit:
                // Radio constante (órbita circular)
                break;
        }

        // Calcular posición en espiral
        float x = actualSpiralCenter.x + Mathf.Cos(spiralAngle) * currentSpiralRadius;
        float y = actualSpiralCenter.y + Mathf.Sin(spiralAngle) * currentSpiralRadius;

        transform.position = new Vector3(x, y, transform.position.z);

        // Actualizar dirección de movimiento para rotación
        Vector2 tangent = new Vector2(-Mathf.Sin(spiralAngle), Mathf.Cos(spiralAngle));
        moveDirection = tangent.normalized;
    }

    // ═══════════════════════════════════════════════════
    // REBOTE (BOUNCE)
    // ═══════════════════════════════════════════════════

    private void CheckBounce()
    {
        bool bounced = false;

        // Rebote en paredes (pantalla)
        if (bounceMode == BounceMode.Walls || bounceMode == BounceMode.Both)
        {
            if (CheckScreenBounds())
            {
                bounced = true;
            }
        }

        // Rebote en objetos
        if (bounceMode == BounceMode.Objects || bounceMode == BounceMode.Both)
        {
            if (CheckObjectBounce())
            {
                bounced = true;
            }
        }

        if (bounced)
        {
            OnBounce();
        }
    }

    private bool CheckScreenBounds()
    {
        Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);
        bool bounced = false;

        // Rebote horizontal
        if (screenPos.x <= 0f || screenPos.x >= 1f)
        {
            moveDirection.x = -moveDirection.x;
            bounced = true;
        }

        // Rebote vertical
        if (screenPos.y <= 0f || screenPos.y >= 1f)
        {
            moveDirection.y = -moveDirection.y;
            bounced = true;
        }

        return bounced;
    }

    private bool CheckObjectBounce()
    {
        // Raycast en dirección de movimiento
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            moveDirection,
            currentSpeed * Time.deltaTime + 0.5f,
            bounceLayer
        );

        if (hit.collider != null)
        {
            // Reflejar dirección según normal de superficie
            moveDirection = Vector2.Reflect(moveDirection, hit.normal);
            return true;
        }

        return false;
    }

    private void OnBounce()
    {
        bounceCount++;

        // Reducir velocidad
        currentSpeed *= bounceSpeedMultiplier;

        // Destruir si alcanzó máximo de rebotes
        if (bounceCount >= maxBounces && destroyAfterMaxBounces)
        {
            Destroy(gameObject);
        }
    }

    // ═══════════════════════════════════════════════════
    // DIVISIÓN (SPLIT)
    // ═══════════════════════════════════════════════════

    private void CheckSplit()
    {
        bool shouldSplit = false;

        switch (splitTrigger)
        {
            case SplitTrigger.Distance:
                float distanceTraveled = Vector2.Distance(startPosition, transform.position);
                if (distanceTraveled >= splitDistance)
                {
                    shouldSplit = true;
                }
                break;

            case SplitTrigger.Time:
                splitTimer += Time.deltaTime;
                float splitThreshold = useBPMForSplit ? (splitBeats * (60f / bpm)) : splitTime;
                if (splitTimer >= splitThreshold)
                {
                    shouldSplit = true;
                }
                break;

            case SplitTrigger.OnHit:
                // Se maneja desde OnTriggerEnter2D
                break;

            case SplitTrigger.Manual:
                // Se llama manualmente con Split()
                break;
        }

        if (shouldSplit)
        {
            Split();
        }
    }

    public void Split()
    {
        if (hasSplit || splitProjectilePrefab == null) return;

        hasSplit = true;

        // Calcular ángulos de dispersión
        float baseAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        float angleStep = splitAngleSpread / (splitCount - 1);
        float startAngle = baseAngle - (splitAngleSpread / 2f);

        // Spawnear proyectiles divididos
        for (int i = 0; i < splitCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject split = Instantiate(splitProjectilePrefab, transform.position, Quaternion.identity);

            // Configurar dirección del hijo
            UniversalProjectileMover splitMover = split.GetComponent<UniversalProjectileMover>();
            if (splitMover != null)
            {
                splitMover.customDirection = direction;
                splitMover.mainDirection = MovementDirection.Custom;
                splitMover.baseSpeed = currentSpeed * 0.8f; // Ligeramente más lento
                splitMover.Initialize();
            }
            else
            {
                // Fallback: usar Rigidbody2D
                Rigidbody2D rb = split.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * currentSpeed;
                }
            }
        }

        // Destruir proyectil original
        if (destroyOnSplit)
        {
            Destroy(gameObject);
        }
    }

    public void SetCustomDirection(Vector2 dir)
    {
        mainDirection = MovementDirection.Custom;
        customDirection = dir;
        moveDirection = dir.normalized;
    }

    // ═══════════════════════════════════════════════════
    // HOMING (PERSECUCIÓN)
    // ═══════════════════════════════════════════════════

    private void ApplyHoming()
    {
        if (playerTransform == null) return;

        // Esperar delay antes de empezar
        homingTimer += Time.deltaTime;
        float delay = useBPMForHoming ? (homingDelayBeats * (60f / bpm)) : homingDelay;
        if (homingTimer < delay)
            return;

        // Verificar si terminó la duración
        float duration = useBPMForHoming ? (homingDurationBeats * (60f / bpm)) : homingDuration;
        if (homingTimer > delay + duration)
        {
            enableHoming = false;
            return;
        }

        // Calcular dirección hacia jugador
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

        // Interpolar suavemente hacia esa dirección
        moveDirection = Vector2.Lerp(
            moveDirection,
            directionToPlayer,
            homingStrength * Time.deltaTime
        ).normalized;
    }

    // ═══════════════════════════════════════════════════
    // BOOMERANG
    // ═══════════════════════════════════════════════════

    private void CheckBoomerangDistance()
    {
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);

        if (distanceTraveled >= boomerangDistance)
        {
            ReturnToOrigin();
        }
    }

    private void ReturnToOrigin()
    {
        isReturning = true;

        if (useDOTweenMovement)
        {
            if (activeTween != null)
            {
                activeTween.Kill();
            }

            float duration = useBPMForTween ? (tweenBeats * (60f / bpm) * 0.8f) : (tweenDuration * 0.8f);
            activeTween = transform.DOMove(startPosition, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => {
                    if (destroyOnReturn)
                        Destroy(gameObject);
                });
        }
        else
        {
            moveDirection = (startPosition - (Vector2)transform.position).normalized;
            currentSpeed *= returnSpeedMultiplier;

            if (destroyOnReturn)
            {
                StartCoroutine(DestroyOnReachOrigin());
            }
        }
    }

    private IEnumerator DestroyOnReachOrigin()
    {
        while (Vector2.Distance(transform.position, startPosition) > 0.5f)
        {
            yield return null;
        }
        Destroy(gameObject);
    }

    // ═══════════════════════════════════════════════════
    // ROTACIÓN
    // ═══════════════════════════════════════════════════

    private void UpdateRotation()
    {
        if (rotateTowardsMovement && !enableSpin)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        if (enableSpin)
        {
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        }
    }

    // ═══════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════

    private void CheckOffScreen()
    {
        if (enableSpiral && spiralType == SpiralType.Orbit)
            return;

        var cam = Camera.main;
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float leftX = cam.transform.position.x - halfWidth;
        float rightX = cam.transform.position.x + halfWidth;
        float bottomY = cam.transform.position.y - halfHeight;
        float topY = cam.transform.position.y + halfHeight;

        Vector3 pos = transform.position;
        bool offX = pos.x < leftX - wrapMargin || pos.x > rightX + wrapMargin;
        bool offY = pos.y < bottomY - wrapMargin || pos.y > topY + wrapMargin;

        if (!wrapScreenEdges)
        {
            if (offX || offY)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (pos.x < leftX - wrapMargin) pos.x = rightX + wrapMargin;
            else if (pos.x > rightX + wrapMargin) pos.x = leftX - wrapMargin;

            if (pos.y < bottomY - wrapMargin) pos.y = topY + wrapMargin;
            else if (pos.y > topY + wrapMargin) pos.y = bottomY - wrapMargin;

            transform.position = pos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Split al impactar
        if (enableSplit && !hasSplit && splitTrigger == SplitTrigger.OnHit)
        {
            // Verificar si impactó con algo relevante (no player)
            if (!collision.CompareTag("Player"))
            {
                Split();
            }
        }
    }

    // ═══════════════════════════════════════════════════
    // MÉTODOS PÚBLICOS (para control externo)
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Cambiar dirección en runtime
    /// </summary>
    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }

    /// <summary>
    /// Cambiar velocidad en runtime
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    public void SetMainDirection(MovementDirection direction)
    {
        mainDirection = direction;
    }


    public void SetBaseSpeed(float speed)
    {
        baseSpeed = speed;
    }

    /// <summary>
    /// Activar boomerang manualmente
    /// </summary>
    public void TriggerBoomerang()
    {
        if (!isReturning)
        {
            ReturnToOrigin();
        }
    }

    /// <summary>
    /// Dividir proyectil manualmente
    /// </summary>
    public void TriggerSplit()
    {
        Split();
    }

    /// <summary>
    /// Cambiar centro de espiral en runtime
    /// </summary>
    public void SetSpiralCenter(Vector2 newCenter)
    {
        actualSpiralCenter = newCenter;
        useSelfAsCenter = false;
    }

    /// <summary>
    /// Pausar movimiento
    /// </summary>
    public void Pause()
    {
        enabled = false;
        if (activeTween != null)
        {
            activeTween.Pause();
        }
    }

    /// <summary>
    /// Reanudar movimiento
    /// </summary>
    public void Resume()
    {
        enabled = true;
        if (activeTween != null)
        {
            activeTween.Play();
        }
    }

    // ═══════════════════════════════════════════════════
    // GIZMOS
    // ═══════════════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Dirección principal
            Gizmos.color = Color.cyan;
            Vector2 dir = GetDirectionForGizmo();
            Gizmos.DrawRay(transform.position, dir * 3f);

            // Rango boomerang
            if (enableBoomerang)
            {
                Gizmos.color = new Color(1, 1, 0, 0.3f);
                Gizmos.DrawWireSphere(transform.position, boomerangDistance);
            }

            // Oscilación
            if (enableOscillation)
            {
                Gizmos.color = Color.green;
                Vector3 perpendicular = Vector3.up;
                if (oscillationAxis == OscillationAxis.Horizontal)
                    perpendicular = Vector3.right;

                Gizmos.DrawLine(
                    transform.position + perpendicular * oscillationAmplitude,
                    transform.position - perpendicular * oscillationAmplitude
                );
            }

            // Espiral
            if (enableSpiral)
            {
                Gizmos.color = Color.magenta;
                Vector2 center = useSelfAsCenter ? (Vector2)transform.position : spiralCenter;

                // Dibujar espiral aproximada
                int segments = 50;
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / segments;
                    float angle = t * Mathf.PI * 4; // 2 vueltas
                    float radius = spiralRadius * (spiralType == SpiralType.Expanding ? t : 1f - t);

                    Vector2 point = center + new Vector2(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius
                    );

                    Gizmos.DrawWireSphere(point, 0.1f);
                }
            }

            // Split
            if (enableSplit && splitTrigger == SplitTrigger.Distance)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, splitDistance);
            }
        }
    }

    private Vector2 GetDirectionForGizmo()
    {
        switch (mainDirection)
        {
            case MovementDirection.Right: return Vector2.right;
            case MovementDirection.Left: return Vector2.left;
            case MovementDirection.Up: return Vector2.up;
            case MovementDirection.Down: return Vector2.down;
            case MovementDirection.Custom: return customDirection.normalized;
            default: return Vector2.right;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════
// EJEMPLOS DE USO
// ═══════════════════════════════════════════════════════════════════

// 1. HUESO QUE AVANZA EN X Y OSCILA EN Y (Fase 2):
// - Main Direction: Right
// - Enable Oscillation: true
// - Oscillation Axis: Vertical
// - Base Speed: 5

// 2. PROYECTIL BOOMERANG (Fase 7):
// - Main Direction: Towards Player
// - Enable Boomerang: true
// - Boomerang Distance: 8
// - Return Speed Multiplier: 1.5

// 3. LÁSER RÁPIDO CON DOTWEEN (Fase 4):
// - Use DOTween Movement: true
// - Ease Type: InOutQuad
// - Tween Duration: 0.5
// - Tween Target Offset: (0, -10)

// 4. ATAQUE CIRCULAR (SubBoss):
// - Main Direction: Custom
// - Enable Oscillation: true
// - Oscillation Axis: Both
// - Enable Acceleration: true

// 5. DIENTE QUE VA Y VUELVE (Fase 7):
// - Main Direction: Custom (hacia centro)
// - Enable Boomerang: true
// - Destroy On Return: false
// - Use DOTween: true, Ease: InOutBack
