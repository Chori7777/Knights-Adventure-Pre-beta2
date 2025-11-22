using UnityEngine;
using System.Collections;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ataque")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float hitboxRadius = 1f;

    [Header("Daño - NUEVO")]
    [SerializeField] private float damageImmunityDuration = 0.5f; 
    private float lastDamageDealtTime = -Mathf.Infinity; 

    private EnemyCore core;
    private float lastAttackTime = -Mathf.Infinity;
    private Coroutine attackCoroutine;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        CreateAttackPointIfNeeded();
    }

    private void CreateAttackPointIfNeeded()
    {
        if (attackPoint == null)
        {
            GameObject pointObj = new GameObject("AttackPoint");
            pointObj.transform.SetParent(transform);
            pointObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            attackPoint = pointObj.transform;
        }
    }

    private void Update()
    {
        if (!core.CanMove || core.IsAttacking) return;

        if (IsPlayerInRange() && CanAttack())
        {
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private bool IsPlayerInRange()
    {
        if (core.player == null) return false;

        Vector2 direction = core.FacingDirection;
        RaycastHit2D hit = Physics2D.Raycast(attackPoint.position, direction, attackRange, playerLayer);

        return hit.collider != null;
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    private IEnumerator AttackRoutine()
    {
        core.SetAttacking(true);
        lastAttackTime = Time.time;

        if (core.rb != null)
        {
            core.rb.linearVelocity = Vector2.zero;
        }

        if (core.animController != null)
        {
            core.animController.TriggerAttack();
        }

        yield return new WaitForSeconds(attackDuration * 0.5f);

        DealDamage();

        yield return new WaitForSeconds(attackDuration * 0.5f);

        core.SetAttacking(false);
        attackCoroutine = null;
    }

    public void DealDamage()
    {

        if (Time.time < lastDamageDealtTime + damageImmunityDuration)
        {
            Debug.Log($" Daño en cooldown. Espera {damageImmunityDuration}s");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, hitboxRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerLife playerLife = hit.GetComponent<playerLife>();
                if (playerLife != null)
                {
                    Vector2 attackPosition = new Vector2(transform.position.x, 0);
                    playerLife.TakeDamage(attackPosition, attackDamage);

                    lastDamageDealtTime = Time.time;

                    Debug.Log("⚔️ Golpeó al jugador");
                }
            }
        }
    }

    public void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Vector2 direction = core != null ? core.FacingDirection : Vector2.right;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + (Vector3)direction * attackRange);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(attackPoint.position, hitboxRadius);
    }
}