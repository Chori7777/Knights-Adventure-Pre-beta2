using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Seguimiento")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private bool canChasePlayer = true;

    private EnemyCore core;
    private bool movingRight = true;


    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        CreateGroundCheckIfNeeded();
    }

    private void CreateGroundCheckIfNeeded()
    {
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.SetParent(transform);
            checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObj.transform;
        }
    }

    private void Update()
    {
        if (!core.CanMove) return;

        if (canChasePlayer && IsPlayerInRange())
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

   //Que siga al jugador
    private bool IsPlayerInRange()
    {
        return core.player != null && core.DistanceToPlayer() <= detectionRange;
    }

    private void ChasePlayer()
    {
        if (core.player == null) return;

        Vector2 direction = core.DirectionToPlayer();
        core.FaceDirection(direction);

        // Mover hacia el jugador (solo horizontal)
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, core.rb.linearVelocity.y);
        }
    }

//Patrullaje
    private void Patrol()
    {
        // Verificar si hay suelo adelante
        if (!HasGroundAhead())
        {
            FlipDirection();
        }

        // Mover en la dirección actual
        if (core.rb != null)
        {
            float direction = movingRight ? 1f : -1f;
            core.rb.linearVelocity = new Vector2(direction * patrolSpeed, core.rb.linearVelocity.y);
        }

        // Actualizar orientación del sprite
        if (movingRight && !core.FacingRight)
        {
            core.FaceDirection(Vector2.right);
        }
        else if (!movingRight && core.FacingRight)
        {
            core.FaceDirection(Vector2.left);
        }
    }
    //Detecta suelo
    private bool HasGroundAhead()
    {
        if (groundCheck == null) return true;

        Vector2 rayOrigin = groundCheck.position + (movingRight ? Vector3.right * 0.3f : Vector3.left * 0.3f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);

        return hit.collider != null;
    }

    private void FlipDirection()
    {
        movingRight = !movingRight;

        // Detener movimiento al girar
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        }
    }
    //Gizmos para ver el rango de detección y el chequeo de suelo
    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Detección de suelo
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 offset = movingRight ? Vector3.right * 0.3f : Vector3.left * 0.3f;
            Vector3 rayOrigin = groundCheck.position + offset;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        }
    }
}