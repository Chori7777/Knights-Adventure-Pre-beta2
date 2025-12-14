using System;
using System.Collections;
using UnityEngine;

public class RadialRingBurstAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform origin;
    [SerializeField] private int rings = 3;
    [SerializeField] private int bulletsPerRing = 24;
    [SerializeField] private float ringDelay = 0.25f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletLifetime = 4f;
    [SerializeField] private float angleOffsetPerRing = 7.5f;
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
        routine = StartCoroutine(Run());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator Run()
    {
        if (bulletPrefab == null) { OnFinished?.Invoke(); yield break; }
        Vector3 center = origin != null ? origin.position : transform.position;
        float baseAngle = 0f;
        for (int r = 0; r < rings; r++)
        {
            float step = 360f / Mathf.Max(1, bulletsPerRing);
            for (int i = 0; i < bulletsPerRing; i++)
            {
                float ang = baseAngle + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)).normalized;
                GameObject b = Instantiate(bulletPrefab, center, Quaternion.identity);
                var rb = b.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = dir * bulletSpeed;
                if (bulletLifetime > 0f) Destroy(b, bulletLifetime);
            }
            baseAngle += angleOffsetPerRing;
            yield return new WaitForSeconds(ringDelay);
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
