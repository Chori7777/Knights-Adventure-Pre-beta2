using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    private BossLife currentBoss;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowForBoss(BossLife boss)
    {
        currentBoss = boss;
        if (healthFillImage != null)
        {
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Vertical;
            healthFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            float amt = boss.maxHealth > 0 ? (float)boss.health / boss.maxHealth : 0f;
            healthFillImage.fillAmount = amt;
        }
        if (healthText != null)
        {
            healthText.text = boss.health + "/" + boss.maxHealth;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthFillImage != null)
        {
            float amt = max > 0 ? (float)current / max : 0f;
            healthFillImage.fillAmount = amt;
        }
        if (healthText != null)
        {
            healthText.text = current + "/" + max;
        }
    }

    public void Hide()
    {
        currentBoss = null;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
