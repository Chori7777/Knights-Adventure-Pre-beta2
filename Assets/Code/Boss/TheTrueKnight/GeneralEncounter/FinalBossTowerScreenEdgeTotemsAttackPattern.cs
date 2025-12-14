using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossTowerScreenEdgeTotemsAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject totemPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [SerializeField] private Transform player;
    [SerializeField] private int totemsPerSide = 2;
    [SerializeField] private float fireInterval = 0.6f;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float duration = 6f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<GameObject> spawnedTotems = new List<GameObject>();

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
        routine = StartCoroutine(TotemsRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        for (int i = 0; i < spawnedTotems.Count; i++)
        {
            var t = spawnedTotems[i];
            if (t != null) Destroy(t);
        }
        spawnedTotems.Clear();
    }

    private IEnumerator TotemsRoutine()
    {
        if (totemPrefab == null || bulletPrefab == null || leftEdge == null || rightEdge == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        SpawnEdgeTotems(leftEdge.position, -1);
        SpawnEdgeTotems(rightEdge.position, 1);
        float t = 0f;
        float next = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            next -= Time.deltaTime;
            if (next <= 0f)
            {
                FireAll();
                next = fireInterval;
            }
            yield return null;
        }
        StopAttack();
        routine = null;
        OnFinished?.Invoke();
    }

    private void SpawnEdgeTotems(Vector3 edgePos, int side)
    {
        for (int i = 0; i < totemsPerSide; i++)
        {
            float y = edgePos.y + (i - totemsPerSide / 2f) * 1.5f;
            Vector3 pos = new Vector3(edgePos.x, y, 0f);
            GameObject t = Instantiate(totemPrefab, pos, Quaternion.identity);
            spawnedTotems.Add(t);
        }
    }

    private void FireAll()
    {
        for (int i = 0; i < spawnedTotems.Count; i++)
        {
            var t = spawnedTotems[i];
            if (t == null) continue;
            Vector3 origin = t.transform.position;
            Vector2 dir = player != null ? (player.position - origin).normalized : Vector2.left;
            GameObject b = Instantiate(bulletPrefab, origin, Quaternion.identity);
            var rb = b.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * bulletSpeed;
        }
    }
}
