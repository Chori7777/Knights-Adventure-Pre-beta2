using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    private Animator anim;
    private bossCore core;

    private bool hasMovementParam;
    private bool hasSpeedXParam;
    private bool hasIsAttackingParam;
    private bool hasDamageParam;
    private bool hasDeathParam;
    private bool hasPhaseParam;

    public void Initialize(bossCore bossCore)
    {
        core = bossCore;
        anim = core.anim;
        if (anim == null) return;
        DetectAvailableParameters();
    }

    private void DetectAvailableParameters()
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            switch (param.name)
            {
                case "Movement": hasMovementParam = true; break;
                case "SpeedX": hasSpeedXParam = true; break;
                case "isAttacking": hasIsAttackingParam = true; break;
                case "damage":
                case "Damage": hasDamageParam = true; break;
                case "Death": hasDeathParam = true; break;
                case "Phase": hasPhaseParam = true; break;
            }
        }
    }

    private void LateUpdate()
    {
        if (anim == null || core == null) return;
        UpdateAllAnimations();
    }

    private void UpdateAllAnimations()
    {
        UpdateMovementAnimation();
        UpdateStateAnimations();
    }

    private void UpdateMovementAnimation()
    {
        if (!hasMovementParam || core.rb == null) return;
        float moveAmount = Mathf.Abs(core.rb.linearVelocity.x);
        anim.SetFloat("Movement", moveAmount);
    }

    private void UpdateStateAnimations()
    {
        if (hasIsAttackingParam)
            anim.SetBool("isAttacking", core.IsAttacking);

        if (hasSpeedXParam && core.rb != null)
            anim.SetFloat("SpeedX", Mathf.Abs(core.rb.linearVelocity.x));
    }

    public void SetDamage(bool value)
    {
        if (hasDamageParam)
            anim.SetBool("damage", value);
    }

    public void SetDeath(bool value)
    {
        if (hasDeathParam)
            anim.SetBool("Death", value);
    }

    public void SetPhase(int phase)
    {
        if (hasPhaseParam)
            anim.SetInteger("Phase", phase);
    }
}
