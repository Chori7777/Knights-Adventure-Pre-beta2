using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WarningBlink : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private float blinkInterval = 0.15f;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool destroyOnEnd = true;
    [SerializeField] private AudioClip warningSFX;
    [SerializeField] private bool playSFX = false;

    private SpriteRenderer spriteRenderer;
    private Image uiImage;
    private CanvasGroup canvasGroup;
    private Coroutine routine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (autoStart) StartBlink();
    }

    public void StartBlink()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        float elapsed = 0f;
        bool state = true;
        SetVisible(true);
        if (playSFX && warningSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(warningSFX);
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
            state = !state;
            SetVisible(state);
        }
        SetVisible(true);
        routine = null;
        if (destroyOnEnd) Destroy(gameObject);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            return;
        }
        if (uiImage != null)
        {
            uiImage.enabled = visible;
            return;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
            return;
        }
        gameObject.SetActive(visible);
    }
}
