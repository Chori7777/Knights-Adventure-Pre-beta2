using UnityEngine;

public class StarSpawnOnDialogueEnd : MonoBehaviour
{
    [SerializeField] private BossLife boss;
    [SerializeField] private float extraDelay = 0f;

    private void Awake()
    {
        if (boss == null)
        {
            boss = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
        }
        if (boss != null)
        {
            boss.OnDeathDialoguesComplete += OnDeathDialoguesComplete;
        }
    }

    private void OnDestroy()
    {
        if (boss != null)
        {
            boss.OnDeathDialoguesComplete -= OnDeathDialoguesComplete;
        }
    }

    private void OnDeathDialoguesComplete()
    {
        if (boss == null || !boss.IsDead) return;
        if (extraDelay > 0f)
        {
            StartCoroutine(SpawnWithDelay(extraDelay));
        }
        else
        {
            boss.TriggerStarSpawnNow();
        }
    }

    private System.Collections.IEnumerator SpawnWithDelay(float delay)
    {
        float t = 0f;
        while (t < delay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (boss != null) boss.TriggerStarSpawnNow();
    }
}

