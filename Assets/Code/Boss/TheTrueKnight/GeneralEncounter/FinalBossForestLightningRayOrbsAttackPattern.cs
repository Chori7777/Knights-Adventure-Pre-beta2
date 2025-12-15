using System;
using System.Collections;
using UnityEngine;

public class FinalBossForestLightningRayOrbsAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private GameObject rayPrefab;
    [SerializeField] private Transform spawnTopRef;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float rayFallSpeed = 12f;
    [SerializeField] private float orbSpawnInterval = 1.0f;
    [SerializeField] private float orbFallSpeed = 10f;
    [SerializeField] private float orbLifetime = 4f;
    [SerializeField] private bool bringRayToFront = true;
    [SerializeField] private string raySortingLayer = "";
    [SerializeField] private int raySortingOrder = 9999;
    [SerializeField] private bool useRayLine = true;
    [SerializeField] private float telegraphDuration = 0.75f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private Camera cam;

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
        routine = StartCoroutine(RayFallSpawnOrbs());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator RayFallSpawnOrbs()
    {
        if (orbPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        GameObject ray = null;
        Vector3 start = GetTopSpawn();
        if (useRayLine && rayPrefab != null)
        {
            ray = Instantiate(rayPrefab, start, Quaternion.identity);
            if (bringRayToFront)
            {
                var sr = ray.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    if (!string.IsNullOrEmpty(raySortingLayer)) sr.sortingLayerName = raySortingLayer;
                    sr.sortingOrder = raySortingOrder;
                }
            }
        }
        // Mostrar línea telegráfica al frente y luego eliminarla para no estorbar
        if (ray != null)
        {
            float tTele = Mathf.Max(0f, telegraphDuration);
            if (tTele > 0f)
                yield return new WaitForSeconds(tTele);
            Destroy(ray);
        }
        // Iniciar la lluvia de orbes sin línea visible
        Vector3 pos = start;
        float nextSpawn = 0f;
        while (true)
        {
            pos += Vector3.down * rayFallSpeed * Time.deltaTime;
            if (Physics2D.Raycast(pos, Vector2.down, groundCheckDistance, groundLayer))
            {
                break;
            }
            nextSpawn -= Time.deltaTime;
            if (nextSpawn <= 0f)
            {
                SpawnFallingOrb(pos);
                nextSpawn = orbSpawnInterval;
            }
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private Vector3 GetTopSpawn()
    {
        if (spawnTopRef != null) return spawnTopRef.position;
        if (cam == null) cam = Camera.main;
        if (cam != null) return new Vector3(cam.transform.position.x, cam.transform.position.y + 6f, 0f);
        return transform.position + Vector3.up * 6f;
    }

    private void SpawnFallingOrb(Vector3 origin)
    {
        if (orbPrefab == null) return;
        GameObject o = Instantiate(orbPrefab, origin, Quaternion.identity);
        AddTrail(o);
        var rb = o.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.down * orbFallSpeed;
        StartCoroutine(OrbFallLifetime(o));
    }

    private IEnumerator OrbFallLifetime(GameObject o)
    {
        float t = 0f;
        while (o != null && t < orbLifetime)
        {
            t += Time.deltaTime;
            var p = o.transform.position;
            if (Physics2D.Raycast(p, Vector2.down, groundCheckDistance, groundLayer)) break;
            o.transform.position += Vector3.down * orbFallSpeed * Time.deltaTime;
            yield return null;
        }
        if (o != null) Destroy(o);
    }

    private void AddTrail(GameObject go)
    {
        var tr = go.GetComponent<TrailRenderer>();
        if (tr == null) tr = go.AddComponent<TrailRenderer>();
        tr.time = 0.25f;
        tr.minVertexDistance = 0.08f;
        tr.autodestruct = false;
        tr.startWidth = 0.08f;
        tr.endWidth = 0.056f;
        tr.material = new Material(Shader.Find("Sprites/Default"));
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 1f, 1f, 0.8f), 0f), new GradientColorKey(new Color(1f, 1f, 1f, 0f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        tr.colorGradient = g;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            tr.sortingLayerID = sr.sortingLayerID;
            tr.sortingOrder = sr.sortingOrder - 1;
        }
    }
}
