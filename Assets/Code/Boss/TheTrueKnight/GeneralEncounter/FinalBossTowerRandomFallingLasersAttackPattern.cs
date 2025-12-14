using System;
using System.Collections;
using UnityEngine;

public class FinalBossTowerRandomFallingLasersAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform spawnTopRef;
    [SerializeField] private float spawnWidth = 12f;
    [SerializeField] private int laserCount = 8;
    [SerializeField] private float spawnInterval = 0.3f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float warningDelay = 0.6f;
    [SerializeField] private GameObject warningLinePrefab;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayMaxDistance = 30f;
    [SerializeField] private float warningLineWidth = 0.08f;
    [SerializeField] private Color warningLineColor = Color.red;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private System.Collections.Generic.List<GameObject> spawned = new System.Collections.Generic.List<GameObject>();

    private void OnEnable()
    {
        if (autoStartOnEnable) StartAttack();
    }

    private void OnDisable()
    {
        StopAttack();
        CleanupSpawned();
    }

    public void StartAttack()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LaserRain());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        CleanupSpawned();
    }

    private IEnumerator LaserRain()
    {
        if (laserPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Vector3 top = spawnTopRef != null ? spawnTopRef.position : transform.position + Vector3.up * 6f;
        for (int i = 0; i < laserCount; i++)
        {
            Vector3 pos = new Vector3(
                top.x + UnityEngine.Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f),
                top.y,
                0f
            );
            GameObject warn = CreateWarningLine(pos);
            if (warningDelay > 0f) yield return new WaitForSeconds(warningDelay);
            if (warn != null) Destroy(warn);
            GameObject l = Instantiate(laserPrefab, pos, Quaternion.identity);
            spawned.Add(l);
            Destroy(l, lifetime);
            yield return new WaitForSeconds(spawnInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private void CleanupSpawned()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            var go = spawned[i];
            if (go != null)
            {
                Destroy(go);
            }
        }
        spawned.Clear();
    }

    private GameObject CreateWarningLine(Vector3 start)
    {
        Vector3 end = start + Vector3.down * rayMaxDistance;
        var hit = Physics2D.Raycast(start, Vector2.down, rayMaxDistance, groundLayer);
        if (hit.collider != null)
        {
            end = hit.point;
        }

        GameObject lineObj = null;
        LineRenderer lr = null;
        if (warningLinePrefab != null)
        {
            lineObj = Instantiate(warningLinePrefab, Vector3.zero, Quaternion.identity);
            lr = lineObj.GetComponent<LineRenderer>();
        }
        else
        {
            lineObj = new GameObject("LaserWarningLine");
            lr = lineObj.AddComponent<LineRenderer>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            lr.material = mat;
        }
        if (lr != null)
        {
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startWidth = warningLineWidth;
            lr.endWidth = warningLineWidth;
            lr.startColor = warningLineColor;
            lr.endColor = warningLineColor;
        }
        return lineObj;
    }
}
