using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossSnowGlitchChasersAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject glitchPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform areaLeft;
    [SerializeField] private Transform areaRight;
    [SerializeField] private Transform areaBottom;
    [SerializeField] private Transform areaTop;
    [SerializeField] private int spawnCount = 6;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float activeDuration = 3f;
    [SerializeField] private float gapBetweenSpawns = 0.1f;
    [SerializeField] private bool autoStartOnEnable = false;

    private Coroutine routine;
    private readonly List<GameObject> glitches = new List<GameObject>();

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
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOne();
            if (gapBetweenSpawns > 0f) yield return new WaitForSeconds(gapBetweenSpawns);
        }
        float t = 0f;
        while (t < activeDuration)
        {
            UpdateGlitches();
            t += Time.deltaTime;
            yield return null;
        }
        Cleanup();
        routine = null;
        OnFinished?.Invoke();
    }

    private void SpawnOne()
    {
        if (glitchPrefab == null) return;
        float y = UnityEngine.Random.Range(areaBottom.position.y, areaTop.position.y);
        bool left = UnityEngine.Random.value < 0.5f;
        Vector3 pos = new Vector3(left ? areaLeft.position.x : areaRight.position.x, y, 0f);
        var go = Instantiate(glitchPrefab, pos, Quaternion.identity);
        glitches.Add(go);
        var ctrl = go.GetComponent<GlitchChaser>();
        if (ctrl == null) ctrl = go.AddComponent<GlitchChaser>();
        ctrl.Init(player, detectionRadius, dashSpeed, linePrefab);
    }

    private void UpdateGlitches()
    {
        for (int i = glitches.Count - 1; i >= 0; i--)
        {
            var g = glitches[i];
            if (g == null) { glitches.RemoveAt(i); continue; }
        }
    }

    private void Cleanup()
    {
        for (int i = 0; i < glitches.Count; i++)
        {
            var g = glitches[i];
            if (g != null) Destroy(g);
        }
        glitches.Clear();
    }
}

public class GlitchChaser : MonoBehaviour
{
    private Transform player;
    private float radius;
    private float speed;
    private GameObject linePrefab;
    private bool locked;
    private Vector3 lockedTarget;
    private GameObject lineInstance;

    public void Init(Transform p, float r, float s, GameObject linePf)
    {
        player = p;
        radius = r;
        speed = s;
        linePrefab = linePf;
    }

    private void Update()
    {
        if (player == null) return;
        if (!locked)
        {
            float d = Vector3.Distance(transform.position, player.position);
            if (d <= radius)
            {
                locked = true;
                lockedTarget = player.position;
                if (linePrefab != null)
                {
                    lineInstance = Instantiate(linePrefab, transform.position, Quaternion.identity);
                    Vector3 dir = (lockedTarget - transform.position).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    lineInstance.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    float len = Vector3.Distance(transform.position, lockedTarget);
                    lineInstance.transform.localScale = new Vector3(lineInstance.transform.localScale.x, len, lineInstance.transform.localScale.z);
                }
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, lockedTarget, speed * Time.deltaTime);
        }
    }
}
