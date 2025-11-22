using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Sistema de notificación visual para confirmación de guardado
/// </summary>
public class SaveNotification : MonoBehaviour
{
    public static SaveNotification Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image iconImage;

    [Header("Iconos")]
    [SerializeField] private Sprite saveIcon;
    [SerializeField] private Sprite loadIcon;
    [SerializeField] private Sprite errorIcon;

    [Header("Configuración de Animación")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Posición")]
    [SerializeField] private Vector2 startOffset = new Vector2(0, -50);

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine currentNotification;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Mostrar notificación de guardado exitoso
    /// </summary>
    public void ShowSaveSuccess()
    {
        ShowNotification("💾 Progreso guardado", saveIcon);
    }

    /// <summary>
    /// Mostrar notificación de carga exitosa
    /// </summary>
    public void ShowLoadSuccess()
    {
        ShowNotification("📂 Partida cargada", loadIcon);
    }

    /// <summary>
    /// Mostrar notificación de error
    /// </summary>
    public void ShowError(string message)
    {
        ShowNotification($"❌ {message}", errorIcon);
    }

    /// <summary>
    /// Mostrar notificación personalizada
    /// </summary>
    public void ShowNotification(string message, Sprite icon = null)
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }

        currentNotification = StartCoroutine(NotificationSequence(message, icon));
    }

    private IEnumerator NotificationSequence(string message, Sprite icon)
    {
        gameObject.SetActive(true);

        // Configurar contenido
        if (messageText != null)
            messageText.text = message;

        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Reset posición
        rectTransform.anchoredPosition = originalPosition + startOffset;

        // Fade In + Slide Up
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(
                originalPosition + startOffset,
                originalPosition,
                EaseOutCubic(t)
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = originalPosition;

        // Mantener visible
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        currentNotification = null;
    }

    // Easing function para animación suave
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}

// ========== INTEGRACIÓN CON ControladorDatosJuego ==========
// En GuardarDatos(), después de guardar:
/*
if (SaveNotification.Instance != null)
{
    SaveNotification.Instance.ShowSaveSuccess();
}
*/

// En CargarDatos(), después de cargar:
/*
if (SaveNotification.Instance != null)
{
    SaveNotification.Instance.ShowLoadSuccess();
}
*/