using UnityEngine;
using DG.Tweening;

public class ElevatorSystem : MonoBehaviour
{
    [Header("Puntos de Movimiento")]
    [SerializeField] private Transform pointA; // Punto A
    [SerializeField] private Transform pointB; // Punto B
    [SerializeField] private bool startAtPointA = true;

    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f; // Velocidad en unidades/segundo
    [SerializeField] private Ease easeType = Ease.InOutSine; // Tipo de interpolación
    [SerializeField] private float waitTimeAtFloor = 1f; // Tiempo de espera en cada piso

    [Header("Activación")]
    [SerializeField] private bool moveOnPlayerEnter = true; // Activar movimiento al subir el jugador
    [SerializeField] private bool canCallWithButton = true; // Puede llamarse con botón
    [SerializeField] private LayerMask playerLayer; // Layer del jugador

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip arriveSound;

    private AudioSource audioSource;

    private bool isAtPointA = true;
    private bool isMoving = false;
    private bool playerOnElevator = false;
    private Tween currentTween;

    private Transform playerTransform;

    private void Start()
    {
        // Posicionar ascensor en punto inicial
        if (startAtPointA && pointA != null)
        {
            transform.position = pointA.position;
            isAtPointA = true;
        }
        else if (!startAtPointA && pointB != null)
        {
            transform.position = pointB.position;
            isAtPointA = false;
        }

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (moveSound != null || arriveSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // El jugador está encima
                {
                    playerOnElevator = true;
                    playerTransform = collision.transform;

                    // Poner al jugador como hijo del ascensor
                    if (playerTransform != null)
                        playerTransform.SetParent(transform);

                    Debug.Log("Jugador subió al ascensor");

                    // Mover automáticamente si está configurado
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

        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = distance / moveSpeed;

        if (audioSource != null && moveSound != null)
            audioSource.PlayOneShot(moveSound);

        currentTween = transform.DOMove(targetPosition, duration)
            .SetEase(easeType)
            .OnComplete(() => OnArriveAtFloor(movingToA));
    }

    private void OnArriveAtFloor(bool arrivedAtA)
    {
        isMoving = false;
        isAtPointA = arrivedAtA;

        if (audioSource != null && arriveSound != null)
            audioSource.PlayOneShot(arriveSound);

        // Espera opcional
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

    // Getters públicos
    public bool IsMoving => isMoving;
    public bool IsAtPointA => isAtPointA;
    public bool PlayerOnElevator => playerOnElevator;
}
