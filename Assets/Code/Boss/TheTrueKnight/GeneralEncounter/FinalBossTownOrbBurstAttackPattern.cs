using System;
using System.Collections;
using UnityEngine;

public class FinalBossTownOrbBurstAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private Transform player;
    [SerializeField] private int bursts = 4;
    [SerializeField] private float burstInterval = 0.6f;
    [SerializeField] private int orbsPerBurst = 5;
    [SerializeField] private float orbSpeed = 10f;
    [SerializeField] private float spreadAngle = 20f;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine routine;

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
        routine = StartCoroutine(BurstRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator BurstRoutine()
    {
        if (orbPrefab == null || shootOrigin == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        for (int b = 0; b < bursts; b++)
        {
            ShootBurst();
            yield return new WaitForSeconds(burstInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }

    private void ShootBurst()
    {
        Vector3 origin = shootOrigin.position;
        Vector2 baseDir = player != null ? (player.position - origin).normalized : Vector2.down;
        float half = spreadAngle * 0.5f;
        for (int i = 0; i < orbsPerBurst; i++)
        {
            float t = (orbsPerBurst == 1) ? 0f : (float)i / (orbsPerBurst - 1);
            float ang = Mathf.Lerp(-half, half, t);
            Vector2 dir = Rotate(baseDir, ang);
            GameObject o = Instantiate(orbPrefab, origin, Quaternion.identity);
            AddTrail(o);
            var rb = o.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * orbSpeed;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad);
        float sa = Mathf.Sin(rad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
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
