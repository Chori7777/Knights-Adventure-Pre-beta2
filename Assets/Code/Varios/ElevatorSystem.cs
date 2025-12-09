using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ElevatorSystem : MonoBehaviour
{
    [Header("Puntos de Movimiento")]
    [SerializeField] private Transform pointA; // Punto A
    [SerializeField] private Transform pointB; // Punto B
    [SerializeField] private bool startAtPointA = true;

    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private float waitTimeAtFloor = 1f;

    [Header("Activación")]
    [SerializeField] private bool moveOnPlayerEnter = true; // Activar movimiento al subir el jugador
    [SerializeField] private bool canCallWithButton = true; // Puede llamarse con boton
    [SerializeField] private LayerMask playerLayer; // Layer del jugador
    [SerializeField] private bool autoLoop = false;
    [SerializeField] private bool useBPM = false;
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float unitsPerBeat = 4f;
    [SerializeField] private float beatsWaitAtFloor = 2f;
    [SerializeField] private bool useTimeSteps = false;
    [SerializeField] private float stepIntervalSeconds = 0.2f;
    [SerializeField] private float stepDistanceUnits = 0.5f;
    [SerializeField] private bool stickPlayerToPlatform = true;
    [SerializeField] private float stickYOffsetThreshold = 0.15f;
    [SerializeField] private bool teleportToAOnArriveB = false;
    [SerializeField] private bool teleportLoopSMB = false;

    [Header("Auto retorno")]
    [SerializeField] private bool autoReturnToAWhenEmpty = true;
    [SerializeField] private float autoReturnDelay = 1f;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip arriveSound;

    private AudioSource audioSource;

    private bool isAtPointA = true;
    private bool isMoving = false;
    private bool playerOnElevator = false;
    private Tween currentTween;

    private Transform playerTransform;
    private Rigidbody2D rb;
    [Header("Objeto a mover")]
    [SerializeField] private Transform platformRoot;
    [Header("Espacio de referencia")]
    [SerializeField] private bool lockToParentSpace = true;
    private Vector3 localPointA;
    private Vector3 localPointB;
    private Vector3 lastPlatformPosition;
    private bool autoReturnScheduled = false;

    private void Start()
    {
        if (platformRoot == null) platformRoot = transform;
        rb = platformRoot.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        if (lockToParentSpace)
        {
            if (pointA != null)
            {
                localPointA = (platformRoot.parent != null)
                    ? platformRoot.parent.InverseTransformPoint(pointA.position)
                    : pointA.position;
            }
            if (pointB != null)
            {
                localPointB = (platformRoot.parent != null)
                    ? platformRoot.parent.InverseTransformPoint(pointB.position)
                    : pointB.position;
            }
        }
        // Posicionar ascensor en punto inicial
        if (startAtPointA && (pointA != null || lockToParentSpace))
        {
            Vector3 worldA = lockToParentSpace
                ? (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointA) : localPointA)
                : pointA.position;
            platformRoot.position = worldA;
            isAtPointA = true;
        }
        else if (!startAtPointA && (pointB != null || lockToParentSpace))
        {
            Vector3 worldB = lockToParentSpace
                ? (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointB) : localPointB)
                : pointB.position;
            platformRoot.position = worldB;
            isAtPointA = false;
        }

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (moveSound != null || arriveSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (autoLoop && ((pointA != null && pointB != null) || lockToParentSpace))
        {
            Vector3 worldA = lockToParentSpace
                ? (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointA) : localPointA)
                : pointA.position;
            Vector3 worldB = lockToParentSpace
                ? (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointB) : localPointB)
                : pointB.position;
            if (teleportLoopSMB)
            {
                StartCoroutine(AutoTeleportLoopCoroutine(worldA, worldB));
            }
            else
            {
                if (!useTimeSteps)
                {
                    float speed = useBPM ? (unitsPerBeat * (bpm / 60f)) : moveSpeed;
                    float durationAB = Vector3.Distance(platformRoot.position, worldB) / speed;
                    float durationBA = Vector3.Distance(worldB, worldA) / speed;
                    float waitInterval = useBPM ? (beatsWaitAtFloor * (60f / bpm)) : waitTimeAtFloor;

                    Sequence seq = DOTween.Sequence();
                    if (rb != null)
                        seq.Append(rb.DOMove((Vector2)worldB, durationAB).SetEase(easeType));
                    else
                        seq.Append(platformRoot.DOMove(worldB, durationAB).SetEase(easeType));
                    seq.AppendInterval(waitInterval);
                    if (rb != null)
                        seq.Append(rb.DOMove((Vector2)worldA, durationBA).SetEase(easeType));
                    else
                        seq.Append(platformRoot.DOMove(worldA, durationBA).SetEase(easeType));
                    seq.AppendInterval(waitInterval);
                    seq.SetLoops(-1);
                }
                else
                {
                    StartCoroutine(AutoLoopStepCoroutine(worldA, worldB));
                }
            }
        }

        lastPlatformPosition = platformRoot.position;

        // Si arrancamos en B y esta vacio, programar retorno automatico
        TryScheduleAutoReturn();
    }

    private void Update()
    {
        Vector3 current = platformRoot.position;
        Vector3 delta = current - lastPlatformPosition;
        if (isMoving && playerOnElevator && playerTransform != null && !stickPlayerToPlatform)
        {
            playerTransform.position += new Vector3(-delta.x, 0f, 0f);
        }
        lastPlatformPosition = current;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            playerOnElevator = true;
            playerTransform = collision.transform;
            if (stickPlayerToPlatform && playerTransform != null)
            {
                // Solo parentear si el jugador esta sobre la plataforma (Y mayor o igual)
                if (playerTransform.position.y >= platformRoot.position.y - stickYOffsetThreshold)
                {
                    playerTransform.SetParent(platformRoot);
                }
            }

            if (moveOnPlayerEnter && !isMoving)
                MoveToOppositeFloor();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;
        if (!stickPlayerToPlatform) return;
        if (collision.transform == null) return;
        if (playerTransform == null) playerTransform = collision.transform;
        // Asegurar pegado mientras este sobre la plataforma
        if (playerTransform.position.y >= platformRoot.position.y - stickYOffsetThreshold)
        {
            if (playerTransform.parent != platformRoot)
            {
                playerTransform.SetParent(platformRoot);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform == playerTransform)
        {
            playerOnElevator = false;
            if (playerTransform != null)
            {
                var t = playerTransform;
                if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
                {
                    if (t != null && t.parent == platformRoot)
                        t.SetParent(null);
                }
                else
                {
                    StartCoroutine(UnparentDeferred(t));
                }
            }
            playerTransform = null;

            // Si se baja el jugador y estamos en B, programar retorno
            TryScheduleAutoReturn();
        }
    }

    public void MoveToOppositeFloor()
    {
        if (isMoving) return;

        if (isAtPointA)
            MoveToPointB();
        else
            MoveToPointA();
    }

    public void MoveToPointA()
    {
        if (isMoving || isAtPointA) return;
        StartMove(pointA.position, true);
    }

    public void MoveToPointB()
    {
        if (isMoving || !isAtPointA) return;
        StartMove(pointB.position, false);
    }

    private void StartMove(Vector3 targetPosition, bool movingToA)
    {
        isMoving = true;

        Vector3 worldTarget = targetPosition;
        if (lockToParentSpace)
        {
            worldTarget = movingToA
                ? (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointA) : localPointA)
                : (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointB) : localPointB);
        }

        if (!useTimeSteps)
        {
            float distance = Vector3.Distance(platformRoot.position, worldTarget);
            float speed = useBPM ? (unitsPerBeat * (bpm / 60f)) : moveSpeed;
            float duration = distance / speed;

            if (audioSource != null && moveSound != null)
                audioSource.PlayOneShot(moveSound);
            if (currentTween != null)
            {
                currentTween.Kill();
                currentTween = null;
            }
            if (rb != null)
            {
                currentTween = rb.DOMove((Vector2)worldTarget, duration)
                    .SetEase(easeType)
                    .OnComplete(() => OnArriveAtFloor(movingToA));
            }
            else
            {
                currentTween = platformRoot.DOMove(worldTarget, duration)
                    .SetEase(easeType)
                    .OnComplete(() => OnArriveAtFloor(movingToA));
            }
        }
        else
        {
            if (audioSource != null && moveSound != null)
                audioSource.PlayOneShot(moveSound);
            if (currentTween != null)
            {
                currentTween.Kill();
                currentTween = null;
            }
            StartCoroutine(StepMoveCoroutine(worldTarget, movingToA));
        }
    }

    private void OnArriveAtFloor(bool arrivedAtA)
    {
        isMoving = false;
        isAtPointA = arrivedAtA;

        if (audioSource != null && arriveSound != null)
            audioSource.PlayOneShot(arriveSound);

        float waitInterval = useBPM ? (beatsWaitAtFloor * (60f / bpm)) : waitTimeAtFloor;

        if (!arrivedAtA && teleportToAOnArriveB)
        {
            Vector3 worldA = lockToParentSpace
                ? (platformRoot.parent != null ? platformRoot.parent.TransformPoint(localPointA) : localPointA)
                : (pointA != null ? pointA.position : platformRoot.position);

            if (currentTween != null)
            {
                currentTween.Kill();
                currentTween = null;
            }

            if (rb != null)
                rb.position = (Vector2)worldA;
            else
                platformRoot.position = worldA;

            lastPlatformPosition = platformRoot.position;
            isAtPointA = true;

            if (teleportLoopSMB && !autoLoop)
            {
                DOVirtual.DelayedCall(waitInterval, () =>
                {
                    if (!isMoving) MoveToPointB();
                });
            }
        }
        else
        {
            if (teleportLoopSMB && arrivedAtA && !autoLoop)
            {
                // Si es un loop tipo SMB pero no hay teleport en B, aseguramos movimiento A->B continuo
                DOVirtual.DelayedCall(waitInterval, () =>
                {
                    if (!isMoving) MoveToPointB();
                });
            }
            else
            {
                DOVirtual.DelayedCall(waitInterval, () => { });
            }
        }

        // Si llegamos a B y no hay jugador, programar retorno automatico
        if (!arrivedAtA)
        {
            TryScheduleAutoReturn();
        }
    }

    private IEnumerator StepMoveCoroutine(Vector3 target, bool movingToA)
    {
        isMoving = true;
        Vector3 dir = (target - platformRoot.position).normalized;
        while (Vector3.Distance(platformRoot.position, target) > 0.001f)
        {
            Vector3 next = platformRoot.position + dir * stepDistanceUnits;
            if (Vector3.Distance(next, target) < stepDistanceUnits)
                next = target;

            if (rb != null)
                rb.position = next;
            else
                platformRoot.position = next;

            yield return new WaitForSeconds(stepIntervalSeconds);
        }
        OnArriveAtFloor(movingToA);
    }

    private IEnumerator AutoLoopStepCoroutine(Vector3 worldA, Vector3 worldB)
    {
        while (true)
        {
            yield return StepMoveCoroutine(worldB, false);
            float waitInterval = useBPM ? (beatsWaitAtFloor * (60f / bpm)) : waitTimeAtFloor;
            yield return new WaitForSeconds(waitInterval);

            yield return StepMoveCoroutine(worldA, true);
            waitInterval = useBPM ? (beatsWaitAtFloor * (60f / bpm)) : waitTimeAtFloor;
            yield return new WaitForSeconds(waitInterval);
        }
    }

    private IEnumerator AutoTeleportLoopCoroutine(Vector3 worldA, Vector3 worldB)
    {
        while (true)
        {
            MoveToPointB();
            while (isMoving) yield return null;
            float waitInterval = useBPM ? (beatsWaitAtFloor * (60f / bpm)) : waitTimeAtFloor;
            yield return new WaitForSeconds(waitInterval);
        }
    }

    public void CallElevator(bool callToPointA)
    {
        if (!canCallWithButton || isMoving) return;

        if (callToPointA && isAtPointA) return;
        if (!callToPointA && !isAtPointA) return;

        if (callToPointA)
            MoveToPointA();
        else
            MoveToPointB();
    }

    private void TryScheduleAutoReturn()
    {
        if (!autoReturnToAWhenEmpty) return;
        if (isMoving) return;
        if (isAtPointA) return;
        if (playerOnElevator) return;
        if (autoReturnScheduled) return;
        autoReturnScheduled = true;
        DOVirtual.DelayedCall(autoReturnDelay, () =>
        {
            autoReturnScheduled = false;
            if (!playerOnElevator && !isMoving && !isAtPointA)
            {
                MoveToPointA();
            }
        });
    }

    private IEnumerator UnparentDeferred(Transform t)
    {
        yield return null;
        if (t != null && t.parent == platformRoot)
        {
            t.SetParent(null);
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA.position, 0.5f);
        Gizmos.DrawWireCube(pointA.position, Vector3.one * 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB.position, 0.5f);
        Gizmos.DrawWireCube(pointB.position, Vector3.one * 0.3f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    // Getters publicos
    public bool IsMoving => isMoving;
    public bool IsAtPointA => isAtPointA;
    public bool PlayerOnElevator => playerOnElevator;
}
