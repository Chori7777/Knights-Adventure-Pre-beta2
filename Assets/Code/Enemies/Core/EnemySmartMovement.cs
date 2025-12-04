using UnityEngine;

public class EnemySmartMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;

    [Header("Salto Automático")]
    [SerializeField] private float jumpVelocity = 8f;
    [SerializeField] private float obstacleCheckDistance = 1.2f;
    [SerializeField] private float obstacleCheckBottom = 0.2f;
    [SerializeField] private float obstacleCheckTop = 1.2f;
    [SerializeField] private float minTimeBetweenJumps = 0.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private EnemyCore core;
    private Transform groundCheck;
    private bool movingRight = true;
    private bool isGrounded;
    private bool jumpRequested = false;
    private float lastJumpTime = -10f;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();


        GameObject checkObj = new GameObject("GroundCheck");
        checkObj.transform.SetParent(transform);
        checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
        groundCheck = checkObj.transform;

    }

    private void Update()
    {
        if (!core.CanMove) return;

        UpdateGrounded();

        if (IsPlayerNear())
            ChasePlayer();
        else
            Patrol();

        // Detección de obstáculo y solicitud de salto
        if (isGrounded && Time.time - lastJumpTime >= minTimeBetweenJumps)
        {
            bool jumpable = IsThereJumpableObstacleAhead();

            if (jumpable)
            {
                jumpRequested = true;
                lastJumpTime = Time.time;
            }
        }
    }

    private void FixedUpdate()
    {
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }
    }


    private void UpdateGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.15f, groundLayer);
        Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.15f, isGrounded ? Color.cyan : Color.gray);

    }


    private bool IsThereJumpableObstacleAhead()
    {
        Vector2 dir = core.FacingDirection;
        Vector2 originBottom = (Vector2)transform.position + Vector2.up * obstacleCheckBottom;
        Vector2 originTop = (Vector2)transform.position + Vector2.up * obstacleCheckTop;

        RaycastHit2D hitLow = Physics2D.Raycast(originBottom, dir, obstacleCheckDistance, obstacleLayer);
        RaycastHit2D hitHigh = Physics2D.Raycast(originTop, dir, obstacleCheckDistance, obstacleLayer);

        // Dibujar rayos en escena
        Debug.DrawLine(originBottom, originBottom + dir * obstacleCheckDistance, hitLow ? Color.red : Color.green);
        Debug.DrawLine(originTop, originTop + dir * obstacleCheckDistance, hitHigh ? Color.magenta : Color.blue);

        if (hitLow.collider != null && hitHigh.collider == null)
        {

            return true;
        }
        else if (hitLow.collider != null && hitHigh.collider != null)
        {

        }
        else if (hitLow.collider == null)
        {

        }

        return false;
    }

 
    private void PerformJump()
    {
        if (core.rb == null) return;

        Vector2 lv = core.rb.linearVelocity;
        lv.y = jumpVelocity;
        core.rb.linearVelocity = lv;


    }

    private bool IsPlayerNear()
    {
        return core.player != null && core.DistanceToPlayer() <= detectionRange;
    }

    private void ChasePlayer()
    {
        Vector2 direction = core.DirectionToPlayer();
        core.FaceDirection(direction);
        core.rb.linearVelocity = new Vector2(direction.x * moveSpeed, core.rb.linearVelocity.y);
    }

    private void Patrol()
    {
        Vector2 frontCheck = groundCheck.position + (movingRight ? Vector3.right * 0.5f : Vector3.left * 0.5f);
        bool hasGroundAhead = Physics2D.Raycast(frontCheck, Vector2.down, 0.5f, groundLayer);
        Debug.DrawLine(frontCheck, frontCheck + Vector2.down * 0.5f, hasGroundAhead ? Color.cyan : Color.magenta);

        if (!hasGroundAhead)
        {
            movingRight = !movingRight;
            core.rb.linearVelocity = new Vector2(0f, core.rb.linearVelocity.y);
  
            return;
        }

        float dir = movingRight ? 1f : -1f;
        core.rb.linearVelocity = new Vector2(dir * moveSpeed, core.rb.linearVelocity.y);
        core.FaceDirection(new Vector2(dir, 0));
    }
}
