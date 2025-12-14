using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossTownBatsDiveRestAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject batPrefab;
    [SerializeField] private Transform spawnTopRef;
    [SerializeField] private Transform[] restPoints;
    [SerializeField] private int batCount = 6;
    [SerializeField] private float spawnInterval = 0.2f;
    [SerializeField] private float diveSpeed = 12f;
    [SerializeField] private float ascendSpeed = 10f;
    [SerializeField] private float restDuration = 1.5f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<GameObject> bats = new List<GameObject>();

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
        routine = StartCoroutine(SpawnBats());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        for (int i = 0; i < bats.Count; i++)
        {
            var b = bats[i];
            if (b != null) Destroy(b);
        }
        bats.Clear();
    }

    private IEnumerator SpawnBats()
    {
        if (batPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector3 top = spawnTopRef != null ? spawnTopRef.position : transform.position + Vector3.up * 6f;
        for (int i = 0; i < batCount; i++)
        {
            Vector3 pos = new Vector3(top.x + UnityEngine.Random.Range(-4f, 4f), top.y, 0f);
            GameObject b = Instantiate(batPrefab, pos, Quaternion.identity);
            bats.Add(b);
            StartCoroutine(BatRoutine(b));
            yield return new WaitForSeconds(spawnInterval);
        }
        float t = 0f;
        while (t < 6f)
        {
            bats.RemoveAll(g => g == null);
            if (bats.Count == 0) break;
            t += Time.deltaTime;
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private IEnumerator BatRoutine(GameObject b)
    {
        if (b == null) yield break;
        var rb = b.GetComponent<Rigidbody2D>();
        Vector2 diveDir = (UnityEngine.Random.value < 0.5f) ? new Vector2(-1f, -1f) : new Vector2(1f, -1f);
        diveDir.Normalize();
        float diveTime = 0.8f;
        float t = 0f;
        while (b != null && t < diveTime)
        {
            if (rb != null) rb.linearVelocity = diveDir * diveSpeed;
            else b.transform.position += new Vector3(diveDir.x, diveDir.y, 0f) * diveSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        Transform rest = PickRestPoint();
        if (rest == null)
        {
            if (b != null) Destroy(b);
            yield break;
        }
        while (b != null && Vector3.Distance(b.transform.position, rest.position) > 0.05f)
        {
            Vector3 dir = (rest.position - b.transform.position).normalized;
            if (rb != null) rb.linearVelocity = (Vector2)dir * ascendSpeed;
            else b.transform.position += dir * ascendSpeed * Time.deltaTime;
            yield return null;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(restDuration);
        if (b != null) Destroy(b);
    }

    private Transform PickRestPoint()
    {
        if (restPoints == null || restPoints.Length == 0) return null;
        int i = UnityEngine.Random.Range(0, restPoints.Length);
        return restPoints[i];
    }
}
