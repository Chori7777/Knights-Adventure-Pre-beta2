using System;
using System.Collections;
using UnityEngine;

public class BouncingKnivesAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private int knifeCount = 6;
    [SerializeField] private float knifeSpeed = 10f;
    [SerializeField] private int maxBounces = 4;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private Transform spawn;
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
        routine = StartCoroutine(SpawnRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        if (knifePrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Camera cam = Camera.main;
        Vector3 c = cam != null ? cam.transform.position : transform.position;
        float halfH = cam != null ? cam.orthographicSize : 5f;
        float halfW = cam != null ? halfH * cam.aspect : 8f;
        Transform s = spawn != null ? spawn : transform;
        for (int i = 0; i < knifeCount; i++)
        {
            GameObject p = Instantiate(knifePrefab, s.position, Quaternion.identity);
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * knifeSpeed;
            var bounce = p.AddComponent<BouncingKnife>();
            bounce.Init(knifeSpeed, maxBounces, lifetime, c, halfW, halfH);
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
