using System;
using System.Collections;
using UnityEngine;

public class FinalBossForestCenterOrbExplosionAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform center;
    [SerializeField] private Transform[] explosionPoints;
    [SerializeField] private float chargeDuration = 2f;
    [SerializeField] private float finalScaleMultiplier = 2f;
    [SerializeField] private int orbsPerPoint = 10;
    [SerializeField] private float speedMin = 6f;
    [SerializeField] private float speedMax = 12f;
    [SerializeField] private float postLifetime = 2f;
    [SerializeField] private bool preExplodeFromPoints = false;
    [SerializeField] private int preOrbsPerPoint = 6;
    [SerializeField] private float preSpeedMin = 4f;
    [SerializeField] private float preSpeedMax = 8f;
    [SerializeField] private float preExplodeDuration = 0.8f;
    [SerializeField] private float gatherSpeed = 10f;
    [SerializeField] private float gatherDuration = 1.2f;
    [SerializeField] private bool autoStartOnEnable = false;

    private Coroutine routine;
    private System.Collections.Generic.List<GameObject> preSpawned = new System.Collections.Generic.List<GameObject>();

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
        routine = StartCoroutine(ChargeThenExplode());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator ChargeThenExplode()
    {
        if (orbPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        if (preExplodeFromPoints)
        {
            Vector3 c0 = center != null ? center.position : transform.position;
            PreSpawnBursts();
            float t0 = 0f;
            while (t0 < preExplodeDuration)
            {
                t0 += Time.deltaTime;
                yield return null;
            }
            yield return StartCoroutine(GatherPreSpawnedToCenter(c0));
        }
        Vector3 c = center != null ? center.position : transform.position;
        GameObject core = Instantiate(orbPrefab, c, Quaternion.identity);
        Vector3 baseScale = core.transform.localScale;
        float t = 0f;
        while (t < chargeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / chargeDuration);
            float s = Mathf.Lerp(1f, finalScaleMultiplier, k);
            core.transform.localScale = baseScale * s;
            yield return null;
        }
        if (core != null) Destroy(core);
        if (explosionPoints != null && explosionPoints.Length > 0)
        {
            for (int i = 0; i < explosionPoints.Length; i++)
            {
                var p = explosionPoints[i];
                if (p == null) continue;
                SpawnBurstAt(p.position);
            }
        }
        else
        {
            SpawnBurstAt(c);
        }
        yield return new WaitForSeconds(postLifetime);
        routine = null;
        OnFinished?.Invoke();
    }

    private void PreSpawnBursts()
    {
        if (explosionPoints != null && explosionPoints.Length > 0)
        {
            for (int i = 0; i < explosionPoints.Length; i++)
            {
                var p = explosionPoints[i];
                if (p == null) continue;
                PreSpawnBurstAt(p.position);
            }
        }
        else
        {
            PreSpawnBurstAt(center != null ? center.position : transform.position);
        }
    }

    private void PreSpawnBurstAt(Vector3 pos)
    {
        float step = 360f / Mathf.Max(1, preOrbsPerPoint);
        for (int i = 0; i < preOrbsPerPoint; i++)
        {
            float ang = step * i + UnityEngine.Random.Range(-10f, 10f);
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)).normalized;
            GameObject o = Instantiate(orbPrefab, pos, Quaternion.identity);
            var rb = o.GetComponent<Rigidbody2D>();
            float spd = UnityEngine.Random.Range(preSpeedMin, preSpeedMax);
            if (rb != null)
                rb.linearVelocity = dir * spd;
            preSpawned.Add(o);
        }
    }

    private IEnumerator GatherPreSpawnedToCenter(Vector3 c)
    {
        float t = 0f;
        while (t < gatherDuration)
        {
            for (int i = 0; i < preSpawned.Count; i++)
            {
                var o = preSpawned[i];
                if (o == null) continue;
                var rb = o.GetComponent<Rigidbody2D>();
                Vector2 dir = ((Vector2)(c - o.transform.position)).normalized;
                if (rb != null)
                    rb.linearVelocity = dir * gatherSpeed;
                else
                    o.transform.position = Vector3.MoveTowards(o.transform.position, c, gatherSpeed * Time.deltaTime);
            }
            t += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < preSpawned.Count; i++)
        {
            var o = preSpawned[i];
            if (o != null) Destroy(o);
        }
        preSpawned.Clear();
    }

    private void SpawnBurstAt(Vector3 pos)
    {
        float step = 360f / Mathf.Max(1, orbsPerPoint);
        for (int i = 0; i < orbsPerPoint; i++)
        {
            float ang = step * i + UnityEngine.Random.Range(-10f, 10f);
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)).normalized;
            GameObject o = Instantiate(orbPrefab, pos, Quaternion.identity);
            var rb = o.GetComponent<Rigidbody2D>();
            float spd = UnityEngine.Random.Range(speedMin, speedMax);
            if (rb != null)
                rb.linearVelocity = dir * spd;
            else
                StartCoroutine(MoveNoRb(o, dir, spd));
        }
    }

    private IEnumerator MoveNoRb(GameObject o, Vector2 dir, float spd)
    {
        float t = 0f;
        while (o != null && t < postLifetime)
        {
            o.transform.position += new Vector3(dir.x, dir.y, 0f) * spd * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (o != null) Destroy(o);
    }
}
