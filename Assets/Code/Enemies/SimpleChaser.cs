using UnityEngine;

[RequireComponent(typeof(EnemyCore))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyLife))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SimpleChaser : MonoBehaviour
{
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite chaseSprite;
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private float chaseRange = 6f;
    [SerializeField] private bool aggressive = false;
    [SerializeField] private bool phaseThroughWalls = false;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private bool colliderAsTrigger = true;
    [SerializeField] private float flipMinDeltaX = 0.2f;
    [SerializeField] private float flipCooldown = 0.15f;
    [SerializeField] private bool enableFlip = false;

    private EnemyCore core;
    private EnemyLife life;
    private SpriteRenderer sr;
    private bool deathApplied = false;
    private Rigidbody2D rb;
    private Collider2D col;
    private float lastFlipTime = -10f;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
        life = GetComponent<EnemyLife>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (core != null) core.SetAutoFlip(enableFlip);
        var mv = GetComponent<EnemyMovement>();
        if (mv != null) mv.SetDetectionRange(chaseRange);
        if (phaseThroughWalls)
        {
            if (rb != null) rb.isKinematic = true;
            if (col != null && colliderAsTrigger) col.isTrigger = true;
        }
    }

    private void Update()
    {
        if (sr == null || core == null) return;

        if (core.IsDead)
        {
            if (!deathApplied && deathSprite != null)
            {
                sr.sprite = deathSprite;
                deathApplied = true;
            }
            return;
        }

        bool chasing = aggressive || (core.player != null && core.DistanceToPlayer() <= chaseRange);
        if (chasing)
        {
            if (chaseSprite != null) sr.sprite = chaseSprite;
        }
        else
        {
            if (idleSprite != null) sr.sprite = idleSprite;
        }

        if (phaseThroughWalls && core.player != null && !core.IsDead && chasing)
        {
            Vector2 dir = (core.player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            if (sr != null && enableFlip)
            {
                float dx = core.player.position.x - transform.position.x;
                if (Mathf.Abs(dx) >= flipMinDeltaX && Time.time > lastFlipTime + flipCooldown)
                {
                    bool desiredFlip = dx < 0f;
                    if (sr.flipX != desiredFlip)
                    {
                        sr.flipX = desiredFlip;
                        lastFlipTime = Time.time;
                    }
                }
            }
        }
    }

    public void SetAggressive(bool value)
    {
        aggressive = value;
    }

    public void SetPhaseThroughWalls(bool value)
    {
        phaseThroughWalls = value;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();
        if (phaseThroughWalls)
        {
            if (rb != null) rb.isKinematic = true;
            if (col != null && colliderAsTrigger) col.isTrigger = true;
        }
        else
        {
            if (rb != null) rb.isKinematic = false;
            if (col != null && colliderAsTrigger) col.isTrigger = false;
        }
    }
}
