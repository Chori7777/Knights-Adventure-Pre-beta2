using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossForestGapWaveOrbsAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform startEdge;
    [SerializeField] private Transform endEdge;
    [SerializeField] private Transform player;
    [SerializeField] private int rows = 9;
    [SerializeField] private int gapCenterSize = 1;
    [SerializeField] private float spacingY = 0.8f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float homingSpeed = 10f;
    [SerializeField] private float homingDuration = 2.0f;
    [SerializeField] private Color reTargetColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private float arrivalXEpsilon = 0.05f;
    [SerializeField] private float maxWaitBeforeRetarget = 3.0f;
    [SerializeField] private bool useSnapshotRetarget = true;
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
        routine = StartCoroutine(WaveThenRetarget());
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

    private IEnumerator WaveThenRetarget()
    {
        if (orbPrefab == null || startEdge == null || endEdge == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector3 dir = (endEdge.position - startEdge.position).normalized;
        float verticalCenterY = startEdge.position.y;
        int gapStartRow = rows / 2 - gapCenterSize / 2;
        for (int i = 0; i < rows; i++)
        {
            if (i >= gapStartRow && i < gapStartRow + gapCenterSize) continue;
            float y = verticalCenterY + (i - rows / 2) * spacingY;
            Vector3 pos = new Vector3(startEdge.position.x, y, 0f);
            GameObject o = Instantiate(orbPrefab, pos, Quaternion.identity);
            spawned.Add(o);
            StartCoroutine(MoveToEdge(o, endEdge.position, dir));
            yield return null;
        }
        float wait = 0f;
        while (!AllReachedEndX() && wait < maxWaitBeforeRetarget)
        {
            wait += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < spawned.Count; i++)
        {
            var go = spawned[i];
            if (go == null) continue;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = reTargetColor;
            if (useSnapshotRetarget)
            {
                Vector3 snapshot = player != null ? player.position : go.transform.position + Vector3.left;
                StartCoroutine(HomeToPoint(go, snapshot));
            }
            else
            {
                StartCoroutine(HomeToPlayer(go));
            }
        }
        yield return new WaitForSeconds(homingDuration);
        routine = null;
        OnFinished?.Invoke();
    }

    private bool AllReachedEndX()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            var go = spawned[i];
            if (go == null) continue;
            if (Mathf.Abs(go.transform.position.x - endEdge.position.x) > arrivalXEpsilon)
                return false;
        }
        return true;
    }

    private IEnumerator MoveToEdge(GameObject o, Vector3 target, Vector3 dir)
    {
        var rb = o.GetComponent<Rigidbody2D>();
        while (o != null && Vector3.Distance(o.transform.position, target) > 0.01f)
        {
            if (rb != null)
                rb.linearVelocity = (Vector2)dir * moveSpeed;
            else
                o.transform.position = Vector3.MoveTowards(o.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator HomeToPlayer(GameObject o)
    {
        float t = 0f;
        var rb = o.GetComponent<Rigidbody2D>();
        while (o != null && t < homingDuration)
        {
            Vector3 target = player != null ? player.position : o.transform.position + Vector3.left;
            Vector2 dir = ((Vector2)(target - o.transform.position)).normalized;
            if (rb != null)
                rb.linearVelocity = dir * homingSpeed;
            else
                o.transform.position += new Vector3(dir.x, dir.y, 0f) * homingSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (o != null) Destroy(o);
    }

    private IEnumerator HomeToPoint(GameObject o, Vector3 targetPoint)
    {
        float t = 0f;
        var rb = o.GetComponent<Rigidbody2D>();
        Vector2 dir = ((Vector2)(targetPoint - o.transform.position)).normalized;
        while (o != null && t < homingDuration)
        {
            if (rb != null)
                rb.linearVelocity = dir * homingSpeed;
            else
                o.transform.position += new Vector3(dir.x, dir.y, 0f) * homingSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (o != null) Destroy(o);
    }
}
