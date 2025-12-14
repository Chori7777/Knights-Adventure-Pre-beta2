using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrueFinalBossVisualEffects : MonoBehaviour
{
    [SerializeField] private CameraEffectsController cameraFX;
    [SerializeField] private int redPresetIndex = 0;
    [SerializeField] private Image blackOverlayImage;
    [SerializeField] private float overlayMaxAlpha = 1f;
    [SerializeField] private bool avoidPresetZoom = true;
    private Coroutine overlayPulseRoutine;

    public void ActivateRedBackground()
    {
        if (cameraFX != null)
        {
            if (avoidPresetZoom) cameraFX.ApplyPresetIndexNoZoom(redPresetIndex);
            else cameraFX.ApplyPresetIndex(redPresetIndex);
        }
    }

    public void PlayPreset(int presetIndex)
    {
        if (cameraFX != null)
        {
            if (avoidPresetZoom) cameraFX.ApplyPresetIndexNoZoom(presetIndex);
            else cameraFX.ApplyPresetIndex(presetIndex);
        }
    }

    public void FadeToBlack(float duration)
    {
        if (blackOverlayImage != null)
        {
            StartCoroutine(OverlayFade(overlayMaxAlpha, duration));
        }
        else if (cameraFX != null)
        {
            cameraFX.ApplyVignette(1f, duration);
        }
        else
        {
            Debug.LogWarning("[TrueFinalBossVFX] No hay overlay ni CameraEffectsController asignados para FadeToBlack");
        }
    }

    public void OverlayImmediate(float alpha)
    {
        if (blackOverlayImage == null) return;
        var c = blackOverlayImage.color;
        c.a = Mathf.Clamp01(alpha);
        blackOverlayImage.color = c;
    }

    public void ResetEffects(float duration)
    {
        if (cameraFX != null) cameraFX.ResetEffects(duration);
        if (blackOverlayImage != null) StartCoroutine(OverlayFade(0f, duration));
    }

    public void ShakeCamera(float duration, float intensity)
    {
        StartCoroutine(ShakeRoutine(duration, intensity));
    }

    public void StartBlackPulse(float minIntensity, float maxIntensity, float period)
    {
        if (blackOverlayImage != null)
        {
            if (overlayPulseRoutine != null) StopCoroutine(overlayPulseRoutine);
            overlayPulseRoutine = StartCoroutine(OverlayPulse(minIntensity, maxIntensity, period));
        }
        else if (cameraFX != null)
        {
            cameraFX.StartVignettePulse(Color.black, minIntensity, maxIntensity, period);
        }
        else
        {
            Debug.LogWarning("[TrueFinalBossVFX] No hay overlay ni CameraEffectsController asignados para pulso negro");
        }
    }

    public void StopPulse()
    {
        if (blackOverlayImage != null)
        {
            if (overlayPulseRoutine != null) StopCoroutine(overlayPulseRoutine);
            overlayPulseRoutine = null;
            var c = blackOverlayImage.color; c.a = 0f; blackOverlayImage.color = c;
        }
        if (cameraFX != null) cameraFX.StopVignettePulse();
    }

    private IEnumerator ShakeRoutine(float duration, float intensity)
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        var t = cam.transform;
        var original = t.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            t.position = new Vector3(original.x + x, original.y + y, original.z);
            yield return null;
        }
        t.position = original;
    }

    private IEnumerator OverlayFade(float targetAlpha, float duration)
    {
        if (blackOverlayImage == null) yield break;
        Color c = blackOverlayImage.color;
        float start = c.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            c.a = Mathf.Lerp(start, targetAlpha, k);
            blackOverlayImage.color = c;
            yield return null;
        }
        c.a = targetAlpha;
        blackOverlayImage.color = c;
    }

    private IEnumerator OverlayPulse(float min, float max, float period)
    {
        if (blackOverlayImage == null) yield break;
        min = Mathf.Clamp01(min);
        max = Mathf.Clamp01(max);
        period = Mathf.Max(0.01f, period);
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            float p = (Mathf.Sin((timer / period) * Mathf.PI * 2f) * 0.5f) + 0.5f;
            float a = Mathf.Lerp(min, max, p);
            var c = blackOverlayImage.color; c.a = a; blackOverlayImage.color = c;
            yield return null;
        }
    }
}
