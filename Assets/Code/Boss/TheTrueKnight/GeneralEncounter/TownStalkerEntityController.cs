using System.Collections;
using UnityEngine;

public class TownStalkerEntityController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float passiveSpeed = 1.2f;
    [SerializeField] private float aggressiveSpeed = 8f;
    [SerializeField] private float hoverAmplitude = 0.2f;
    [SerializeField] private float hoverFrequency = 2f;
    [SerializeField] private float drainDistance = 2.5f;
    [SerializeField] private float drainRate = 5f; // unidades de tiempo por segundo
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;
    [SerializeField] private bool forceAggressive = false;
    [SerializeField] private bool dashNonStopToPlayer = true;
    [SerializeField] private int dashDamage = 2;
    [SerializeField] private float dashHitDistance = 1.2f;
    [SerializeField] private float windupDuration = 0.6f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float timeBetweenDashes = 0.6f;
    [SerializeField] private float repositionDistance = 4f;
    [SerializeField] private float retreatDrainThreshold = 30f;
    [SerializeField] private float retreatCooldown = 3f;
    [SerializeField] private GameObject dashTrailPrefab;
    [SerializeField] private float trailSpawnInterval = 0.035f;
    [SerializeField] private float trailLifetime = 0.25f;
    [SerializeField] private AudioClip proximityHum;
    [SerializeField] private float proximityMaxVolume = 0.8f;
    [SerializeField] private float proximityMaxDistance = 8f;
    [SerializeField] private bool anchorBoundariesToCamera = true;
    [SerializeField] private float boundaryMargin = 2f;
    [SerializeField] private float boundaryVerticalOffset = 0f;
    [SerializeField] private float flipMinDeltaX = 0.2f;
    [SerializeField] private float flipCooldown = 0.15f;
    [SerializeField] private bool enableNearRetreat = true;
    [SerializeField] private float nearRetreatDistance = 1.5f;

    private float hoverPhase = 0f;
    private bool aggressive = false;
    private float drainAccumulated = 0f;
    private enum StalkerState { PassiveChase, Windup, Dash, Reposition, Retreat, Resting }
    private StalkerState state = StalkerState.PassiveChase;
    private float stateTimer = 0f;
    private Vector3 dashDir = Vector3.right;
    private bool dashHitApplied = false;
    private Vector3 repositionTarget;
    private Vector3 retreatTarget;
    private float trailTimer = 0f;
    private AudioSource proximitySource;
    private SpriteRenderer sr;
    private float lastFlipTime = -10f;

    public float CurrentScoreTime { get; set; } = 30f; // asignado externamente por el sistema de score

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        proximitySource = GetComponent<AudioSource>();
        if (proximitySource == null) proximitySource = gameObject.AddComponent<AudioSource>();
        proximitySource.loop = true;
        proximitySource.playOnAwake = false;
        proximitySource.spatialBlend = 0f;
        proximitySource.volume = 0f;
        proximitySource.clip = proximityHum;
        if (proximitySource.clip != null) proximitySource.Play();
    }

    private void Update()
    {
        if (anchorBoundariesToCamera) UpdateBoundaryAnchorsToCamera();
        hoverPhase += Time.deltaTime * hoverFrequency;
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(hoverPhase) * hoverAmplitude * Time.deltaTime;
        transform.position = pos;

        if (player == null) return;

        aggressive = forceAggressive || CurrentScoreTime <= 0f;

        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        UpdateProximityNoise(dist);
        if (sr != null)
        {
            float dx = player.position.x - transform.position.x;
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

        if (state == StalkerState.PassiveChase)
        {
            float speed = passiveSpeed;
            if (aggressive)
            {
                state = StalkerState.Windup;
                stateTimer = windupDuration;
            }
            else
            {
                if (enableNearRetreat && dist <= nearRetreatDistance)
                {
                    retreatTarget = ChooseRetreatTarget();
                    state = StalkerState.Retreat;
                    return;
                }
                transform.position += dir * speed * Time.deltaTime;
                if (dist < drainDistance)
                {
                    float drained = drainRate * Time.deltaTime;
                    drainAccumulated += drained;
                    CurrentScoreTime = Mathf.Max(0f, CurrentScoreTime - drained);
                    if (PlayerHealthUI.Instance != null)
                    {
                        PlayerHealthUI.Instance.AddTime(-drained);
                    }
                    if (drainAccumulated >= retreatDrainThreshold && dist < drainDistance * 0.7f)
                    {
                        retreatTarget = ChooseRetreatTarget();
                        state = StalkerState.Retreat;
                    }
                }
            }
            return;
        }

        if (state == StalkerState.Windup)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                dashDir = (player.position - transform.position).normalized;
                dashHitApplied = false;
                trailTimer = 0f;
                state = StalkerState.Dash;
                stateTimer = dashDuration;
            }
            return;
        }

        if (state == StalkerState.Dash)
        {
            transform.position += dashDir * aggressiveSpeed * Time.deltaTime;
            stateTimer -= Time.deltaTime;
            trailTimer += Time.deltaTime;
            if (trailTimer >= trailSpawnInterval)
            {
                trailTimer = 0f;
                SpawnAfterimage();
            }
            if (!dashHitApplied && dist < dashHitDistance)
            {
                var pl = player.GetComponent<playerLife>();
                if (pl != null) pl.TakeDamage(transform.position, dashDamage);
                dashHitApplied = true;
            }
            if (stateTimer <= 0f)
            {
                repositionTarget = player.position - dashDir * repositionDistance;
                state = StalkerState.Reposition;
                stateTimer = timeBetweenDashes;
            }
            return;
        }

        if (state == StalkerState.Reposition)
        {
            Vector3 toTarget = (repositionTarget - transform.position);
            Vector3 step = toTarget.normalized * passiveSpeed * Time.deltaTime;
            if (step.sqrMagnitude >= toTarget.sqrMagnitude || Vector3.Distance(transform.position, repositionTarget) < 0.2f)
            {
                transform.position = repositionTarget;
                stateTimer -= Time.deltaTime;
            }
            else
            {
                transform.position += step;
                stateTimer -= Time.deltaTime;
            }
            if (stateTimer <= 0f)
            {
                state = StalkerState.Windup;
                stateTimer = windupDuration;
            }
            return;
        }

        if (state == StalkerState.Retreat)
        {
            Vector3 toTarget = (retreatTarget - transform.position);
            Vector3 step = toTarget.normalized * aggressiveSpeed * 0.8f * Time.deltaTime;
            if (step.sqrMagnitude >= toTarget.sqrMagnitude || Vector3.Distance(transform.position, retreatTarget) < 0.25f)
            {
                transform.position = retreatTarget;
                state = StalkerState.Resting;
                stateTimer = retreatCooldown;
                drainAccumulated = 0f;
            }
            else
            {
                transform.position += step;
            }
            return;
        }

        if (state == StalkerState.Resting)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                aggressive = false;
                state = StalkerState.PassiveChase;
            }
            return;
        }
    }

    private void UpdateBoundaryAnchorsToCamera()
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic) return;
        Vector3 camPos = cam.transform.position;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        float leftX = camPos.x - halfWidth + boundaryMargin;
        float rightX = camPos.x + halfWidth - boundaryMargin;
        float y = camPos.y + boundaryVerticalOffset;
        if (leftBoundary == null)
        {
            var go = new GameObject("StalkerLeftBoundary_Auto");
            leftBoundary = go.transform;
        }
        if (rightBoundary == null)
        {
            var go = new GameObject("StalkerRightBoundary_Auto");
            rightBoundary = go.transform;
        }
        leftBoundary.position = new Vector3(leftX, y, 0f);
        rightBoundary.position = new Vector3(rightX, y, 0f);
    }

    private void UpdateProximityNoise(float dist)
    {
        if (proximitySource == null) return;
        float t = 1f - Mathf.InverseLerp(0.5f, proximityMaxDistance, dist);
        float target = Mathf.Clamp01(t) * proximityMaxVolume * (AudioManager.Instance != null ? AudioManager.Instance.sfxVolume * AudioManager.Instance.masterVolume : 1f);
        proximitySource.volume = Mathf.MoveTowards(proximitySource.volume, target, Time.deltaTime * 2f);
    }

    private Vector3 ChooseRetreatTarget()
    {
        bool useLeft = leftBoundary != null && (rightBoundary == null || Vector3.Distance(player.position, leftBoundary.position) > Vector3.Distance(player.position, rightBoundary.position));
        if (useLeft) return leftBoundary.position;
        if (rightBoundary != null) return rightBoundary.position;
        return transform.position + (transform.position - player.position).normalized * repositionDistance * 2f;
    }

    private void SpawnAfterimage()
    {
        if (dashTrailPrefab != null)
        {
            var a = Instantiate(dashTrailPrefab, transform.position, Quaternion.identity);
            Destroy(a, trailLifetime);
            return;
        }
        if (sr == null) return;
        var go = new GameObject("StalkerAfterimage");
        var c = go.AddComponent<SpriteRenderer>();
        c.sprite = sr.sprite;
        c.flipX = sr.flipX;
        c.color = new Color(1f, 1f, 1f, 0.7f);
        c.sortingLayerID = sr.sortingLayerID;
        c.sortingOrder = sr.sortingOrder - 1;
        go.transform.position = transform.position;
        StartCoroutine(FadeAndDestroy(c));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer c)
    {
        float t = trailLifetime;
        while (t > 0f && c != null)
        {
            t -= Time.deltaTime;
            var col = c.color;
            col.a = Mathf.Clamp01(t / trailLifetime);
            c.color = col;
            yield return null;
        }
        if (c != null) Destroy(c.gameObject);
    }
}
