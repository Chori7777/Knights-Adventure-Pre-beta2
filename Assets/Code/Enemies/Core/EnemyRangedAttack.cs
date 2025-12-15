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
    [Header("Visual")]
    [SerializeField] private bool addTrailToProjectiles = true;
    [SerializeField] private float projTrailTime = 0.25f;
    [SerializeField] private float projTrailWidth = 0.08f;
    [SerializeField] private Color projTrailStartColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color projTrailEndColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private bool useProjectileAfterimage = false;
    [SerializeField] private float projAfterimageInterval = 0.045f;
    [SerializeField] private float projAfterimageLifetime = 0.22f;
    [SerializeField] private Color projAfterimageColor = new Color(1f, 1f, 1f, 0.7f);

    private EnemyCore core;
    private float lastAttackTime = -Mathf.Infinity;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        CreateFirePointIfNeeded();
    }

    private void Awake()
    {
        addTrailToProjectiles = true;
        useProjectileAfterimage = false;
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

    private void AddTrail(GameObject go)
    {
        var tr = go.GetComponent<TrailRenderer>();
        if (tr == null) tr = go.AddComponent<TrailRenderer>();
        tr.time = projTrailTime;
        tr.minVertexDistance = 0.08f;
        tr.autodestruct = false;
        tr.startWidth = projTrailWidth;
        tr.endWidth = projTrailWidth * 0.7f;
        tr.material = new Material(Shader.Find("Sprites/Default"));
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(projTrailStartColor, 0f), new GradientColorKey(projTrailEndColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(projTrailStartColor.a, 0f), new GradientAlphaKey(projTrailEndColor.a, 1f) }
        );
        tr.colorGradient = g;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            tr.sortingLayerID = sr.sortingLayerID;
            tr.sortingOrder = sr.sortingOrder - 1;
        }
    }

    private IEnumerator ProjectileAfterimageRoutine(GameObject go)
    {
        SpriteRenderer sr = null;
        if (go != null)
        {
            sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>(true);
        }
        float t = 0f;
        while (go != null && sr != null && t < 3f)
        {
            SpawnAfterimage(sr, projAfterimageLifetime);
            t += projAfterimageInterval;
            yield return new WaitForSeconds(projAfterimageInterval);
        }
    }

    private void SpawnAfterimage(SpriteRenderer source, float lifetime)
    {
        var go = new GameObject("ProjectileAfterimage");
        var c = go.AddComponent<SpriteRenderer>();
        c.sprite = source.sprite;
        c.flipX = source.flipX;
        c.color = projAfterimageColor;
        c.sortingLayerID = source.sortingLayerID;
        c.sortingOrder = source.sortingOrder - 1;
        go.transform.position = source.transform.position;
        StartCoroutine(FadeAndDestroy(c, lifetime));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer c, float lifetime)
    {
        float t = lifetime;
        while (t > 0f && c != null)
        {
            t -= Time.unscaledDeltaTime;
            var col = c.color;
            col.a = Mathf.Clamp01(t / lifetime);
            c.color = col;
            yield return null;
        }
        if (c != null) Destroy(c.gameObject);
    }
    public void ConfigureProjectile(GameObject prefab, float speed, int count = 1, float spread = 15f)
    {
        projectilePrefab = prefab;
        projectileSpeed = speed;
        projectileCount = Mathf.Max(1, count);
        spreadAngle = spread;
    }
    public void SetRanges(float detection, float attack, float minAttack)
    {
        detectionRange = detection;
        attackRange = attack;
        minAttackRange = minAttack;
    }
    public void TryAttackNow()
    {
        StopAllCoroutines();
        lastAttackTime = Time.time - attackCooldown;
        StartCoroutine(AttackRoutine());
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
            if (addTrailToProjectiles) AddTrail(projectile);
            var projComp = projectile.GetComponent<Projectile>();
            if (projComp != null)
            {
                projComp.SetAddTrail(true);
                projComp.SetUseAfterimage(false);
                projComp.SetTrailStyle(projTrailTime, projTrailWidth, projTrailStartColor, projTrailEndColor);
            }
            else if (useProjectileAfterimage)
            {
                StartCoroutine(ProjectileAfterimageRoutine(projectile));
            }

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
