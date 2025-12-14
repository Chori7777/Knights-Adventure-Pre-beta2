using UnityEngine;

public class TrueFinalBossTrigger : MonoBehaviour
{
    [SerializeField] private TrueFinalBossController controller;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bossIntroSound;
    [SerializeField] private bool enableAllAttackScripts = true;
    [SerializeField] private bool disableZones = true;
    [SerializeField] private bool skipIntroDialogues = true;

    [SerializeField] private string bossName = "TheTrueKnight";
    [SerializeField] private string startDialogue = "";
    [SerializeField] private bool waitDialogueToStart = true;
    [SerializeField] private bool logDebug = true;
    [SerializeField] private float silenceSeconds = 0f;
    [SerializeField] private string afterTransformationDialogue1;
    [SerializeField] private string afterTransformationDialogue2;
    [SerializeField] private string combatDialogue;
    [SerializeField] private string[] introDialogues;
    [SerializeField] private string[] postTransformationDialogues;
    [SerializeField] private string[] preCombatDialogues;
    [SerializeField] private string[] preMusicDialogues;

    private void Start()
    {
        if (logDebug) Debug.Log("[TrueFinalBossTrigger] Inicializando boss trigger");
        if (controller != null)
        {
            controller.SetZoneChangesDisabled(disableZones);
            controller.SetBossGlobalAttacksEnabled(enableAllAttackScripts);
            controller.Init();
            controller.OnTransformationComplete += OnTransformationComplete;
            if (skipIntroDialogues)
            {
                if (musicSource != null) musicSource.Play();
                controller.EnterCombat();
                TryShowBossHealthUI();
                return;
            }
            controller.StartIntro();
        }
        if (BossNameUI.Instance != null)
        {
            BossNameUI.Instance.MostrarNombre(bossName);
            if (logDebug) Debug.Log($"[TrueFinalBossTrigger] Nombre del jefe mostrado: {bossName}");
        }
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            if (logDebug) Debug.Log("[TrueFinalBossTrigger] Música del jefe detenida al inicio para evitar reproducción automática");
        }
        if (string.IsNullOrEmpty(startDialogue) && logDebug) Debug.LogWarning("[TrueFinalBossTrigger] startDialogue está vacío, no se mostrará diálogo inicial");
        if (TextManager.Instance == null && logDebug) Debug.LogWarning("[TrueFinalBossTrigger] No hay TextManager en la escena; añade el componente para mostrar diálogos");
        StartCoroutine(OrchestrationSequence());
    }

    private bool transformationDone;
    private void OnTransformationComplete() { transformationDone = true; }

    private System.Collections.IEnumerator OrchestrationSequence()
    {
        if (bossIntroSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(bossIntroSound, 0.8f, 1f);
            if (logDebug) Debug.Log("[TrueFinalBossTrigger] SFX de spawn reproducido");
        }
        if (preMusicDialogues != null && preMusicDialogues.Length > 0)
        {
            if (TextManager.Instance != null)
            {
                yield return TextManager.Instance.PlaySequenceAndWait(preMusicDialogues);
            }
            else
            {
                for (int i = 0; i < preMusicDialogues.Length; i++)
                {
                    yield return ShowDialogueAndWait(preMusicDialogues[i]);
                }
            }
        }
        else if (!string.IsNullOrEmpty(startDialogue))
        {
            yield return ShowDialogueAndWait(startDialogue);
        }

        if (musicSource != null)
        {
            musicSource.Play();
            if (logDebug) Debug.Log("[TrueFinalBossTrigger] Música del jefe iniciada tras diálogos");
            if (controller != null) controller.StopPulse();
        }
        
        if (controller != null)
        {
            controller.EnterCombat();
            if (logDebug) Debug.Log("[TrueFinalBossTrigger] Combate iniciado manualmente");
            TryShowBossHealthUI();
        }
    }

    private System.Collections.IEnumerator ShowDialogueAndWait(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowDialogue(text);
            if (logDebug) Debug.Log($"[TrueFinalBossTrigger] Diálogo mostrado: {text}");
            while (TextManager.IsOpen) yield return null;
        }
        else
        {
            Debug.LogWarning("[TrueFinalBossTrigger] No hay TextManager en escena, mostrando por log: " + text);
            yield return new UnityEngine.WaitForSecondsRealtime(1f);
        }
    }

    private void TryShowBossHealthUI()
    {
        var life = FindFirstObjectByType<BossLife>(FindObjectsInactive.Include);
        var bossUI = BossHealthUI.Instance != null
            ? BossHealthUI.Instance
            : FindFirstObjectByType<BossHealthUI>(FindObjectsInactive.Include);
        if (life != null && bossUI != null)
        {
            if (!bossUI.gameObject.activeSelf)
                bossUI.gameObject.SetActive(true);
            bossUI.ShowForBoss(life);
            if (logDebug) Debug.Log("[TrueFinalBossTrigger] BossHealthUI mostrado para True Final Boss");
        }
        else
        {
            Debug.LogWarning("[TrueFinalBossTrigger] No se encontró BossLife o BossHealthUI en la escena");
        }
    }
}
