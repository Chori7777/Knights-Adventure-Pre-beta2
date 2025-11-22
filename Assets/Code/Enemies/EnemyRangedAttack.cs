using UnityEngine;
using System.Collections;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float minAttackRange = 2f; // No disparar si está muy cerca

    [Header("Proyectil")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Ataque")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 0.5f;

    [Header("Múltiples Proyectiles (Opcional)")]
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 15f; // Ángulo de dispersión

    private EnemyCore core;
    private float lastAttackTime = -Mathf.Infinity;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        CreateFirePointIfNeeded();
    }

    private void CreateFirePointIfNeeded()
    {
        if (firePoint == null)
        {
            GameObject pointObj = new GameObject("FirePoint");
            pointObj.transform.SetParent(transform);
            pointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
            firePoint = pointObj.transform;
        }
    }

    private void Update()
    {
        if (!core.CanMove || core.IsAttacking) return;

        // Girar hacia el jugador si está en rango de detección
        if (IsPlayerDetected())
        {
            core.FaceTarget(core.player);

            // Atacar si está en rango de ataque
            if (IsPlayerInAttackRange() && CanAttack())
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }


    private bool IsPlayerDetected()
    {
        return core.player != null && core.DistanceToPlayer() <= detectionRange;
    }

    private bool IsPlayerInAttackRange()
    {
        float distance = core.DistanceToPlayer();
        return distance <= attackRange && distance >= minAttackRange;
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    private IEnumerator AttackRoutine()
    {
        core.SetAttacking(true);
        lastAttackTime = Time.time;

        // Detener movimiento
        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }

        // Trigger de animación
        if (core.animController != null)
        {
            core.animController.TriggerAttack();
        }

        // Esperar antes de disparar (sincronizado con animación)
        yield return new WaitForSeconds(attackDuration * 0.5f);

        // Disparar proyectil(es)
        ShootProjectiles();

        // Esperar resto de la animación
        yield return new WaitForSeconds(attackDuration * 0.5f);

        core.SetAttacking(false);
    }


    public void ShootProjectiles()
    {
 

        if (core.player == null) return;

        // Dirección base hacia el jugador
        Vector2 baseDirection = core.DirectionToPlayer();

        for (int i = 0; i < projectileCount; i++)
        {
            // Calcular ángulo de dispersión
            float angle = 0f;
            if (projectileCount > 1)
            {
                float step = spreadAngle / (projectileCount - 1);
                angle = -spreadAngle / 2 + step * i;
            }

            // Rotar dirección
            Vector2 direction = RotateVector(baseDirection, angle);

            // Instanciar proyectil
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Configurar velocidad
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }

            // Opcional: Rotar sprite del proyectil
            float rotationAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

            Debug.Log("disparando proyectil");
        }
    }
    public void CancelAttack()
    {

        StopAllCoroutines();
        core.SetAttacking(false);


        if (core.animController != null)
        {
            core.animController.ResetAttack();
        }

        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }


    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Rango mínimo
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minAttackRange);

        // Línea de disparo
        if (firePoint != null && core != null && core.player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(firePoint.position, core.player.position);
        }
    }
}