using UnityEngine;
using System;

public class TrueFinalBossStateMachine : MonoBehaviour
{
    public enum BossState { Idle, Intro, Transformation, Combat, Reflection, FinalForm, AutoDefeat }

    [SerializeField] private BossState currentState = BossState.Idle;
    [SerializeField] private float introDuration = 3f;
    [SerializeField] private float transformationDuration = 2f;
    [SerializeField] private float reflectionDuration = 10f;
    [SerializeField] private float autoDefeatDuration = 3f;

    public event Action<BossState> OnStateEnter;
    public event Action<BossState> OnStateExit;

    public BossState GetCurrentState() { return currentState; }

    public void ChangeState(BossState next)
    {
        if (currentState == next) return;
        var prev = currentState;
        currentState = next;
        OnStateExit?.Invoke(prev);
        OnStateEnter?.Invoke(currentState);
    }

    public float GetStateDuration(BossState s)
    {
        switch (s)
        {
            case BossState.Intro: return introDuration;
            case BossState.Transformation: return transformationDuration;
            case BossState.Reflection: return reflectionDuration;
            case BossState.AutoDefeat: return autoDefeatDuration;
            default: return 0f;
        }
    }
}

