using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    private Animator anim;
    private bossCore core;

    // Parámetros disponibles
    private bool hasMovementParam;
    private bool hasSpeedXParam;

    // Damage puede ser Bool o Trigger
    private bool hasDamageBoolParam;
    private bool hasDamageTriggerParam;
    private string damageBoolParamName;
    private string damageTriggerParamName;

    // Death puede ser Bool o Trigger
    private bool hasDeathBoolParam;
    private bool hasDeathTriggerParam;
    private string deathBoolParamName;
    private string deathTriggerParamName;

    private bool hasPhaseParam;

    public void Initialize(bossCore bossCore)
    {
        core = bossCore;
        anim = core.anim;

        if (anim == null)
        {
            Debug.LogWarning($"[{gameObject.name}] No se encontró Animator");
            return;
        }

        DetectAvailableParameters();
    }

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

                case "Phase":
                    hasPhaseParam = true;
                    break;
            }
        }

        // ✅ Debug solo si hay parámetros faltantes importantes
        LogMissingParameters();
    }

    private void LogMissingParameters()
    {
        // Solo log informativo, no warnings molestos
        if (!hasDeathBoolParam && !hasDeathTriggerParam)
        {
            Debug.Log($"ℹ️ [{gameObject.name}] Sin parámetro 'Death' (opcional)");
        }

        if (!hasDamageBoolParam && !hasDamageTriggerParam)
        {
            Debug.Log($"ℹ️ [{gameObject.name}] Sin parámetro 'Damage' (opcional)");
        }

        if (!hasPhaseParam)
        {
            Debug.Log($"ℹ️ [{gameObject.name}] Sin parámetro 'Phase' (opcional)");
        }
    }

    private void LateUpdate()
    {
        if (anim == null || core == null) return;

        // ✅ NO actualizar animaciones si el jefe está muerto
        if (core.IsDead) return;

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
        if (hasSpeedXParam && core.rb != null)
        {
            anim.SetFloat("SpeedX", Mathf.Abs(core.rb.linearVelocity.x));
        }
    }

    // ✅ Damage mejorado: soporta Bool y Trigger
    public void SetDamage(bool value)
    {
        // ✅ Solo procesar si hay algún parámetro de damage
        if (!hasDamageBoolParam && !hasDamageTriggerParam)
            return;

        // Prioridad a Trigger
        if (hasDamageTriggerParam && value)
        {
            anim.SetTrigger(damageTriggerParamName);
        }
        // Fallback a Bool
        else if (hasDamageBoolParam)
        {
            anim.SetBool(damageBoolParamName, value);
        }
    }

    // ✅ Death mejorado: soporta Bool y Trigger
    public void SetDeath(bool value)
    {
        if (!value) return; // Solo procesar cuando value = true

        // ✅ Solo procesar si hay algún parámetro de death
        if (!hasDeathBoolParam && !hasDeathTriggerParam)
        {
            Debug.Log($"ℹ️ [{gameObject.name}] Sin animación de muerte configurada");
            return;
        }

        Debug.Log($"🎬 [{gameObject.name}] Activando animación de muerte");

        // Prioridad 1: Usar Trigger si existe
        if (hasDeathTriggerParam)
        {
            anim.SetTrigger(deathTriggerParamName);
            Debug.Log($"   ✅ SetTrigger('{deathTriggerParamName}')");
        }
        // Prioridad 2: Usar Bool si no hay Trigger
        else if (hasDeathBoolParam)
        {
            // Resetear damage primero
            if (hasDamageBoolParam)
            {
                anim.SetBool(damageBoolParamName, false);
            }

            anim.SetBool(deathBoolParamName, true);
            Debug.Log($"   ✅ SetBool('{deathBoolParamName}', true)");

            // Forzar actualización inmediata
            anim.Update(0f);
        }
    }

    // ✅ Phase mejorado: sin warnings si no existe
    public void SetPhase(int phase)
    {
        if (!hasPhaseParam)
        {
            // Sin warning molesto, es opcional
            return;
        }

        anim.SetInteger("Phase", phase);
        Debug.Log($"🔄 [{gameObject.name}] Fase cambiada a: {phase}");
    }

    // ========== ANIMATION EVENTS ==========
    // Estos métodos pueden ser llamados desde Animation Events

    public void OnDamageEnd()
    {
        SetDamage(false);
        if (core != null)
        {
            core.SetTakingDamage(false);
        }
    }

    public void OnDeathAnimationEnd()
    {
        Debug.Log($"✅ [{gameObject.name}] Animación de muerte completada");
        // Aquí puedes llamar al método de muerte del jefe
        // O simplemente destruir el GameObject
    }
}