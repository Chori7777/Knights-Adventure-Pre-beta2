using System;
using System.Collections;
using UnityEngine;

public class FinalBossTowerSpiralBulletRainAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform spawnA;
    [SerializeField] private Transform spawnB;
    [SerializeField] private int bulletsPerSpiral = 60;
    [SerializeField] private float angleStep = 12f;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float openingAngle = 30f;
    [SerializeField] private Vector2 openingDirection = Vector2.down;
    [SerializeField] private float spawnInterval = 0.05f;
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
        routine = StartCoroutine(SpiralRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator SpiralRoutine()
    {
        if (bulletPrefab == null || spawnA == null || spawnB == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        float angA = 0f;
        float angB = 180f;
        for (int i = 0; i < bulletsPerSpiral; i++)
        {
            TrySpawnSpiralBullet(spawnA.position, angA);
            TrySpawnSpiralBullet(spawnB.position, angB);
            angA += angleStep;
            angB -= angleStep;
            yield return new WaitForSeconds(spawnInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private void TrySpawnSpiralBullet(Vector3 origin, float angleDeg)
    {
        Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
        float gap = Vector2.Angle(dir, openingDirection);
        if (gap < openingAngle * 0.5f) return; // respeta abertura para esquivar
        GameObject b = Instantiate(bulletPrefab, origin, Quaternion.identity);
        var mover = b.GetComponent<UniversalProjectileMover>();
        if (mover != null)
        {
            float angular = Mathf.Deg2Rad * angleStep / Mathf.Max(0.0001f, spawnInterval);
            mover.EnableSpiralMovement(origin, false, 0.2f, angular, UniversalProjectileMover.SpiralType.Expanding, bulletSpeed);
            var rb = b.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            var rb = b.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir.normalized * bulletSpeed;
        }
    }
}
