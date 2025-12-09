using System;
using System.Collections;
using UnityEngine;

public class TrueKnightBoss : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private MonoBehaviour[] patternBehaviours;
    [SerializeField] private float directHitDelay = 0.3f;
    [SerializeField] private bool randomOrder = false;

    private IAttackPattern[] patterns;
    private int currentIndex = -1;
    private IAttackPattern current;

    private void Awake()
    {
        if (patternBehaviours != null && patternBehaviours.Length > 0)
        {
            patterns = new IAttackPattern[patternBehaviours.Length];
            for (int i = 0; i < patternBehaviours.Length; i++)
                patterns[i] = patternBehaviours[i] as IAttackPattern;
        }
    }

    public void StartCombat()
    {
        StartNextAttack();
    }

    public void StopCombat()
    {
        if (current != null)
        {
            current.OnFinished -= OnPatternFinished;
            current.StopAttack();
            current = null;
        }
    }

    private void OnPatternFinished()
    {
        StartCoroutine(DirectHitThenNext());
    }

    private IEnumerator DirectHitThenNext()
    {
        if (player != null)
            player.OnBossAttackEnd();
        yield return new WaitForSeconds(directHitDelay);
        StartNextAttack();
    }

    private void StartNextAttack()
    {
        if (patterns == null || patterns.Length == 0) return;
        if (current != null)
        {
            current.OnFinished -= OnPatternFinished;
            current.StopAttack();
        }
        if (randomOrder)
            currentIndex = UnityEngine.Random.Range(0, patterns.Length);
        else
            currentIndex = (currentIndex + 1) % patterns.Length;
        current = patterns[currentIndex];
        if (current == null) return;
        current.OnFinished += OnPatternFinished;
        current.StartAttack();
    }

    public void ReceiveMusicEvent(string name)
    {
    }
}
