using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator anim;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public EnemyAnimationController animController;
    [HideInInspector] public EnemyMovement movement;
    [HideInInspector] public EnemyLife life;
    [HideInInspector] public EnemyMeleeAttack meleeAttack;
    [HideInInspector] public EnemyRangedAttack rangedAttack;
    [HideInInspector] public EnemyFlying flying;

    public bool IsDead { get; private set; }
    public bool IsTakingDamage { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool CanMove => !IsDead && !IsTakingDamage && !IsAttacking;

    [Header("Referencias")]
    public Transform player;

    public bool FacingRight { get; private set; } = true;
    public Vector2 FacingDirection => FacingRight ? Vector2.right : Vector2.left;

    private void Awake()
    {
        InitializeComponents();
        FindPlayer();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animController = GetComponent<EnemyAnimationController>();
        movement = GetComponent<EnemyMovement>();
        life = GetComponent<EnemyLife>();
        meleeAttack = GetComponent<EnemyMeleeAttack>();
        rangedAttack = GetComponent<EnemyRangedAttack>();
        flying = GetComponent<EnemyFlying>();

        if (animController != null)
        {
            animController.Initialize(this);
        }

        if (movement != null) movement.Initialize(this);
        if (life != null) life.Initialize(this);
        if (meleeAttack != null) meleeAttack.Initialize(this);
        if (rangedAttack != null) rangedAttack.Initialize(this);
        if (flying != null) flying.Initialize(this);
    }

    public void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction.x > 0 && !FacingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && FacingRight)
        {
            Flip();
        }
    }

    public void FaceTarget(Transform target)
    {
        if (target == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        FaceDirection(direction);
    }

    private void Flip()
    {
        FacingRight = !FacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void SetDead(bool value)
    {
        IsDead = value;
    }

    public void SetTakingDamage(bool value)
    {
        IsTakingDamage = value;
    }

    public void SetAttacking(bool value)
    {
        IsAttacking = value;
    }
    public void CancelAttack()
    {
        if (meleeAttack != null)
        {
            meleeAttack.CancelAttack();
        }

        if (rangedAttack != null)
        {
            rangedAttack.CancelAttack();
        }

        SetAttacking(false);
    }

    public float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    public Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return (player.position - transform.position).normalized;
    }
}