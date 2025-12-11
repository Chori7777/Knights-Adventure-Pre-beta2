using UnityEngine;

public class TrueFinalBossSnowBridge : MonoBehaviour
{
    [System.Serializable]
    public class SnowPatternEntry
    {
        public int number;
        public MonoBehaviour behaviour;
        public GameObject root;
    }

    [SerializeField] private SnowPatternEntry[] patterns;

    public void ActivateByNumber(int number)
    {
        var entry = Get(number);
        if (entry == null) return;
        if (entry.root != null) entry.root.SetActive(true);
        var p = entry.behaviour as IAttackPattern;
        if (p != null) p.StartAttack();
    }

    public void StopAll()
    {
        if (patterns == null) return;
        for (int i = 0; i < patterns.Length; i++)
        {
            var p = patterns[i];
            if (p == null) continue;
            var b = p.behaviour as IAttackPattern;
            if (b != null) b.StopAttack();
            if (p.root != null) p.root.SetActive(false);
        }
    }

    private SnowPatternEntry Get(int number)
    {
        if (patterns == null) return null;
        for (int i = 0; i < patterns.Length; i++)
        {
            var e = patterns[i];
            if (e != null && e.number == number) return e;
        }
        return null;
    }
}

