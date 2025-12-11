using UnityEngine;

public class TrueFinalBossAlterTownAttackController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] attackBehaviours;

    public void StartZone()
    {
        if (attackBehaviours == null) return;
        for (int i = 0; i < attackBehaviours.Length; i++)
        {
            var b = attackBehaviours[i];
            if (b == null) continue;
            b.enabled = true;
        }
    }

    public void StopZone()
    {
        if (attackBehaviours == null) return;
        for (int i = 0; i < attackBehaviours.Length; i++)
        {
            var b = attackBehaviours[i];
            if (b == null) continue;
            b.enabled = false;
        }
    }
}

