using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatherRainHomingAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject featherPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform spawnTopRef;
    [SerializeField] private int featherCount = 12;
    [SerializeField] private float spawnWidth = 12f;
    [SerializeField] private float spawnHeight = 6f;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float fallTime = 0.6f;
    [SerializeField] private float homingSpeed = 12f;
    [SerializeField] private float homingTurnRate = 360f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private float spawnInterval = 0.06f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<GameObject> spawned = new List<GameObject>();
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
        routine = StartCoroutine(RainThenHome());
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

    private IEnumerator RainThenHome()
    {
        if (featherPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector3 top = GetTopSpawn();
        for (int i = 0; i < featherCount; i++)
        {
            Vector3 pos = new Vector3(
                top.x + UnityEngine.Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f),
                top.y + spawnHeight,
                0f
            );
            GameObject f = Instantiate(featherPrefab, pos, Quaternion.identity);
            spawned.Add(f);
            StartCoroutine(MoveFeather(f));
            yield return new WaitForSeconds(spawnInterval);
        }
        float t = 0f;
        while (t < maxLifetime)
        {
            spawned.RemoveAll(g => g == null);
            if (spawned.Count == 0) break;
            t += Time.deltaTime;
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private Vector3 GetTopSpawn()
    {
        if (spawnTopRef != null) return spawnTopRef.position;
        if (cam == null) cam = Camera.main;
        if (cam != null) return new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        return transform.position;
    }

    private IEnumerator MoveFeather(GameObject f)
    {
        float t = 0f;
        var rb = f != null ? f.GetComponent<Rigidbody2D>() : null;
        Vector2 dir = Vector2.down;
        while (f != null && t < fallTime)
        {
            if (rb != null)
            {
                rb.linearVelocity = dir * fallSpeed;
            }
            else
            {
                f.transform.position += new Vector3(dir.x, dir.y, 0f) * fallSpeed * Time.deltaTime;
            }
            RotateToVelocity(f, rb, dir);
            t += Time.deltaTime;
            yield return null;
        }
        float life = 0f;
        while (f != null && life < maxLifetime)
        {
            Vector3 target = GetTargetPoint();
            Vector2 toTarget = ((Vector2)(target - f.transform.position)).normalized;
            dir = Vector2.MoveTowards(dir, toTarget, homingTurnRate * Mathf.Deg2Rad * Time.deltaTime);
            if (rb != null)
            {
                rb.linearVelocity = dir * homingSpeed;
            }
            else
            {
                f.transform.position += new Vector3(dir.x, dir.y, 0f) * homingSpeed * Time.deltaTime;
            }
            RotateToVelocity(f, rb, dir);
            life += Time.deltaTime;
            yield return null;
        }
        if (f != null) Destroy(f);
    }

    private Vector3 GetTargetPoint()
    {
        if (player != null) return player.position;
        if (cam == null) cam = Camera.main;
        if (cam != null) return cam.transform.position;
        return transform.position;
    }

    private void RotateToVelocity(GameObject f, Rigidbody2D rb, Vector2 dir)
    {
        Vector2 v = rb != null ? rb.linearVelocity : dir;
        float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90f;
        f.transform.rotation = Quaternion.Euler(0f, 0f, ang);
    }
}
