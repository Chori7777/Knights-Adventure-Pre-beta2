using UnityEngine;

public class bossCore : MonoBehaviour
{
    [Header("Componentes principales")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public Transform player;

    [Header("Estado general")]
    public bool IsDead = false;
    public bool IsAttacking = false;
    public bool IsVulnerable = true;
    public bool CanMove = true;

    public Vector2 PlayerPosition => player != null ? (Vector2)player.position : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    public float DistanceToPlayer()
    {
        return player == null ? Mathf.Infinity : Vector2.Distance(transform.position, player.position);
    }

    public Vector2 DirectionToPlayer()
    {
        return player == null ? Vector2.zero : (player.position - transform.position).normalized;
    }

    public void FacePlayer()
    {
        if (player == null) return;
        transform.localScale = player.position.x < transform.position.x
            ? new Vector3(-1, 1, 1)
            : new Vector3(1, 1, 1);
    }

    public void SetCanMove(bool state)
    {
        CanMove = state;
    }

    public void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
