using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossTownFallingRocksAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform spawnTopRef;
    [SerializeField] private float spawnWidth = 12f;
    [SerializeField] private int rockCount = 6;
    [SerializeField] private float spawnInterval = 0.25f;
    [SerializeField] private float fallSpeed = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<GameObject> spawned = new List<GameObject>();

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
        routine = StartCoroutine(SpawnRocks());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        for (int i = 0; i < spawned.Count; i++)
        {
            var go = spawned[i];
            if (go != null) Destroy(go);
        }
        spawned.Clear();
    }

    private IEnumerator SpawnRocks()
    {
        if (rockPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector3 top = spawnTopRef != null ? spawnTopRef.position : transform.position + Vector3.up * 6f;
        for (int i = 0; i < rockCount; i++)
        {
            Vector3 pos = new Vector3(
                top.x + UnityEngine.Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f),
                top.y,
                0f
            );
            GameObject r = Instantiate(rockPrefab, pos, Quaternion.identity);
            spawned.Add(r);
            StartCoroutine(FallAndShake(r));
            yield return new WaitForSeconds(spawnInterval);
        }
        float t = 0f;
        while (t < 3f)
        {
            spawned.RemoveAll(g => g == null);
            if (spawned.Count == 0) break;
            t += Time.deltaTime;
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private IEnumerator FallAndShake(GameObject r)
    {
        var rb = r.GetComponent<Rigidbody2D>();
        while (r != null)
        {
            Vector3 p = r.transform.position;
            if (Physics2D.Raycast(p, Vector2.down, groundCheckDistance, groundLayer))
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                TryShakeCamera();
                break;
            }
            if (rb != null)
                rb.linearVelocity = Vector2.down * fallSpeed;
            else
                r.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }
        if (r != null) Destroy(r);
    }

    private void TryShakeCamera()
    {
        var vfx = UnityEngine.Object.FindFirstObjectByType<TrueFinalBossVisualEffects>();
        if (vfx != null)
        {
            vfx.ShakeCamera(shakeDuration, shakeIntensity);
            return;
        }
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position += new Vector3(
            UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
            UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
            0f
        );
    }
}
