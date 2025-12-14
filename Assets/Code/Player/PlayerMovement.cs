using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    private bool hasAirDashed;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private bool enable4WayDash = false;
    [SerializeField] private bool dashUpAsExtraJump = true;
    [SerializeField] private float dashUpJumpForce = 12f;
    [SerializeField] private bool dashUpConsumesAirDash = true;
    [SerializeField] private bool dashUpConsumesDoubleJump = false;
    [SerializeField] private GameObject dashAuraPrefab;
    [SerializeField] private float dashAuraLifetime = 0.25f;
    [SerializeField] private float dashAuraOffset = 0.5f;
    [SerializeField] private string dashDestroyTag = "Breakable";
    [SerializeField] private int dashDamage = 1;

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

    private bool isTakingDamage;
    [SerializeField] private bool allowControlsWhileDamaged = false;

    [Header("Camera Holder")]
    [SerializeField] private Transform cameraHolder;
    private Vector3 originalCameraPosition;

    [Header("Resistance Shield")]
    [SerializeField] public bool enableResistanceShield = false;
    [SerializeField] private float shieldMax = 100f;
    [SerializeField] private float shieldDurability = 100f;
    [SerializeField] private float shieldTickCost = 2f;
    [SerializeField] private float shieldRechargeDelay = 1.5f;
    [SerializeField] private float shieldRechargeRate = 25f;
    [SerializeField] private Image shieldBarFill;
    private float shieldIdleTimer = 0f;
    private bool shieldRecharging = false;

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
        if (isTakingDamage && !allowControlsWhileDamaged) return;

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

        UpdateShieldRecharge(Time.deltaTime);
        UpdateShieldUI();
    }

    private void FixedUpdate()
    {
        if (isTakingDamage && !allowControlsWhileDamaged) return;

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
        float h = 0f;
        if (InputBindings.Get(InputBindings.GameAction.MoveLeft)) h -= 1f;
        if (InputBindings.Get(InputBindings.GameAction.MoveRight)) h += 1f;
        horizontalInput = Mathf.Clamp(h, -1f, 1f);
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
            hasAirDashed = false;
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
        if (InputBindings.GetDown(InputBindings.GameAction.Jump))
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

    private void PerformDashUpJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        float force = dashUpJumpForce > 0f ? dashUpJumpForce : doubleJumpForce;
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        if (dashUpConsumesDoubleJump)
        {
            hasDoubleJumped = true;
        }
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
        else if (rb.linearVelocity.y > 0 && !InputBindings.Get(InputBindings.GameAction.Jump))
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
        if (Input.GetKeyDown(dashKey) && Time.time > lastDashTime + dashCooldown)
        {
            bool canDashNow = isGrounded || (!isGrounded && !hasAirDashed);
            if (!canDashNow) return;

            if (isGrounded)
            {
                PerformJump(jumpForce);
                hasAirDashed = false;
            }
            else
            {
                hasAirDashed = true;
            }

            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;

            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            if (enable4WayDash)
            {
                bool up = Input.GetKey(KeyCode.UpArrow);
                bool down = Input.GetKey(KeyCode.DownArrow);
                bool left = Input.GetKey(KeyCode.LeftArrow);
                bool right = Input.GetKey(KeyCode.RightArrow);

                Vector2 v = Vector2.zero;
                if (up) v += Vector2.up;
                if (down) v += Vector2.down;
                if (left) v += Vector2.left;
                if (right) v += Vector2.right;
                if (v != Vector2.zero) dir = v.normalized;

                // Dash hacia arriba como salto adicional
                if (dashUpAsExtraJump && up && !down && dir.y > 0.5f)
                {
                    PerformDashUpJump();
                    if (dashUpConsumesAirDash) hasAirDashed = true;
                    if (dashUpConsumesDoubleJump) hasDoubleJumped = true;
                    // Consumir cooldown del dash, pero no entrar en estado de dash
                    isDashing = false;
                    dashTimer = 0f;
                    lastDashTime = Time.time;
                    // Aura opcional
                    if (dashAuraPrefab != null)
                    {
                        Vector3 auraPos = transform.position - new Vector3(0f, 1f, 0f) * dashAuraOffset;
                        GameObject aura = Instantiate(dashAuraPrefab, auraPos, Quaternion.identity);
                        aura.transform.right = Vector3.up;
                        Destroy(aura, dashAuraLifetime);
                    }
                    return;
                }
            }
            currentDashDir = dir;

            if (dashAuraPrefab != null)
            {
                Vector3 auraPos = transform.position - new Vector3(dir.x, dir.y, 0f) * dashAuraOffset;
                GameObject aura = Instantiate(dashAuraPrefab, auraPos, Quaternion.identity);
                aura.transform.right = new Vector3(dir.x, dir.y, 0f);
                Destroy(aura, dashAuraLifetime);
            }
        }

        if (isDashing)
        {
            rb.linearVelocity = currentDashDir * dashSpeed;
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

        if (InputBindings.GetDown(InputBindings.GameAction.Action1Attack))
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

    // Propiedades públicas
    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public bool IsAttacking => isAttacking;
    public bool IsDashing => isDashing;
    public bool IsTakingDamage => isTakingDamage;
    public float HorizontalInput => horizontalInput;
    public float VerticalVelocity => rb.linearVelocity.y;
    public bool IsBlocking => InputBindings.Get(InputBindings.GameAction.Action2Shield);
    public bool IsWallSliding => isWallSliding;
    public string DashDestroyTag => dashDestroyTag;

    public void SetControlsEnabled(bool enabled)
    {
        canMove = enabled;
        canJump = enabled;
        canDoubleJump = enabled;
        canAttack = enabled;
        canDash = enabled;
        canWallCling = enabled;
        canBlock = enabled;
        canThrowProjectile = enabled;
    }
    public bool IsSprinting => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    public bool FacingRight => facingRight;

    private Vector2 currentDashDir = Vector2.right;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDashing) return;

        if (!string.IsNullOrEmpty(dashDestroyTag) && collision.CompareTag(dashDestroyTag))
        {
            Destroy(collision.gameObject);
            return;
        }

        var eLife = collision.GetComponent<EnemyLife>();
        if (eLife != null)
        {
            eLife.TakeDamageWithKnockback(transform.position, dashDamage);
            return;
        }

        var bLife = collision.GetComponent<BossLife>();
        if (bLife != null)
        {
            bLife.RecibeDanio(transform.position, dashDamage);
            return;
        }
    }

    private void UpdateShieldRecharge(float dt)
    {
        if (!enableResistanceShield) return;
        bool quiet = Mathf.Abs(rb.linearVelocity.x) < 0.01f && !IsAttacking && !IsDashing && !IsBlocking;
        if (!quiet)
        {
            shieldIdleTimer = 0f;
            shieldRecharging = false;
            return;
        }
        shieldIdleTimer += dt;
        if (shieldIdleTimer >= shieldRechargeDelay) shieldRecharging = true;
        if (shieldRecharging && shieldDurability < shieldMax)
        {
            shieldDurability = Mathf.Min(shieldMax, shieldDurability + shieldRechargeRate * dt);
        }
    }

    private void UpdateShieldUI()
    {
        if (shieldBarFill == null) return;
        if (!enableResistanceShield || !canBlock)
        {
            if (shieldBarFill.gameObject.activeSelf) shieldBarFill.gameObject.SetActive(false);
            return;
        }
        bool show = canBlock && Input.GetKey(KeyCode.X);
        if (shieldBarFill.gameObject.activeSelf != show) shieldBarFill.gameObject.SetActive(show);
        shieldBarFill.fillAmount = shieldDurability / shieldMax;
    }

    public void BossContactTick()
    {
        if (!enableResistanceShield) return;
        if (shieldDurability <= 0f) return;
        shieldDurability = Mathf.Max(0f, shieldDurability - shieldTickCost);
        shieldIdleTimer = 0f;
        shieldRecharging = false;
    }

    public void OnBossAttackEnd()
    {
        if (!enableResistanceShield) return;
        if (shieldDurability < shieldMax * 0.5f)
        {
            var life = GetComponent<playerLife>();
            if (life != null) life.TakeDamage(transform.position, 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing) return;
        var go = collision.gameObject;
        if (!string.IsNullOrEmpty(dashDestroyTag) && go.CompareTag(dashDestroyTag))
        {
            Destroy(go);
            return;
        }
        var eLife = go.GetComponent<EnemyLife>();
        if (eLife != null)
        {
            eLife.TakeDamageWithKnockback(transform.position, dashDamage);
            return;
        }
        var bLife = go.GetComponent<BossLife>();
        if (bLife != null)
        {
            bLife.RecibeDanio(transform.position, dashDamage);
            return;
        }
    }
}
