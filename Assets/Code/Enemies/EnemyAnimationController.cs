using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator anim;
    private EnemyCore core;

    // Parámetros disponibles (se detectan automáticamente)
    private bool hasMovementParam;
    private bool hasSpeedXParam;
    private bool hasSpeedYParam;
    private bool hasGroundedParam;
    private bool hasDamageParam;
    private bool hasDeathParam;
    private bool hasAttackTrigger;
    private bool hasIsAttackingParam;

    public void Initialize(EnemyCore enemyCore)
    {
        core = enemyCore;
        anim = core.anim;

        if (anim == null) return;

        // Detectar qué parámetros existen
        DetectAvailableParameters();
    }
    //Detecta los parametros que tiene el animator para no tirar errores
    private void DetectAvailableParameters()
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            switch (param.name)
            {
                case "Movement":
                    hasMovementParam = true;
                    break;
                case "SpeedX":
                    hasSpeedXParam = true;
                    break;
                case "SpeedY":
                    hasSpeedYParam = true;
                    break;
                case "Grounded":
                case "isGrounded":
                    hasGroundedParam = true;
                    break;
                case "damage":
                case "Damage":
                    hasDamageParam = true;
                    break;
                case "Death":
                case "isDead":
                    hasDeathParam = true;
                    break;
                case "Attack":
                    hasAttackTrigger = true;
                    break;
                case "isAttacking":
                    hasIsAttackingParam = true;
                    break;
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
        UpdateVelocityAnimation();
        UpdateStateAnimations();
    }

    //Animacinoes de movimiento
    private void UpdateMovementAnimation()
    {
        if (!hasMovementParam || core.rb == null) return;

        float moveAmount = Mathf.Abs(core.rb.linearVelocity.x);
        anim.SetFloat("Movement", moveAmount);
    }

    private void UpdateVelocityAnimation()
    {
        if (core.rb == null) return;

        if (hasSpeedXParam)
        {
            anim.SetFloat("SpeedX", Mathf.Abs(core.rb.linearVelocity.x));
        }

        if (hasSpeedYParam)
        {
            anim.SetFloat("SpeedY", core.rb.linearVelocity.y);
        }
    }

    private void UpdateStateAnimations()
    {
        if (hasIsAttackingParam)
        {
            anim.SetBool("isAttacking", core.IsAttacking);
        }
    }

    //triggers y booleanos para las animaciones
    public void TriggerAttack()
    {
        if (hasAttackTrigger)
        {
            anim.SetTrigger("Attack");
        }
    }
    public void ResetAttack()
    {
        if (hasAttackTrigger)
        {
            anim.ResetTrigger("Attack");
        }

        if (hasIsAttackingParam)
        {
            anim.SetBool("isAttacking", false);
        }
    }

    public void SetDamage(bool value)
    {
        if (hasDamageParam)
        {
            anim.SetBool("damage", value);
        }
    }

    public void SetDeath(bool value)
    {
        if (hasDeathParam)
        {
            anim.SetBool("Death", value);
        }
    }

    public void SetGrounded(bool value)
    {
        if (hasGroundedParam)
        {
            anim.SetBool("Grounded", value);
        }
    }

    //Para el momento en que el ataque conecta
    public void OnAttackHitFrame()
    {
  
        if (core.meleeAttack != null)
        {
            core.meleeAttack.DealDamage();
        }
    }

    public void OnAttackEnd()
    {
        core.SetAttacking(false);
    }

    public void OnDamageEnd()
    {
        SetDamage(false);
        core.SetTakingDamage(false);
    }
}