using UnityEngine;
using System.Collections;

public class TheTrueKnightBossAI : MonoBehaviour
{
    [SerializeField] private TheTrueKnightAttackManager attackManager;
    [SerializeField] private bool logEvents = true;

    [Header("Auto IA")]
    [SerializeField] private bool enableAutoAI = false;
    [SerializeField] private float timeBetweenAttacks = 2.0f;
    [SerializeField] private bool aiBurstCenter = true;
    [SerializeField] private bool aiBouncingKnives = true;
    [SerializeField] private bool aiFrontSlices = true;
    [SerializeField] private bool aiGroundSlamRocks = true;

    [System.Serializable]
    public class AttackToggle
    {
        public string name;
        public bool enabled;
    }
    [SerializeField] private AttackToggle[] aiAttackToggles;

    private Coroutine aiRoutine;

    [System.Serializable]
    public class Alias
    {
        public string eventName;
        public string attackName;
    }

    [SerializeField] private Alias[] aliases;

    public void ReceiveMusicEvent(string name)
    {
        if (logEvents) Debug.Log("[BossAI] MusicEvent: " + name);
        string mapped = ResolveAlias(name);
        if (attackManager != null)
        {
            attackManager.Play(mapped);
        }
        else
        {
            Debug.LogWarning("[BossAI] No attackManager asignado");
        }
    }

    private void OnEnable()
    {
        if (enableAutoAI) StartAutoAI();
    }

    private void OnDisable()
    {
        StopAutoAI();
    }

    public void StartAutoAI()
    {
        if (aiRoutine != null) StopCoroutine(aiRoutine);
        aiRoutine = StartCoroutine(AILoop());
    }

    public void StopAutoAI()
    {
        if (aiRoutine != null)
        {
            StopCoroutine(aiRoutine);
            aiRoutine = null;
        }
    }

    private IEnumerator AILoop()
    {
        yield return new WaitForSeconds(0.5f);
        while (enableAutoAI)
        {
            string next = PickNextAttack();
            if (!string.IsNullOrEmpty(next) && attackManager != null)
            {
                attackManager.Play(next);
            }
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private string PickNextAttack()
    {
        if (aiAttackToggles != null && aiAttackToggles.Length > 0)
        {
            int enabledCount = 0;
            for (int i = 0; i < aiAttackToggles.Length; i++)
            {
                var t = aiAttackToggles[i];
                if (t != null && t.enabled && !string.IsNullOrEmpty(t.name)) enabledCount++;
            }
            if (enabledCount > 0)
            {
                int togglePick = Random.Range(0, enabledCount);
                for (int i = 0; i < aiAttackToggles.Length; i++)
                {
                    var t = aiAttackToggles[i];
                    if (t != null && t.enabled && !string.IsNullOrEmpty(t.name))
                    {
                        if (togglePick == 0) return t.name;
                        togglePick--;
                    }
                }
            }
        }
        int count = 0;
        if (aiBurstCenter) count++;
        if (aiBouncingKnives) count++;
        if (aiFrontSlices) count++;
        if (aiGroundSlamRocks) count++;
        if (count == 0) return null;
        int pick = Random.Range(0, count);
        if (aiBurstCenter)
        {
            if (pick == 0) return "burst_center"; else pick--;
        }
        if (aiBouncingKnives)
        {
            if (pick == 0) return "bounce_knives"; else pick--;
        }
        if (aiFrontSlices)
        {
            if (pick == 0) return "front_slices"; else pick--;
        }
        if (aiGroundSlamRocks)
        {
            if (pick == 0) return "ground_slam_rocks"; else pick--;
        }
        return "attack_0";
    }

    private string ResolveAlias(string name)
    {
        if (aliases != null)
        {
            for (int i = 0; i < aliases.Length; i++)
            {
                var a = aliases[i];
                if (a != null && a.eventName == name && !string.IsNullOrEmpty(a.attackName))
                    return a.attackName;
            }
        }
        if (name == "Ataque lento") return "attack_0";
        if (name == "AtaqueEspecial") return "spawn_proj";
        if (name == "TilemapA") return "tilemap_A";
        if (name == "ActivarObjetos") return "activate_objs";
        if (name == "BurstCenter") return "burst_center";
        if (name == "BouncingKnives") return "bounce_knives";
        if (name == "FrontSlices") return "front_slices";
        if (name == "GroundSlamRocks") return "ground_slam_rocks";
        return name;
    }
}
