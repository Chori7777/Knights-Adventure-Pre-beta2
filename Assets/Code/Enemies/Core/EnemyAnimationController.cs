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
    private bool hasDamageBoolParam;
    private bool hasDamageTriggerParam;
    private bool hasDeathBoolParam;
    private bool hasDeathTriggerParam;
    private bool hasAttackTrigger;
    private bool hasIsAttackingParam;

    // Nombres reales de parámetros detectados
    private string movementParamName;
    private string speedXParamName;
    private string speedYParamName;
    private string groundedParamName;
    private string damageBoolParamName;
    private string damageTriggerParamName;
    private string deathBoolParamName;
    private string deathTriggerParamName;

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
                    movementParamName = "Movement";
                    break;
                case "Speed":
                    hasMovementParam = true;
                    movementParamName = "Speed";
                    break;
                case "SpeedX":
                    hasSpeedXParam = true;
                    speedXParamName = "SpeedX";
                    break;
                case "SpeedY":
                    hasSpeedYParam = true;
                    speedYParamName = "SpeedY";
                    break;
                case "Grounded":
                case "isGrounded":
                    hasGroundedParam = true;
                    groundedParamName = param.name;
                    break;
                case "damage":
                case "Damage":
                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        hasDamageBoolParam = true;
                        damageBoolParamName = param.name;
                    }
                    else if (param.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasDamageTriggerParam = true;
                        damageTriggerParamName = param.name;
                    }
                    break;
                case "Death":
                case "isDead":
                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        hasDeathBoolParam = true;
                        deathBoolParamName = param.name;
                    }
                    else if (param.type == AnimatorControllerParameterType.Trigger)
                    {
                        hasDeathTriggerParam = true;
                        deathTriggerParamName = param.name;
                    }
                    break;
                case "Attack":
                    hasAttackTrigger = true;
                    break;
                case "isAttacking":
                    hasIsAttackingParam = true;
                    break;
            }
        }

        // ✅ DEBUG: Mostrar qué parámetros de Death se encontraron
        if (hasDeathBoolParam)
            Debug.Log($"[EnemyAnimationController] Death detectado como BOOL: {deathBoolParamName}");
        if (hasDeathTriggerParam)
            Debug.Log($"[EnemyAnimationController] Death detectado como TRIGGER: {deathTriggerParamName}");
    }

    private void LateUpdate()
    {
        if (anim == null || core == null) return;

        // ✅ NO actualizar animaciones si está muerto
        if (core.IsDead) return;

        UpdateAllAnimations();
    }

    private void UpdateAllAnimations()
    {
        UpdateMovementAnimation();
        UpdateVelocityAnimation();
        UpdateStateAnimations();
    }

    //Animaciones de movimiento
    private void UpdateMovementAnimation()
    {
        if (!hasMovementParam || core.rb == null) return;

        float moveAmount = Mathf.Abs(core.rb.linearVelocity.x);
        anim.SetFloat(movementParamName, moveAmount);
    }

    private void UpdateVelocityAnimation()
    {
        if (core.rb == null) return;

        if (hasSpeedXParam)
        {
            anim.SetFloat(speedXParamName, Mathf.Abs(core.rb.linearVelocity.x));
        }

        if (hasSpeedYParam)
        {
            anim.SetFloat(speedYParamName, core.rb.linearVelocity.y);
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
        if (hasDamageBoolParam)
        {
            anim.SetBool(damageBoolParamName, value);
        }
        if (hasDamageTriggerParam && value)
        {
            anim.SetTrigger(damageTriggerParamName);
        }
    }

    // ✅ ARREGLADO: Método mejorado para Death
    public void SetDeath(bool value)
    {
        if (!value) return; // Solo procesar cuando value = true

        Debug.Log("[EnemyAnimationController] SetDeath llamado");

        // ✅ Prioridad 1: Usar Trigger si existe
        if (hasDeathTriggerParam)
        {
            anim.SetTrigger(deathTriggerParamName);
            Debug.Log($"[EnemyAnimationController] SetTrigger('{deathTriggerParamName}')");
        }
        // ✅ Prioridad 2: Usar Bool si no hay Trigger
        else if (hasDeathBoolParam)
        {
            // Primero resetear cualquier otro parámetro que pueda interferir
            if (hasDamageBoolParam)
            {
                anim.SetBool(damageBoolParamName, false);
            }

            // Luego activar Death
            anim.SetBool(deathBoolParamName, true);
            Debug.Log($"[EnemyAnimationController] SetBool('{deathBoolParamName}', true)");

            // ✅ CRÍTICO: Forzar que el Animator actualice inmediatamente
            anim.Update(0f);
        }
        else
        {
            Debug.LogWarning("[EnemyAnimationController] No hay parámetro Death (ni Bool ni Trigger)");
        }

        // ✅ DEBUG: Verificar estado del Animator
        if (anim != null)
        {
            Debug.Log($"[EnemyAnimationController] Animator enabled: {anim.enabled}");
            Debug.Log($"[EnemyAnimationController] Animator speed: {anim.speed}");

            // Ver estado actual
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[EnemyAnimationController] Estado actual: {stateInfo.shortNameHash}");
        }
    }

    public void SetGrounded(bool value)
    {
        if (hasGroundedParam)
        {
            anim.SetBool(groundedParamName, value);
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
