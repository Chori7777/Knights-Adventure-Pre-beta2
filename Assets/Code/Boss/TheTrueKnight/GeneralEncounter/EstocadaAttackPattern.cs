using System;
using System.Collections;
using UnityEngine;

public class EstocadaAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private Animator bossAnimator;
    [SerializeField] private string triggerName = "Attack";
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float lungeDistance = 1.0f;
    [SerializeField] private float lungeSpeed = 8f;
    [SerializeField] private Transform forwardRef;
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
        routine = StartCoroutine(PerformEstocada());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator PerformEstocada()
    {
        if (bossAnimator != null && !string.IsNullOrEmpty(triggerName))
            bossAnimator.SetTrigger(triggerName);

        float elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 dir = Vector3.right;
        if (forwardRef != null)
            dir = forwardRef.right;
        Vector3 target = start + dir * lungeDistance;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, lungeSpeed * Time.deltaTime);
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
