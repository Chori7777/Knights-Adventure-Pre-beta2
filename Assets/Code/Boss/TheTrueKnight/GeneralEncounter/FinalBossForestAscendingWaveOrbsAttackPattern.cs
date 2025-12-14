using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossForestAscendingWaveOrbsAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform lineYRef;
    [SerializeField] private Transform topYRef;
    [SerializeField] private int count = 8;
    [SerializeField] private float spawnWidth = 12f;
    [SerializeField] private float ascendSpeed = 12f;
    [SerializeField] private float shootDownSpeed = 14f;
    [SerializeField] private float shootDelay = 0.2f;
    [SerializeField] private float postDuration = 1.5f;
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
        routine = StartCoroutine(AscendThenShoot());
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

    private IEnumerator AscendThenShoot()
    {
        if (orbPrefab == null || lineYRef == null || topYRef == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        float y = lineYRef.position.y;
        for (int i = 0; i < count; i++)
        {
            float x = transform.position.x + UnityEngine.Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f);
            Vector3 pos = new Vector3(x, y, 0f);
            GameObject o = Instantiate(orbPrefab, pos, Quaternion.identity);
            spawned.Add(o);
            StartCoroutine(MoveVertical(o, topYRef.position.y, ascendSpeed));
            yield return null;
        }
        yield return new WaitForSeconds(shootDelay);
        for (int i = 0; i < spawned.Count; i++)
        {
            var o = spawned[i];
            if (o == null) continue;
            StartCoroutine(ShootDown(o));
        }
        yield return new WaitForSeconds(postDuration);
        routine = null;
        OnFinished?.Invoke();
    }

    private IEnumerator MoveVertical(GameObject o, float targetY, float speed)
    {
        var rb = o.GetComponent<Rigidbody2D>();
        while (o != null && Mathf.Abs(o.transform.position.y - targetY) > 0.01f)
        {
            Vector3 p = o.transform.position;
            Vector3 t = new Vector3(p.x, targetY, 0f);
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, Mathf.Sign(targetY - p.y) * speed);
            else
                o.transform.position = Vector3.MoveTowards(p, t, speed * Time.deltaTime);
            yield return null;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator ShootDown(GameObject o)
    {
        var rb = o.GetComponent<Rigidbody2D>();
        float xDrift = UnityEngine.Random.Range(-4f, 4f);
        float t = 0f;
        while (o != null && t < postDuration)
        {
            Vector2 vel = new Vector2(xDrift, -shootDownSpeed);
            if (rb != null)
                rb.linearVelocity = vel;
            else
                o.transform.position += new Vector3(vel.x, vel.y, 0f) * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        if (o != null) Destroy(o);
    }
}
