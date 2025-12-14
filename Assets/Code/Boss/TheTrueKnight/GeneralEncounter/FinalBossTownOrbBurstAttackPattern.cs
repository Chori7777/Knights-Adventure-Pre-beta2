using System;
using System.Collections;
using UnityEngine;

public class FinalBossTownOrbBurstAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private Transform player;
    [SerializeField] private int bursts = 4;
    [SerializeField] private float burstInterval = 0.6f;
    [SerializeField] private int orbsPerBurst = 5;
    [SerializeField] private float orbSpeed = 10f;
    [SerializeField] private float spreadAngle = 20f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;

    private void OnEnable()
    {
        if (autoStartOnEnable) StartAttack();
    }

    private void OnDisable()
    {
        StopAttack();
    }

    public void StartAttack()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(BurstRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator BurstRoutine()
    {
        if (orbPrefab == null || shootOrigin == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        for (int b = 0; b < bursts; b++)
        {
            ShootBurst();
            yield return new WaitForSeconds(burstInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private void ShootBurst()
    {
        Vector3 origin = shootOrigin.position;
        Vector2 baseDir = player != null ? (player.position - origin).normalized : Vector2.down;
        float half = spreadAngle * 0.5f;
        for (int i = 0; i < orbsPerBurst; i++)
        {
            float t = (orbsPerBurst == 1) ? 0f : (float)i / (orbsPerBurst - 1);
            float ang = Mathf.Lerp(-half, half, t);
            Vector2 dir = Rotate(baseDir, ang);
            GameObject o = Instantiate(orbPrefab, origin, Quaternion.identity);
            var rb = o.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * orbSpeed;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }
}
