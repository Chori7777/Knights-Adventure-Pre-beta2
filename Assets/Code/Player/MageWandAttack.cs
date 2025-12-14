using UnityEngine;
using System.Collections;

public class MageWandAttack : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Ataque")]
    [SerializeField] private int orbCount = 3;
    [SerializeField] private float orbSpeed = 10f;
    [SerializeField] private float spreadAngle = 12f;
    [SerializeField] private float orbLifetime = 3f;
    [SerializeField] private int orbDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTriggerName = "WandAttack";

    private float lastAttackTime = -10f;
    private PlayerMovement pm;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (firePoint == null) firePoint = transform;
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (pm != null && !pm.canThrowProjectile) return;
        if (!InputBindings.GetDown(InputBindings.GameAction.Action1Attack)) return;
        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;

        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        ShootOrbs();
    }

    private void ShootOrbs()
    {
        if (orbPrefab == null || firePoint == null) return;

        Vector2 baseDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        float half = spreadAngle * 0.5f;

        for (int i = 0; i < Mathf.Max(1, orbCount); i++)
        {
            float t = (orbCount == 1) ? 0f : (float)i / (orbCount - 1);
            float ang = Mathf.Lerp(-half, half, t);
            Vector2 dir = Rotate(baseDir, ang);
            GameObject o = Instantiate(orbPrefab, firePoint.position, Quaternion.identity);
            var rb = o.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir.normalized * orbSpeed;

            var dmg = o.GetComponent<MageOrbDamage>();
            if (dmg == null) dmg = o.AddComponent<MageOrbDamage>();
            dmg.damage = orbDamage;
            dmg.lifetime = orbLifetime;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }
}
