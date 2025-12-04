using UnityEngine;
using System.Collections;


public class EnemyFlying : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Patrulla Aérea")]
    [SerializeField] private Transform[] patrolPoints; // Puntos de patrulla
    [SerializeField] private float waypointReachDistance = 0.5f;
    [SerializeField] private float idleTimeAtWaypoint = 1f;

    [Header("Seguimiento")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private bool canChasePlayer = true;
    [SerializeField] private float chaseHeight = 2f; // Altura a mantener sobre el jugador

    [Header("Lugar de Reposo")]
    [SerializeField] private Transform restingSpot; // Punto de descanso
    [SerializeField] private float returnToRestTime = 5f; // Tiempo sin ver jugador para volver
    [SerializeField] private float restDuration = 3f; // Tiempo de descanso antes de volver a patrullar

    private EnemyCore core;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float timeSinceLastPlayerSeen = 0f;
    private bool isReturningToRest = false;
    private bool isResting = false; //  Flag de descanso
    private float restTimer = 0f; // Timer de descanso

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;

        // Si no hay punto de reposo, usar posición inicial
        if (restingSpot == null)
        {
            GameObject restObj = new GameObject("RestingSpot");
            restObj.transform.position = transform.position;
            restObj.transform.SetParent(transform.parent);
            restingSpot = restObj.transform;
        }

        // Si no hay puntos de patrulla, crear algunos alrededor del punto de reposo
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            CreateDefaultPatrolPoints();
        }

        // Desactivar gravedad para enemigos voladores
        if (core.rb != null)
        {
            core.rb.gravityScale = 0f;
        }
    }

    private void CreateDefaultPatrolPoints()
    {
        GameObject patrolParent = new GameObject($"{gameObject.name}_PatrolPoints");
        patrolParent.transform.position = restingSpot.position;
        patrolParent.transform.SetParent(transform.parent);

        patrolPoints = new Transform[4];
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(3, 2, 0),
            new Vector3(-3, 2, 0),
            new Vector3(-3, -2, 0),
            new Vector3(3, -2, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject($"PatrolPoint_{i}");
            point.transform.position = restingSpot.position + offsets[i];
            point.transform.SetParent(patrolParent.transform);
            patrolPoints[i] = point.transform;
        }
    }

    private void Update()
    {
        if (!core.CanMove) return;

        bool playerDetected = IsPlayerInRange();

        if (playerDetected)
        {
            timeSinceLastPlayerSeen = 0f;
            isReturningToRest = false;
            isResting = false; //  Cancelar descanso si ve al jugador
            restTimer = 0f;

            if (canChasePlayer)
            {
                ChasePlayer();
            }
        }
        else
        {
            timeSinceLastPlayerSeen += Time.deltaTime;

            // Volver a descansar si pasó mucho tiempo sin ver al jugador
            if (timeSinceLastPlayerSeen >= returnToRestTime && !isReturningToRest && !isResting)
            {
                isReturningToRest = true;
            }

            //  Lógica de descanso
            if (isResting)
            {
                Rest();
            }
            else if (isReturningToRest)
            {
                ReturnToRest();
            }
            else if (!isWaiting)
            {
                PatrolBetweenPoints();
            }
        }
    }


    private bool IsPlayerInRange()
    {
        return core.player != null && core.DistanceToPlayer() <= detectionRange;
    }

    private void ChasePlayer()
    {
        if (core.player == null) return;

        // Posición objetivo sobre el jugador a cierta altura
        Vector3 targetPosition = core.player.position + Vector3.up * chaseHeight;
        Vector2 direction = (targetPosition - transform.position).normalized;

        // Girar hacia el jugador
        core.FaceDirection(direction);

        // Moverse hacia el jugador
        if (core.rb != null)
        {
            core.rb.linearVelocity = direction * chaseSpeed;
        }
    }
    private void PatrolBetweenPoints()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        // Girar hacia el punto
        core.FaceDirection(direction);

        // Moverse hacia el punto
        if (core.rb != null)
        {
            core.rb.linearVelocity = direction * flySpeed;
        }

        // Verificar si llegó al punto
        float distance = Vector2.Distance(transform.position, targetPoint.position);
        if (distance <= waypointReachDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        // Detener movimiento
        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(idleTimeAtWaypoint);

        // Siguiente punto
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    private void ReturnToRest()
    {
        if (restingSpot == null) return;

        Vector2 direction = (restingSpot.position - transform.position).normalized;
        core.FaceDirection(direction);

        if (core.rb != null)
        {
            core.rb.linearVelocity = direction * flySpeed;
        }

        // Si llegó al punto de reposo
        float distance = Vector2.Distance(transform.position, restingSpot.position);
        if (distance <= waypointReachDistance)
        {
            isReturningToRest = false;
            isResting = true; // Empezar a descansar
            restTimer = 0f;

            if (core.rb != null)
            {
                core.rb.linearVelocity = Vector2.zero;
            }

        }
    }

    private void Rest()
    {
        // Incrementar timer de descanso
        restTimer += Time.deltaTime;

        // Mantener quieto
        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }

        // Cuando termina el descanso, volver a patrullar
        if (restTimer >= restDuration)
        {
            isResting = false;
            restTimer = 0f;
            timeSinceLastPlayerSeen = 0f; // Reset para no volver inmediatamente
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Punto de reposo
        if (restingSpot != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(restingSpot.position, 0.5f);
            Gizmos.DrawLine(transform.position, restingSpot.position);
        }

        // Puntos de patrulla
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);

                    // Línea al siguiente punto
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
}