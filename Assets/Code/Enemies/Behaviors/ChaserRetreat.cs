using UnityEngine;
using System.Collections;

public class ChaserRetreat : MonoBehaviour
{
    [SerializeField] private float retreatTriggerRange = 1.5f;
    [SerializeField] private float retreatSpeed = 3f;
    [SerializeField] private float retreatDuration = 0.75f;
    [SerializeField] private float retreatCooldown = 2f;

    private EnemyCore core;
    private EnemyMovement movement;
    private EnemySmartMovement smartMovement;
    private float lastRetreatTime = -10f;
    private bool isRetreating = false;

    private void Awake()
    {
        core = GetComponent<EnemyCore>();
        movement = GetComponent<EnemyMovement>();
        smartMovement = GetComponent<EnemySmartMovement>();
    }

    private void Update()
    {
        if (core == null || core.player == null) return;
        if (isRetreating) return;
        if (!core.CanMove) return;

        float d = core.DistanceToPlayer();
        if (d <= retreatTriggerRange && Time.time >= lastRetreatTime + retreatCooldown)
        {
            StartCoroutine(RetreatRoutine());
        }
    }

    private IEnumerator RetreatRoutine()
    {
        isRetreating = true;
        lastRetreatTime = Time.time;
        bool movementWasEnabled = movement != null && movement.enabled;
        bool smartWasEnabled = smartMovement != null && smartMovement.enabled;
        if (movement != null) movement.enabled = false;
        if (smartMovement != null) smartMovement.enabled = false;

        float timer = 0f;
        while (timer < retreatDuration)
        {
            Vector2 dirToPlayer = core.DirectionToPlayer();
            if (core.rb != null && core.rb.bodyType != RigidbodyType2D.Kinematic)
            {
                float vx = -dirToPlayer.x * retreatSpeed;
                core.rb.linearVelocity = new Vector2(vx, core.rb.linearVelocity.y);
            }
            else
            {
                Vector3 step = (Vector3)(-dirToPlayer.normalized * retreatSpeed * Time.deltaTime);
                transform.position += step;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (movement != null) movement.enabled = movementWasEnabled;
        if (smartMovement != null) smartMovement.enabled = smartWasEnabled;
        isRetreating = false;
    }
}
