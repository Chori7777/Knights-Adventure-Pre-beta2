using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossSnowSideOrbsRetargetAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform areaLeft;
    [SerializeField] private Transform areaRight;
    [SerializeField] private Transform areaBottom;
    [SerializeField] private Transform areaTop;
    [SerializeField] private int orbsPerSide = 8;
    [SerializeField] private float horizontalSpeed = 9f;
    [SerializeField] private float travelDuration = 2.2f;
    [SerializeField] private float homingSpeed = 15f;
    [SerializeField] private float retargetStaggerInterval = 0.08f;
    [SerializeField] private float postRetargetLifetime = 2.5f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;
    private readonly List<OrbMover> movers = new List<OrbMover>();

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
        SpawnSide(true);
        SpawnSide(false);
        float t = 0f;
        while (t < travelDuration)
        {
            for (int i = 0; i < movers.Count; i++)
            {
                var m = movers[i];
                if (m == null) continue;
                m.UpdateHorizontal(horizontalSpeed);
            }
            t += Time.deltaTime;
            yield return null;
        }
        Vector3 target = player != null ? player.position : (areaRight.position + areaLeft.position) * 0.5f;
        for (int i = 0; i < movers.Count; i++)
        {
            var m = movers[i];
            if (m == null) continue;
            m.SetColor(new Color(1f, 0.5f, 0f, 1f));
            m.BeginHomingSnapshot(target, homingSpeed, postRetargetLifetime);
            if (retargetStaggerInterval > 0f) yield return new WaitForSeconds(retargetStaggerInterval);
        }
        bool anyAlive = true;
        while (anyAlive)
        {
            anyAlive = false;
            for (int i = 0; i < movers.Count; i++)
            {
                var m = movers[i];
                if (m == null) continue;
                m.UpdateHoming();
                if (m.IsAlive) anyAlive = true;
            }
            yield return null;
        }
        Cleanup();
        routine = null;
        OnFinished?.Invoke();
    }

    private void SpawnSide(bool left)
    {
        if (orbPrefab == null) return;
        for (int i = 0; i < orbsPerSide; i++)
        {
            float y = Mathf.Lerp(areaBottom.position.y, areaTop.position.y, (i + 0.5f) / orbsPerSide);
            float x = left ? areaLeft.position.x : areaRight.position.x;
            var go = Instantiate(orbPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            AddTrail(go);
            var m = new OrbMover(go, left ? Vector2.right : Vector2.left);
            movers.Add(m);
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

public class OrbMover
{
    private readonly GameObject go;
    private readonly Vector2 dir;
    private Vector2 homingDir;
    private float speed;
    private bool homing;
    private SpriteRenderer sr;
    private float lifetime;
    public bool IsAlive => go != null;

    public OrbMover(GameObject g, Vector2 d)
    {
        go = g;
        dir = d.normalized;
        sr = go.GetComponent<SpriteRenderer>();
    }

    public void UpdateHorizontal(float s)
    {
        if (go == null) return;
        go.transform.position += (Vector3)(dir * s * Time.deltaTime);
    }

    public void BeginHomingSnapshot(Vector3 targetPos, float s, float lt)
    {
        speed = s;
        homingDir = ((Vector2)(targetPos - go.transform.position)).normalized;
        lifetime = lt;
        homing = true;
    }

    public void UpdateHoming()
    {
        if (!homing || go == null) return;
        go.transform.position += (Vector3)(homingDir * speed * Time.deltaTime);
        if (lifetime > 0f)
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy();
        }
    }

    public void SetColor(Color c)
    {
        if (sr != null) sr.color = c;
    }

    public void Destroy()
    {
        if (go != null) UnityEngine.Object.Destroy(go);
    }
}
