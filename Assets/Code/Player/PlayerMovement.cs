using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hurtSound;

    private Rigidbody2D rb;
    private PlayerAnimationController animController;

    [Header("Puntos de Detección")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("Capas")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float blockMoveSpeed = 2f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private bool isSprintMode = true;

    private float horizontalInput;
    private bool facingRight = true;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private float fallMultiplier = 3f;
    [SerializeField] private float lowJumpMultiplier = 2.5f;
    [SerializeField] private float coyoteTime = 0.15f;

    private float coyoteTimeCounter;
    private bool hasDoubleJumped;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    private bool isDashing;
    private float dashTimer;
    private float lastDashTime = -10f;

    [Header("Interacción con Paredes")]
    [SerializeField] private float wallSlideSpeed = 1.5f;
    [SerializeField] private float wallSlideAcceleration = 2f;
    [SerializeField] private float wallJumpForceX = 10f;
    [SerializeField] private float wallJumpForceY = 16f;
    [SerializeField] private float wallGravity = 0.3f;
    [SerializeField] private float wallJumpCooldown = 0.2f;

    private float originalGravity;
    private bool isWallSliding;
    private float currentWallSlideSpeed;
    private float lastWallJumpTime = -10f;

    [Header("Combate")]
    [SerializeField] private float attackStepSpeed = 5f;
    [SerializeField] private float attackStepDelay = 0.15f;
    [SerializeField] private float attackStepDuration = 0.1f;
    [SerializeField] private float attackGroundDuration = 0.4f;
    [SerializeField] private float attackAirDuration = 0.4f;
    [SerializeField] private float attackCooldown = 0.1f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float damageRecoveryTime = 0.5f;

    private int currentCombo;
    private bool isAttacking;
    private float attackDelayTimer;
    private float attackMoveTimer;
    private bool attackStepActive;
    private float lastAttackTime = -10f;

    [Header("Habilidades Desbloqueables")]
    public bool canMove = true;
    public bool canJump = true;
    public bool canDoubleJump = true;
    public bool canAttack = true;
    public bool canDash = true;
    public bool canWallCling = true;
    public bool canBlock = true;
    public bool canThrowProjectile = true; 

    [Header("Detección")]
    [SerializeField] private float groundCheckRay = 0.2f;
    [SerializeField] private float groundCheckSpacing = 0.3f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallCheckHeight = 0.5f;
    [SerializeField] private float inputDeadzone = 0.1f;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool wasGrounded;

    [Header("Efecto de Cámara")]
    [SerializeField] private float cameraShakeIntensity = 0.3f;
    [SerializeField] private float cameraShakeDuration = 0.2f;

    [SerializeField] private bool isTakingDamage;

    [Header("Camera Holder")]
    [SerializeField] private Transform cameraHolder;
    private Vector3 originalCameraPosition;

    private void Start()
    {
        InitializeComponents();

        if (cameraHolder != null)
        {
            originalCameraPosition = cameraHolder.localPosition;
        }
    }

    private void Update()
    {
        if (isTakingDamage) return;

        CaptureInput();
        UpdateDetectionStates();
        UpdateJumpTimers();
        UpdateAttackStepTimer();

        if (!isDashing)
        {
            HandleAllActions();
        }
        else
        {
            UpdateDashTimer();
        }

        ApplyBetterFalling();
    }

    private void FixedUpdate()
    {
        if (isTakingDamage) return;

        bool isBlocking = Input.GetKey(KeyCode.X);
        bool isHoldingCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (attackStepActive && attackMoveTimer > 0)
        {
            float direction = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * attackStepSpeed, rb.linearVelocity.y);
            attackMoveTimer -= Time.fixedDeltaTime;

            if (attackMoveTimer <= 0)
            {
                attackStepActive = false;
            }
        }
        else if (!isDashing && canMove && !isAttacking && !isBlocking)
        {
            float finalSpeed = moveSpeed;

            if (isHoldingCtrl)
            {
                if (isSprintMode)
                {
                    finalSpeed = moveSpeed * sprintMultiplier;
                }
                else
                {
                    finalSpeed = moveSpeed / sprintMultiplier;
                }
            }

            ApplyMovement(horizontalInput * finalSpeed);
        }
        else if (isBlocking && !isAttacking)
        {
            ApplyMovement(horizontalInput * blockMoveSpeed);
        }
        else if (isAttacking && !attackStepActive)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<PlayerAnimationController>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<PlayerAnimationController>();
        }

        animController.Initialize(this);
        originalGravity = rb.gravityScale;
    }

    private void CaptureInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    private void UpdateDetectionStates()
    {
        wasGrounded = isGrounded;

        Vector2 leftCheckPos = (Vector2)groundCheck.position + Vector2.left * groundCheckSpacing;
        Vector2 rightCheckPos = (Vector2)groundCheck.position + Vector2.right * groundCheckSpacing;

        bool leftGrounded = Physics2D.Raycast(leftCheckPos, Vector2.down, groundCheckRay, groundLayer);
        bool rightGrounded = Physics2D.Raycast(rightCheckPos, Vector2.down, groundCheckRay, groundLayer);

        isGrounded = leftGrounded || rightGrounded;

        Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 upperWallCheck = (Vector2)wallCheck.position + Vector2.up * wallCheckHeight;
        Vector2 lowerWallCheck = (Vector2)wallCheck.position + Vector2.down * wallCheckHeight;

        bool upperWallHit = Physics2D.Raycast(upperWallCheck, wallDirection, wallCheckDistance, wallLayer);
        bool centerWallHit = Physics2D.Raycast(wallCheck.position, wallDirection, wallCheckDistance, wallLayer);
        bool lowerWallHit = Physics2D.Raycast(lowerWallCheck, wallDirection, wallCheckDistance, wallLayer);

        isTouchingWall = upperWallHit || centerWallHit || lowerWallHit;

        if (isGrounded && !wasGrounded)
        {
            hasDoubleJumped = false;
            currentWallSlideSpeed = 0f;
        }
    }

    private void UpdateJumpTimers()
    {
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
    }

    private void HandleAllActions()
    {
        if (canMove) HandleMovement();
        if (canWallCling) HandleWallCling();
        if (canJump) HandleJump();
        if (canAttack) HandleAttack();
        if (canDash) HandleDash();
    }

    private void HandleMovement()
    {
        if (isAttacking) return;
        FlipCharacter(horizontalInput);
    }

    private void ApplyMovement(float speed)
    {
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    private void FlipCharacter(float direction)
    {
        if (direction > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWallSliding && Time.time > lastWallJumpTime + wallJumpCooldown)
            {
                WallJump();
            }
            else if (isGrounded || coyoteTimeCounter > 0f)
            {
                PerformJump(jumpForce);
                hasDoubleJumped = false;
                coyoteTimeCounter = 0f;
            }
            else if (!isGrounded && !hasDoubleJumped && canDoubleJump)
            {
                PerformDoubleJump();
            }
        }
    }

    private void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void PerformDoubleJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
        hasDoubleJumped = true;
        animController?.TriggerDoubleJump();
    }

    private void WallJump()
    {
        lastWallJumpTime = Time.time;

        float jumpDirX = facingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(jumpDirX * wallJumpForceX, wallJumpForceY);

        Flip();

        hasDoubleJumped = false;
        isWallSliding = false;
        currentWallSlideSpeed = 0f;
        rb.gravityScale = originalGravity;
    }

    private void ApplyBetterFalling()
    {
        if (isWallSliding) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravity;
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
        }

        if (isDashing)
        {
            float dashDirection = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y);
        }
    }

    private void UpdateDashTimer()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0)
        {
            isDashing = false;
        }
    }

    private void HandleWallCling()
    {
        if (Time.time < lastWallJumpTime + wallJumpCooldown)
        {
            isWallSliding = false;
            if (rb.gravityScale == wallGravity) rb.gravityScale = originalGravity;
            return;
        }

        bool isPressingTowardsWall = Mathf.Abs(horizontalInput) > inputDeadzone &&
                                     Mathf.Sign(horizontalInput) == (facingRight ? 1 : -1);

        bool canWallSlide = !isGrounded && isTouchingWall && isPressingTowardsWall && rb.linearVelocity.y <= 0;

        if (canWallSlide)
        {
            if (!isWallSliding)
            {
                isWallSliding = true;
                currentWallSlideSpeed = 0f;
                hasDoubleJumped = false;
            }

            currentWallSlideSpeed = Mathf.MoveTowards(currentWallSlideSpeed, wallSlideSpeed, wallSlideAcceleration * Time.deltaTime);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -currentWallSlideSpeed);
            rb.gravityScale = wallGravity;
        }
        else
        {
            if (isWallSliding)
            {
                isWallSliding = false;
                currentWallSlideSpeed = 0f;
            }

            if (rb.gravityScale == wallGravity) rb.gravityScale = originalGravity;
        }
    }

    private void UpdateAttackStepTimer()
    {
        if (isAttacking && attackDelayTimer > 0)
        {
            attackDelayTimer -= Time.deltaTime;
            if (attackDelayTimer <= 0 && !attackStepActive)
            {
                attackStepActive = true;
                attackMoveTimer = attackStepDuration;
            }
        }
    }

    private void HandleAttack()
    {
        if (isAttacking || Time.time < lastAttackTime + attackCooldown) return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentCombo = isGrounded ? ((currentCombo == 1) ? 2 : 1) : 1;
            StartAttack(currentCombo);

            if (AudioManager.Instance != null && attackSound != null)
            {
                AudioManager.Instance.PlaySFX(attackSound, 0.1f, 0.5f);
            }
        }
    }

    private void StartAttack(int comboIndex)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animController.SetComboIndex(comboIndex);

        attackDelayTimer = attackStepDelay;
        attackStepActive = false;
        attackMoveTimer = 0;

        float duration = isGrounded ? attackGroundDuration : attackAirDuration;
        StartCoroutine(AttackCoroutine(duration));
    }

    private IEnumerator AttackCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopAttack();
    }

    private void StopAttack()
    {
        isAttacking = false;
        attackDelayTimer = 0;
        attackMoveTimer = 0;
        attackStepActive = false;
    }

    public void TakeDamage(Vector2 attackerPosition)
    {
        if (rb == null || animController == null)
        {
            InitializeComponents();
        }

        isTakingDamage = true;
        animController?.TriggerDamage();

        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
        knockbackDirection.y = Mathf.Clamp(knockbackDirection.y + 0.5f, 0.5f, 1f);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (cameraHolder != null)
            StartCoroutine(CameraShake());

        StartCoroutine(DamageRecoveryCoroutine());

        if (hurtSound != null && AudioManager.Instance != null)
        {
            float randomPitch = Random.Range(0.9f, 1.1f);
            AudioManager.Instance.PlaySFX(hurtSound, 0.5f, randomPitch);
        }
    }

    private IEnumerator CameraShake()
    {
        float elapsed = 0f;
        Vector3 originalPos = originalCameraPosition;

        while (elapsed < cameraShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * cameraShakeIntensity;
            float y = Random.Range(-1f, 1f) * cameraShakeIntensity;

            cameraHolder.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraHolder.localPosition = originalPos;
    }

    private IEnumerator DamageRecoveryCoroutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        isTakingDamage = false;
        animController.StopDamage();
    }

//propiedades publicas
    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public bool IsAttacking => isAttacking;
    public bool IsDashing => isDashing;
    public bool IsTakingDamage => isTakingDamage;
    public float HorizontalInput => horizontalInput;
    public float VerticalVelocity => rb.linearVelocity.y;
    public bool IsBlocking => Input.GetKey(KeyCode.X);
    public bool IsWallSliding => isWallSliding;
    public bool IsSprinting => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    public bool FacingRight => facingRight;
}