using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool alwaysFacePlayer = true;

    private bossCore core;

    public void Initialize(bossCore bossCore)
    {
        core = bossCore;
    }

    private void Update()
    {
        if (core == null || core.IsDead) return;

        if (alwaysFacePlayer && core.player != null)
        {
            core.FacePlayer();
        }
    }

    public void MoveTowards(Vector2 targetPosition, float speedMultiplier = 1f)
    {
        if (!core.CanMove) return;

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * moveSpeed * speedMultiplier, core.rb.linearVelocity.y);
        }
    }

    public void MoveTowardsPlayer(float speedMultiplier = 1f)
    {
        if (core.player == null) return;
        MoveTowards(core.PlayerPosition, speedMultiplier);
    }

    public void MoveAwayFromPlayer(float speedMultiplier = 1f)
    {
        if (core.player == null) return;

        Vector2 direction = -core.DirectionToPlayer();

        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(direction.x * moveSpeed * speedMultiplier, core.rb.linearVelocity.y);
        }
    }

    public void Stop()
    {
        if (core.rb != null)
        {
            core.rb.linearVelocity = new Vector2(0, core.rb.linearVelocity.y);
        }
    }
}
