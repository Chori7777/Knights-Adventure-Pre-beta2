using System;
using System.Collections;
using UnityEngine;

public class RotatingLaserTelegraphAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject telegraphLinePrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform center;
    [SerializeField] private int lines = 6;
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float telegraphTime = 1.2f;
    [SerializeField] private float fireSpeed = 12f;
    [SerializeField] private float bulletLifetime = 4f;
    [SerializeField] private float fireInterval = 0.15f;
    [SerializeField] private float lineDistance = 6f;
    [SerializeField] private bool bringLinesToFront = true;
    [SerializeField] private string sortingLayer = "";
    [SerializeField] private int sortingOrder = 9999;
    [SerializeField] private bool destroyLinesOnFinish = true;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly System.Collections.Generic.List<GameObject> telegraphs = new System.Collections.Generic.List<GameObject>();

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
        CleanupTelegraphs();
    }

    private IEnumerator Run()
    {
        if (telegraphLinePrefab == null || bulletPrefab == null) { OnFinished?.Invoke(); yield break; }
        Vector3 c = center != null ? center.position : transform.position;
        telegraphs.Clear();
        for (int i = 0; i < lines; i++)
        {
            float ang = (360f / Mathf.Max(1, lines)) * i;
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
            Vector3 pos = c + (Vector3)(dir * 0.1f);
            GameObject g = Instantiate(telegraphLinePrefab, pos, Quaternion.identity);
            g.transform.right = dir;
            var sr = g.GetComponentInChildren<SpriteRenderer>();
            if (bringLinesToFront && sr != null)
            {
                if (!string.IsNullOrEmpty(sortingLayer)) sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = sortingOrder;
            }
            telegraphs.Add(g);
        }
        float t = 0f;
        while (t < telegraphTime)
        {
            t += Time.deltaTime;
            float step = rotateSpeed * Time.deltaTime;
            for (int i = 0; i < telegraphs.Count; i++)
            {
                var g = telegraphs[i];
                if (g == null) continue;
                g.transform.Rotate(0f, 0f, -step);
            }
            yield return null;
        }
        int shots = Mathf.Max(1, Mathf.RoundToInt(telegraphTime / Mathf.Max(0.0001f, fireInterval)));
        for (int s = 0; s < shots; s++)
        {
            for (int i = 0; i < telegraphs.Count; i++)
            {
                var g = telegraphs[i];
                if (g == null) continue;
                Vector3 dir3 = g.transform.right.normalized;
                Vector3 start = c + dir3 * 0.5f;
                GameObject b = Instantiate(bulletPrefab, start, Quaternion.identity);
                var rb = b.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = (Vector2)dir3 * fireSpeed;
                if (bulletLifetime > 0f) Destroy(b, bulletLifetime);
            }
            yield return new WaitForSeconds(fireInterval);
        }
        if (destroyLinesOnFinish) CleanupTelegraphs();
        routine = null;
        OnFinished?.Invoke();
    }

    private void CleanupTelegraphs()
    {
        for (int i = 0; i < telegraphs.Count; i++)
        {
            var g = telegraphs[i];
            if (g != null) Destroy(g);
        }
        telegraphs.Clear();
    }
}
