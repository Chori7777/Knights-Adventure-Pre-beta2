using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Sistema de tutorial al desbloquear habilidades (con CanvasGroup)
/// </summary>
public class UnlockTutorialUI : MonoBehaviour
{
    public static UnlockTutorialUI Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private CanvasGroup fadeOverlay; // ✅ CAMBIO: Ahora es CanvasGroup
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI buttonPrompt;

    [Header("Configuración Visual")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Animación")]
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private string showTrigger = "Show";
    [SerializeField] private string hideTrigger = "Hide";

    [Header("Audio")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip closeSound;

    private bool isShowing = false;
    private float originalTimeScale = 1f;

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

        // Ocultar panel al inicio
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f; // ✅ Invisible al inicio
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Cerrar con cualquier tecla cuando está mostrando
        if (isShowing && Input.anyKeyDown)
        {
            HideTutorial();
        }
    }

    /// <summary>
    /// Mostrar tutorial de desbloqueo
    /// </summary>
    public void ShowUnlock(Sprite icon, string itemName, string description)
    {
        if (isShowing) return;

        StartCoroutine(ShowTutorialSequence(icon, itemName, description));
    }

    private IEnumerator ShowTutorialSequence(Sprite icon, string itemName, string description)
    {
        isShowing = true;

        // Guardar timeScale original
        originalTimeScale = Time.timeScale;

        // ========== PASO 1: PAUSAR JUEGO ==========
        Time.timeScale = 0f;

        // ========== PASO 2: SONIDO DE DESBLOQUEO ==========
        if (unlockSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(unlockSound, 0.8f);
        }

        // ========== PASO 3: FADE IN DE OVERLAY (CanvasGroup) ==========
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration); // ✅ Cambia el alpha
                yield return null;
            }

            fadeOverlay.alpha = 1f; // ✅ Asegurar opacidad completa
        }

        // ========== PASO 4: MOSTRAR PANEL CON CONTENIDO ==========
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);

            // Configurar contenido
            if (itemIcon != null && icon != null)
                itemIcon.sprite = icon;

            if (titleText != null)
                titleText.text = itemName;

            if (descriptionText != null)
                descriptionText.text = description;

            if (buttonPrompt != null)
                buttonPrompt.text = "Presiona cualquier tecla para continuar";

            // Animación de entrada
            if (panelAnimator != null && !string.IsNullOrEmpty(showTrigger))
            {
                panelAnimator.SetTrigger(showTrigger);
            }
        }

        Debug.Log($"🔓 Tutorial mostrado: {itemName}");
    }

    /// <summary>
    /// Ocultar tutorial y reanudar juego
    /// </summary>
    public void HideTutorial()
    {
        if (!isShowing) return;

        StartCoroutine(HideTutorialSequence());
    }

    private IEnumerator HideTutorialSequence()
    {
        // Sonido de cierre
        if (closeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(closeSound, 0.5f);
        }

        // ========== PASO 1: ANIMACIÓN DE SALIDA ==========
        if (panelAnimator != null && !string.IsNullOrEmpty(hideTrigger))
        {
            panelAnimator.SetTrigger(hideTrigger);
            yield return new WaitForSecondsRealtime(0.3f); // Tiempo para la animación
        }

        // ========== PASO 2: OCULTAR PANEL ==========
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // ========== PASO 3: FADE OUT DE OVERLAY (CanvasGroup) ==========
        if (fadeOverlay != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration); //  Cambia el alpha
                yield return null;
            }

            fadeOverlay.alpha = 0f; // ✅ Totalmente invisible
            fadeOverlay.gameObject.SetActive(false);
        }

        // ========== PASO 4: REANUDAR JUEGO ==========
        Time.timeScale = originalTimeScale;
        isShowing = false;

        Debug.Log(" Tutorial cerrado, juego reanudado");
    }

    /// <summary>
    /// Verificar si el tutorial está activo
    /// </summary>
    public bool IsShowing => isShowing;
}

// ========== CONFIGURACIÓN EN UNITY ==========
//
// 1. Crea un GameObject vacío con CanvasGroup (el overlay)
// 2. Configura el CanvasGroup:
//    - Alpha: 0 (invisible al inicio)
//    - Blocks Raycasts: activado
// 3. Añade un Image hijo al CanvasGroup con color negro/oscuro
// 4. Arrastra el CanvasGroup al campo "Fade Overlay" del script