using System;
using System.Collections;
using UnityEngine;

public class FourDirectionsConvergeAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private float swordSpeed = 10f;
    [SerializeField] private float swordLifetime = 4f;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private bool usePlayerAsTarget = false;
    [SerializeField] private Transform targetOverride;
    [SerializeField] private bool autoStartOnEnable = true;
    [SerializeField] private float finishDelay = 0.5f;
    [SerializeField] private bool showWarning = true;
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 1.0f;
    [SerializeField] private float warningBlinkInterval = 0.15f;
    [SerializeField] private AudioClip warningSFX;
    [SerializeField] private bool playWarningSFX = true;

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
        routine = StartCoroutine(ConvergeRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator ConvergeRoutine()
    {
        if (swordPrefab == null) { OnFinished?.Invoke(); yield break; }
        Vector3 target = GetTargetPoint();
        Vector3 left, right, top, bottom;
        GetEdgePoints(out left, out right, out top, out bottom);

        if (showWarning)
        {
            GameObject wL = SpawnWarning(left);
            GameObject wR = SpawnWarning(right);
            GameObject wT = SpawnWarning(top);
            GameObject wB = SpawnWarning(bottom);
            if (playWarningSFX && warningSFX != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(warningSFX);
            yield return StartCoroutine(BlinkWarnings(wL, wR, wT, wB));
            if (wL != null) Destroy(wL);
            if (wR != null) Destroy(wR);
            if (wT != null) Destroy(wT);
            if (wB != null) Destroy(wB);
        }

        SpawnMovingSword(left, target);
        SpawnMovingSword(right, target);
        SpawnMovingSword(top, target);
        SpawnMovingSword(bottom, target);

        yield return new WaitForSeconds(finishDelay);
        routine = null;
        OnFinished?.Invoke();
    }

    private GameObject SpawnWarning(Vector3 pos)
    {
        if (warningPrefab == null) return null;
        return Instantiate(warningPrefab, pos, Quaternion.identity);
    }

    private IEnumerator BlinkWarnings(params GameObject[] warns)
    {
        float elapsed = 0f;
        bool state = true;
        for (int i = 0; i < warns.Length; i++)
        {
            if (warns[i] != null) warns[i].SetActive(true);
        }
        while (elapsed < warningDuration)
        {
            elapsed += warningBlinkInterval;
            state = !state;
            for (int i = 0; i < warns.Length; i++)
            {
                if (warns[i] != null) warns[i].SetActive(state);
            }
            yield return new WaitForSeconds(warningBlinkInterval);
        }
        for (int i = 0; i < warns.Length; i++)
        {
            if (warns[i] != null) warns[i].SetActive(true);
        }
    }

    private void SpawnMovingSword(Vector3 pos, Vector3 target)
    {
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

    private void GetEdgePoints(out Vector3 left, out Vector3 right, out Vector3 top, out Vector3 bottom)
    {
        if (cam == null) cam = Camera.main;
        Vector3 c = cam != null ? cam.transform.position : transform.position;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float halfW = cam != null ? halfH * cam.aspect : 8f;
        left = new Vector3(c.x - halfW - spawnOffset, c.y, 0f);
        right = new Vector3(c.x + halfW + spawnOffset, c.y, 0f);
        top = new Vector3(c.x, c.y + halfH + spawnOffset, 0f);
        bottom = new Vector3(c.x, c.y - halfH - spawnOffset, 0f);
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
