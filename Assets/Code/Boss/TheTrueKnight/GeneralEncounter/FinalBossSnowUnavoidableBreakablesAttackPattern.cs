using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossSnowUnavoidableBreakablesAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject breakablePrefab;
    [SerializeField] private Transform areaLeft;
    [SerializeField] private Transform areaRight;
    [SerializeField] private Transform areaBottom;
    [SerializeField] private Transform areaTop;
    [SerializeField] private int waves = 3;
    [SerializeField] private int objectsPerWave = 6;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float gapBetweenWaves = 0.6f;
    [SerializeField] private bool alternateSides = true;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<BreakableMover> movers = new List<BreakableMover>();

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
        Cleanup();
    }

    private IEnumerator Run()
    {
        for (int w = 0; w < waves; w++)
        {
            bool left = alternateSides ? (w % 2 == 0) : UnityEngine.Random.value < 0.5f;
            SpawnWave(left);
            float t = 0f;
            while (t < gapBetweenWaves)
            {
                UpdateMovers();
                t += Time.deltaTime;
                yield return null;
            }
        }
        bool any = true;
        while (any)
        {
            any = false;
            for (int i = 0; i < movers.Count; i++)
            {
                var m = movers[i];
                if (m == null) continue;
                m.Update(speed);
                if (m.IsAlive) any = true;
            }
            yield return null;
        }
        Cleanup();
        routine = null;
        OnFinished?.Invoke();
    }

    private void SpawnWave(bool fromLeft)
    {
        if (breakablePrefab == null) return;
        for (int i = 0; i < objectsPerWave; i++)
        {
            float y = Mathf.Lerp(areaBottom.position.y, areaTop.position.y, (i + 0.5f) / objectsPerWave);
            float x = fromLeft ? areaLeft.position.x : areaRight.position.x;
            var go = Instantiate(breakablePrefab, new Vector3(x, y, 0f), Quaternion.identity);
            go.tag = "Breakable";
            var m = new BreakableMover(go, fromLeft ? Vector2.right : Vector2.left, areaLeft.position.x, areaRight.position.x);
            movers.Add(m);
        }
    }

    private void UpdateMovers()
    {
        for (int i = 0; i < movers.Count; i++)
        {
            var m = movers[i];
            if (m == null) continue;
            m.Update(speed);
        }
    }

    private void Cleanup()
    {
        for (int i = 0; i < movers.Count; i++)
        {
            var m = movers[i];
            if (m != null) m.Destroy();
        }
        movers.Clear();
    }
}

public class BreakableMover
{
    private readonly GameObject go;
    private readonly Vector2 dir;
    private readonly float leftX;
    private readonly float rightX;
    public bool IsAlive => go != null;

    public BreakableMover(GameObject g, Vector2 d, float lx, float rx)
    {
        go = g;
        dir = d.normalized;
        leftX = lx;
        rightX = rx;
    }

    public void Update(float speed)
    {
        if (go == null) return;
        go.transform.position += (Vector3)(dir * speed * Time.deltaTime);
        if (dir.x > 0f && go.transform.position.x > rightX + 1f) Destroy();
        if (dir.x < 0f && go.transform.position.x < leftX - 1f) Destroy();
    }

    public void Destroy()
    {
        if (go != null) UnityEngine.Object.Destroy(go);
    }
}
