using System;
using System.Collections;
using UnityEngine;

public class RandomSwordsAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float spawnInterval = 0.1f;
    [SerializeField] private float swordSpeed = 9f;
    [SerializeField] private float swordLifetime = 5f;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private bool usePlayerAsTarget = true;
    [SerializeField] private Transform targetOverride;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

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
        routine = StartCoroutine(SpawnRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        if (swordPrefab == null) { OnFinished?.Invoke(); yield break; }
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private void SpawnOne()
    {
        Vector3 target = GetTargetPoint();
        Vector3 pos = GetSpawnPoint();
        GameObject go = Instantiate(swordPrefab, pos, Quaternion.identity);
        Vector3 dir = (target - pos).normalized;
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = (Vector2)dir * swordSpeed;
        }
        else
        {
            go.transform.right = dir;
            StartCoroutine(MoveNoRigidbody(go, dir));
        }
    }

    private Vector3 GetTargetPoint()
    {
        if (usePlayerAsTarget)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) return p.transform.position;
        }
        if (targetOverride != null) return targetOverride.position;
        if (cam == null) cam = Camera.main;
        if (cam != null) return cam.transform.position;
        return Vector3.zero;
    }

    private Vector3 GetSpawnPoint()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;
        float leftX = c.x - halfW - spawnOffset;
        float rightX = c.x + halfW + spawnOffset;
        float topY = c.y + halfH + spawnOffset;
        float bottomY = c.y - halfH - spawnOffset;
        int side = UnityEngine.Random.Range(0, 4);
        if (side == 0)
        {
            float y = UnityEngine.Random.Range(bottomY + spawnOffset, topY - spawnOffset);
            return new Vector3(leftX, y, 0f);
        }
        else if (side == 1)
        {
            float y = UnityEngine.Random.Range(bottomY + spawnOffset, topY - spawnOffset);
            return new Vector3(rightX, y, 0f);
        }
        else if (side == 2)
        {
            float x = UnityEngine.Random.Range(leftX + spawnOffset, rightX - spawnOffset);
            return new Vector3(x, topY, 0f);
        }
        else
        {
            float x = UnityEngine.Random.Range(leftX + spawnOffset, rightX - spawnOffset);
            return new Vector3(x, bottomY, 0f);
        }
    }

    private IEnumerator MoveNoRigidbody(GameObject p, Vector3 dir)
    {
        float t = 0f;
        Vector3 d = dir.normalized;
        while (p != null && (swordLifetime <= 0f || t < swordLifetime))
        {
            p.transform.position += d * swordSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (p != null && swordLifetime > 0f)
            Destroy(p);
    }
}
