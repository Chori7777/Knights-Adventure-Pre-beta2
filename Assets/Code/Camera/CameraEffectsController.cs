using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraEffectsController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private float defaultSaturation = 0f;
    [SerializeField] private float defaultVignette = 0.2f;

    private ColorAdjustments colorAdj;
    private Vignette vignette;

    private void Awake()
    {
        if (volume == null)
            volume = GetComponent<Volume>();
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out colorAdj);
            volume.profile.TryGet(out vignette);
        }
    }

    public void ApplyDesaturation(float targetSaturation, float duration)
    {
        if (colorAdj == null) return;
        StartCoroutine(LerpSaturation(targetSaturation, duration));
    }

    public void ApplyVignette(float targetIntensity, float duration)
    {
        if (vignette == null) return;
        StartCoroutine(LerpVignette(targetIntensity, duration));
    }

    public void ResetEffects(float duration = 0.5f)
    {
        if (colorAdj != null) StartCoroutine(LerpSaturation(defaultSaturation, duration));
        if (vignette != null) StartCoroutine(LerpVignette(defaultVignette, duration));
    }

    private IEnumerator LerpSaturation(float target, float duration)
    {
        float start = colorAdj.saturation.value;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float val = Mathf.Lerp(start, target, t / duration);
            colorAdj.saturation.Override(val);
            yield return null;
        }
        colorAdj.saturation.Override(target);
    }

    private IEnumerator LerpVignette(float target, float duration)
    {
        float start = vignette.intensity.value;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float val = Mathf.Lerp(start, target, t / duration);
            vignette.intensity.Override(val);
            yield return null;
        }
        vignette.intensity.Override(target);
    }
}
