using System;
using UnityEngine;

public class TownStalkerAttackAdapter : MonoBehaviour, IAttackPattern
{
    public event Action OnFinished;
    [SerializeField] private TownStalkerEntityController controller;
    [SerializeField] private float duration = 8f;
    [SerializeField] private bool autoStartOnEnable = true;
    private bool running;
    private float startTime;

    private void OnEnable()
    {
        if (autoStartOnEnable) StartAttack();
    }

    private void Update()
    {
        if (running && Time.time - startTime >= duration)
        {
            StopAttack();
            OnFinished?.Invoke();
        }
    }

    public void StartAttack()
    {
        running = true;
        startTime = Time.time;
        if (controller != null) controller.enabled = true;
    }

    public void StopAttack()
    {
        running = false;
        if (controller != null) controller.enabled = false;
    }
}
