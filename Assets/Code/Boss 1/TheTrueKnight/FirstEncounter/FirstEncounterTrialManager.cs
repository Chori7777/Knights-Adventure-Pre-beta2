using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static FirstEncounterTrialManager;

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
    [SerializeField] private string[] dialogosFase2;
    [Header("HUD")]
    [SerializeField] private TMPro.TextMeshProUGUI hudTimerText;
    [Header("Área Inicial del Combate")]
    [SerializeField] private Transform areaInicialPlayerSpawn;
    [SerializeField] private Transform areaInicialCameraTarget;
    [Header("Música Fase 2")]
    [SerializeField] private AudioClip musicaFase2;
    [SerializeField] private float musicaFase2Volume = 0.5f;

    private int index;
    private float timer;
    private bool running;
    private bool accelerated;
    private bool superAccelerated;
    private BossLife boss;
    private Coroutine timerRoutine;

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
        float t = accelerated ? tiempoFase2 : tiempoFase1;
        if (superAccelerated) t = Mathf.Max(1f, tiempoFase2 * 0.75f);
        return Mathf.Max(1f, t);
    }

    private void StartTrial(int i)
    {
        if (i < 0 || i >= trials.Length)
        {
            EndSequenceVictory();
            return;
        }

        Trial t = trials[i];
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = true;
        timer = GetTime();

        // ✅ Verificar que gestorTeleport no sea null
        if (gestorTeleport == null)
        {
            Debug.LogError("❌ gestorTeleport es NULL en FirstEncounterTrialManager!");
            return;
        }

        // ✅ ASEGURARSE que el jefe esté ACTIVO antes de teletransportar
        if (boss != null && !boss.gameObject.activeSelf)
        {
            boss.gameObject.SetActive(true);
            Debug.Log("✅ Jefe activado para trial");
        }

        gestorTeleport.TeleportTo(t.playerSpawn, t.cameraTarget);

        // ✅ NUEVO: Teletransportar al jefe también
        if (boss != null && t.bossSpawn != null)
        {
            boss.transform.position = t.bossSpawn.position;
            Debug.Log($"✅ Jefe teletransportado a: {t.bossSpawn.position}");
        }

        timerRoutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (running)
        {
            timer -= Time.deltaTime;
            if (hudTimerText != null)
            {
                hudTimerText.text = Mathf.CeilToInt(timer).ToString();
            }
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

            // ✅ ASEGURARSE que esté activo
            if (!boss.gameObject.activeSelf)
            {
                boss.gameObject.SetActive(true);
            }

            Debug.Log($"✅ Jefe aparece en: {t.bossSpawn.position}");
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
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
        running = false;
        if (gestorTeleport != null && areaInicialPlayerSpawn != null)
        {
            gestorTeleport.TeleportInstant(areaInicialPlayerSpawn, areaInicialCameraTarget, 1.2f);
            yield return new WaitForSeconds(1.2f);
        }

        if (AudioManager.Instance != null && musicaFase2 != null)
        {
            AudioManager.Instance.PlayMusic(musicaFase2, musicaFase2Volume, true, true);
        }

        if (gestorTexto != null)
        {
            if (dialogosFase2 != null && dialogosFase2.Length > 0)
            {
                for (int i = 0; i < dialogosFase2.Length; i++)
                {
                    gestorTexto.ShowDialogue(dialogosFase2[i]);
                    yield return new WaitForSeconds(2f);
                }
                gestorTexto.CloseDialogue();
            }
            else if (!string.IsNullOrEmpty(dialogoFase2))
            {
                gestorTexto.ShowDialogue(dialogoFase2);
                yield return new WaitForSeconds(3f);
                gestorTexto.CloseDialogue();
            }
        }
        StartTrial(index);
    }

    public void TeleportToInitialArea()
    {
        if (gestorTeleport != null && areaInicialPlayerSpawn != null)
        {
            gestorTeleport.TeleportInstant(areaInicialPlayerSpawn, areaInicialCameraTarget, 1.2f);
        }
    }

    public void SetSuperAccelerated(bool value)
    {
        superAccelerated = value;
    }

    public float RemainingTime => timer;
}
