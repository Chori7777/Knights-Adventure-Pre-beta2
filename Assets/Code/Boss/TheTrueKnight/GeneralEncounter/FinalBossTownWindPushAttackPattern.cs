using System;
using System.Collections;
using UnityEngine;

public class FinalBossTownWindPushAttackPattern : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private float pushForce = 25f;
    [SerializeField] private float duration = 2.5f;
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
        routine = StartCoroutine(PushRoutine());
    }

    public void StopAttack()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator PushRoutine()
    {
        float t = 0f;
        while (t < duration)
        {
            if (playerRb != null)
            {
                playerRb.AddForce(Vector2.left * pushForce * Time.deltaTime, ForceMode2D.Force);
            }
            t += Time.deltaTime;
            yield return null;
        }
        routine = null;
        OnFinished?.Invoke();
    }
}
