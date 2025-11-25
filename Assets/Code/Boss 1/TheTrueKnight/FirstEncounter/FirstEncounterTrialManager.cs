using UnityEngine;
using System.Collections;

public class FirstEncounterTrialManager : MonoBehaviour
{
    [System.Serializable]
    public class Trial
    {
        public Transform playerSpawn;
        public Transform bossSpawn;
        public Transform cameraTarget;
    }

    [Header("Trials")]
    [SerializeField] private Trial[] trials;
    [Header("Tiempo por Fase")]
    [SerializeField] private float tiempoFase1 = 15f;
    [SerializeField] private float tiempoFase2 = 8f;
    [Header("Teleport")]
    [SerializeField] private FirstEncounterTeleportManager gestorTeleport;
    [Header("Diálogo Fase 2")]
    [SerializeField] private TextManager gestorTexto;
    [SerializeField] private string dialogoFase2;

    private int index;
    private float timer;
    private bool running;
    private bool accelerated;
    private BossLife boss;

    private void Awake()
    {
        if (gestorTexto == null)
        {
            gestorTexto = TextManager.Instance != null ? TextManager.Instance : FindFirstObjectByType<TextManager>();
        }
    }

    public void BeginSequence(BossLife bossLife)
    {
        boss = bossLife;
        accelerated = false;
        index = 0;
        StartTrial(index);
    }

    public void SetAccelerated(bool value)
    {
        accelerated = value;
    }

    private float GetTime()
    {
        return accelerated ? tiempoFase2 : tiempoFase1;
    }

    private void StartTrial(int i)
    {
        if (i < 0 || i >= trials.Length) { EndSequenceVictory(); return; }
        Trial t = trials[i];
        running = true;
        timer = GetTime();
        gestorTeleport.TeleportTo(t.playerSpawn, t.cameraTarget);
        StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (running)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                TrialFailed();
                yield break;
            }
            yield return null;
        }
    }

    public void TrialCompleted()
    {
        if (!running) return;
        running = false;
        Trial t = trials[index];
        if (boss != null)
        {
            boss.transform.position = t.bossSpawn.position;
            boss.gameObject.SetActive(true);
        }
    }

    public void TrialFailed()
    {
        running = false;
        NextTrial();
    }

    public void NextTrial()
    {
        index = (index + 1) % trials.Length;
        StartTrial(index);
    }

    public void EndSequenceVictory()
    {
        running = false;
    }

    public void PauseForDialoguePhase2()
    {
        StartCoroutine(Phase2PauseRoutine());
    }

    private IEnumerator Phase2PauseRoutine()
    {
        running = false;
        if (gestorTexto != null && !string.IsNullOrEmpty(dialogoFase2))
        {
            gestorTexto.ShowDialogue(dialogoFase2);
            yield return new WaitForSeconds(3f);
            gestorTexto.CloseDialogue();
        }
        StartTrial(index);
    }

    public float RemainingTime => timer;
}
