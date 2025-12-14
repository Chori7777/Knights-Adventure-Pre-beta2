using System;
using System.Collections;
using UnityEngine;

public class FinalBossSnowSliceLineAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private Transform line;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float spinSpeed = 540f;
    [SerializeField] private float spinDuration = 1.2f;
    [SerializeField] private float slowSpinSpeed = 120f;
    [SerializeField] private float slowDuration = 0.8f;
    [SerializeField] private float dashSpeed = 18f;
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
        routine = StartCoroutine(SliceRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator SliceRoutine()
    {
        if (line == null || startPoint == null || endPoint == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        line.position = (startPoint.position + endPoint.position) * 0.5f;
        float t = 0f;
        while (t < spinDuration)
        {
            line.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
        while (t < slowDuration)
        {
            line.Rotate(0f, 0f, -slowSpinSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
        line.position = startPoint.position;
        while (Vector3.Distance(line.position, endPoint.position) > 0.05f)
        {
            line.position = Vector3.MoveTowards(line.position, endPoint.position, dashSpeed * Time.deltaTime);
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
