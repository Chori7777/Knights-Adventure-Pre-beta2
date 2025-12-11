using UnityEngine;
using System.Collections;

public class TrueFinalBossController : MonoBehaviour
{
    [SerializeField] private AudioSource music;
    [SerializeField] private TrueFinalBossMusicSync musicSync;
    [SerializeField] private TrueFinalBossStateMachine stateMachine;
    [SerializeField] private TrueFinalBossZoneManager zoneManager;
    [SerializeField] private TrueFinalBossVisualEffects vfx;
    [SerializeField] private Transform boss;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 introFinalPosition;
    [SerializeField] private float introSpeed = 3f;
    [SerializeField] private TrueFinalBossSnowBridge snowBridge;
    [SerializeField] private TrueFinalBossAlterTownAttackController alterController;

    private Coroutine introRoutine;

    public void Init()
    {
        if (musicSync != null)
        {
            musicSync.OnZoneEvent += OnZoneEvent;
            musicSync.OnStateEvent += OnStateEvent;
            musicSync.OnMusicPaused += OnMusicPaused;
            musicSync.OnMusicResumed += OnMusicResumed;
        }
        if (stateMachine != null)
        {
            stateMachine.OnStateEnter += OnEnterState;
            stateMachine.OnStateExit += OnExitState;
        }
    }

    public void BeginFight()
    {
        StartIntro();
    }

    private void OnZoneEvent(string name)
    {
        if (zoneManager != null) zoneManager.ActivateZone(name);
        if (name == "Snow")
        {
            if (snowBridge != null) snowBridge.ActivateByNumber(1);
            if (alterController != null) alterController.StopZone();
        }
        else if (name == "AlterTown")
        {
            if (alterController != null) alterController.StartZone();
            if (snowBridge != null) snowBridge.StopAll();
        }
    }

    private void OnStateEvent(TrueFinalBossStateMachine.BossState s)
    {
        if (stateMachine != null) stateMachine.ChangeState(s);
    }

    private void OnMusicPaused() { }
    private void OnMusicResumed() { }

    private void OnEnterState(TrueFinalBossStateMachine.BossState s)
    {
        if (s == TrueFinalBossStateMachine.BossState.Intro)
        {
            StartIntro();
        }
        else if (s == TrueFinalBossStateMachine.BossState.Reflection)
        {
            if (snowBridge != null) snowBridge.StopAll();
            if (alterController != null) alterController.StopZone();
        }
        else if (s == TrueFinalBossStateMachine.BossState.AutoDefeat)
        {
            if (snowBridge != null) snowBridge.StopAll();
            if (alterController != null) alterController.StopZone();
        }
    }

    private void OnExitState(TrueFinalBossStateMachine.BossState s) { }

    public void StartIntro()
    {
        if (introRoutine != null) StopCoroutine(introRoutine);
        introRoutine = StartCoroutine(IntroCinematic());
    }

    private IEnumerator IntroCinematic()
    {
        if (vfx != null) vfx.ActivateRedBackground();
        if (boss != null)
        {
            Vector3 target = introFinalPosition;
            while (Vector3.Distance(boss.position, target) > 0.01f)
            {
                boss.position = Vector3.MoveTowards(boss.position, target, introSpeed * Time.deltaTime);
                yield return null;
            }
        }
        if (stateMachine != null) stateMachine.ChangeState(TrueFinalBossStateMachine.BossState.Combat);
        introRoutine = null;
    }
}

