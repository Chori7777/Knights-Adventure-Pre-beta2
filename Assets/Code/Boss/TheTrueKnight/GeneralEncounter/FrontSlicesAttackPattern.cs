using System;
using System.Collections;
using UnityEngine;

public class FrontSlicesAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private Transform spawn;
    [SerializeField] private int sliceCount = 5;
    [SerializeField] private float sliceInterval = 0.08f;
    [SerializeField] private float sliceSpeed = 12f;
    [SerializeField] private Transform player;
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
        if (slashPrefab == null)
        {
            OnFinished?.Invoke();
            yield break;
        }
        Transform s = spawn != null ? spawn : transform;
        Vector2 dir = Vector2.right;
        if (player != null) dir = ((Vector2)player.position - (Vector2)s.position).normalized;
        for (int i = 0; i < sliceCount; i++)
        {
            GameObject p = Instantiate(slashPrefab, s.position, Quaternion.identity);
            var rb = p.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * sliceSpeed;
            else p.transform.right = new Vector3(dir.x, dir.y, 0f);
            yield return new WaitForSeconds(sliceInterval);
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
