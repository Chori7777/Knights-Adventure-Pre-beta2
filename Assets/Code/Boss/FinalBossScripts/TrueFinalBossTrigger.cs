using UnityEngine;

public class TrueFinalBossTrigger : MonoBehaviour
{
    [SerializeField] private TrueFinalBossController controller;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bossIntroSound;
    [SerializeField] private string bossName;

    private void Start()
    {
        if (controller != null) controller.Init();
        if (BossNameUI.Instance != null)
        {
            BossNameUI.Instance.MostrarNombre(bossName);
        }
        if (bossIntroSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(bossIntroSound, 0.5f, 1f);
        }
        if (musicSource != null) musicSource.Play();
        if (controller != null) controller.BeginFight();
    }
}

