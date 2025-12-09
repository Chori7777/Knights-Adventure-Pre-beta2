using System;
using System.Collections;
using UnityEngine;

public class RafagaAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnSpreadDegrees = 30f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float fireInterval = 0.05f;
    [SerializeField] private Transform player;
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
        routine = StartCoroutine(FireBurst());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator FireBurst()
    {
        if (projectileSpawn == null || projectilePrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector2 baseDir = Vector2.right;
        if (player != null)
            baseDir = ((Vector2)player.position - (Vector2)projectileSpawn.position).normalized;
        float half = spawnSpreadDegrees * 0.5f;
        for (int i = 0; i < spawnCount; i++)
        {
            float t = spawnCount <= 1 ? 0f : (float)i / (spawnCount - 1);
            float angle = Mathf.Lerp(-half, half, t);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
            GameObject p = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir.normalized * projectileSpeed;
            else
            {
                p.transform.right = new Vector3(dir.x, dir.y, 0f);
                StartCoroutine(MoveNoRigidbody(p, dir));
            }
            yield return new WaitForSeconds(fireInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private IEnumerator MoveNoRigidbody(GameObject p, Vector2 dir)
    {
        float t = 0f;
        Vector3 d = new Vector3(dir.x, dir.y, 0f).normalized;
        while (p != null && (projectileLifetime <= 0f || t < projectileLifetime))
        {
            p.transform.position += d * projectileSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (p != null && projectileLifetime > 0f)
            Destroy(p);
    }
}
