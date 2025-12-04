using UnityEngine;

public class TheTrueKnightBossAI : MonoBehaviour
{
    [SerializeField] private TheTrueKnightAttackManager attackManager;

    public void ReceiveMusicEvent(string name)
    {
        if (attackManager == null) return;
        attackManager.Play(name);
    }
}

