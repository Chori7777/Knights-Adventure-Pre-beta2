using UnityEngine;
using System.Collections;

public abstract class BossAttackPattern : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] protected float timeBetweenAttacks = 3f;
    [SerializeField] protected float minDistanceForMelee = 2f;

    [Header("Alerta Visual")]
    [SerializeField] protected GameObject alertaPrefab;
    [SerializeField] protected float alertDuration = 0.8f;

    protected bossCore core;
    protected GameObject currentAlert;
    protected bool isRunningPattern = false;

    public virtual void Initialize(bossCore bossCore)
    {
        core = bossCore;

        if (alertaPrefab != null)
        {
            currentAlert = Instantiate(alertaPrefab, transform);
            currentAlert.SetActive(false);
        }
    }

    protected virtual void Start()
    {
        if (core != null)
        {
            StartCoroutine(AttackPatternLoop());
        }
    }

    protected virtual IEnumerator AttackPatternLoop()
    {
        yield return new WaitForSeconds(1f);
        isRunningPattern = true;

        while (!core.IsDead)
        {
            if (!core.IsAttacking && core.player != null)
            {
                yield return StartCoroutine(SelectAndExecuteAttack());
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
        }

        isRunningPattern = false;
    }

    protected abstract IEnumerator SelectAndExecuteAttack();

    protected IEnumerator ShowAlert(float duration = -1)
    {
        if (duration < 0) duration = alertDuration;

        if (currentAlert != null)
        {
            currentAlert.SetActive(true);
            Animator alertAnim = currentAlert.GetComponent<Animator>();
            if (alertAnim != null)
                alertAnim.SetTrigger("Alert");
        }

        yield return new WaitForSeconds(duration);

        if (currentAlert != null)
            currentAlert.SetActive(false);
    }

    protected bool IsPlayerInMeleeRange()
    {
        return core.DistanceToPlayer() <= minDistanceForMelee;
    }

    protected void StopMovement()
    {
        if (core.rb != null)
            core.rb.linearVelocity = Vector2.zero;
    }

    public void StopPattern()
    {
        StopAllCoroutines();
        isRunningPattern = false;
    }

    public void ResumePattern()
    {
        if (!isRunningPattern)
            StartCoroutine(AttackPatternLoop());
    }
}
