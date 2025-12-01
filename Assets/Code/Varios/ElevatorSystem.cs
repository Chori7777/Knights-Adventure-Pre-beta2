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
    [SerializeField] private bool canCallWithButton = true; // Puede llamarse con bot�n
    [SerializeField] private LayerMask playerLayer; // Layer del jugador
    [SerializeField] private bool autoLoop = false;

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

            float durationAB = Vector3.Distance(platformRoot.position, worldB) / moveSpeed;
            float durationBA = Vector3.Distance(worldB, worldA) / moveSpeed;

            Sequence seq = DOTween.Sequence();
            if (rb != null)
                seq.Append(rb.DOMove((Vector2)worldB, durationAB).SetEase(easeType));
            else
                seq.Append(platformRoot.DOMove(worldB, durationAB).SetEase(easeType));
            seq.AppendInterval(waitTimeAtFloor);
            if (rb != null)
                seq.Append(rb.DOMove((Vector2)worldA, durationBA).SetEase(easeType));
            else
                seq.Append(platformRoot.DOMove(worldA, durationBA).SetEase(easeType));
            seq.AppendInterval(waitTimeAtFloor);
            seq.SetLoops(-1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // El jugador est� encima
                {
                    playerOnElevator = true;
                    playerTransform = collision.transform;

                    // Poner al jugador como hijo del ascensor
                    if (playerTransform != null)
                        playerTransform.SetParent(platformRoot);

                    Debug.Log("Jugador subi� al ascensor");

                    // Mover autom�ticamente si est� configurado
                    if (moveOnPlayerEnter && !isMoving)
                        MoveToOppositeFloor();

                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform == playerTransform)
        {
            playerOnElevator = false;

            // Desparentar al jugador
            if (playerTransform != null)
                playerTransform.SetParent(null);

            playerTransform = null;
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

        float distance = Vector3.Distance(platformRoot.position, worldTarget);
        float duration = distance / moveSpeed;

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

    private void OnArriveAtFloor(bool arrivedAtA)
    {
        isMoving = false;
        isAtPointA = arrivedAtA;

        if (audioSource != null && arriveSound != null)
            audioSource.PlayOneShot(arriveSound);

        DOVirtual.DelayedCall(waitTimeAtFloor, () => { });
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

    // Getters p�blicos
    public bool IsMoving => isMoving;
    public bool IsAtPointA => isAtPointA;
    public bool PlayerOnElevator => playerOnElevator;
}
