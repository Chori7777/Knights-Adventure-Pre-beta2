using System;
using System.Collections;
using UnityEngine;

public class HitGroundHoldReturnAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform pointA;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxDropDistance = 20f;
    [SerializeField] private float dropSpeed = 8f;
    [SerializeField] private float holdTime = 1.0f;
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private bool autoStartOnEnable = true;
    [SerializeField] private bool destroyOnEnd = true;

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
        routine = StartCoroutine(AttackRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        Vector3 a = pointA != null ? pointA.position : transform.position;
        if (prefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }

        GameObject obj = Instantiate(prefab, a, Quaternion.identity);

        Vector3 b = a - Vector3.up * maxDropDistance;
        var hit = Physics2D.Raycast(a, Vector2.down, maxDropDistance, groundLayer);
        if (hit.collider != null)
        {
            b = new Vector3(a.x, hit.point.y, a.z);
        }

        while (Vector3.Distance(obj.transform.position, b) > 0.01f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, b, dropSpeed * Time.deltaTime);
            yield return null;
        }

        float t = 0f;
        while (t < holdTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        while (Vector3.Distance(obj.transform.position, a) > 0.01f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, a, returnSpeed * Time.deltaTime);
            yield return null;
        }

        if (destroyOnEnd && obj != null) Destroy(obj);
        routine = null;
        OnFinished?.Invoke();
    }
}
