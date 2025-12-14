using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraEffectsController : MonoBehaviour
{
    [System.Serializable]
    public struct EffectPreset
    {
        public string name;
        public float vignetteIntensity;
        public float saturation;
        public float zoom;
        public float duration;
    }
    [SerializeField] private Volume volume;
    [SerializeField] private float defaultSaturation = 0f;
    [SerializeField] private float defaultVignette = 0.2f;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool useOrthographicZoom = true;
    [SerializeField] private ParticleSystem[] particleTargets;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool useMusicSource = false;
    [SerializeField] private bool autoStartOnMusicPlay = true;
    [SerializeField] private bool autoStartOnEnable = false;
    [SerializeField] private bool loopTimeline = false;
    [SerializeField] private float timelineDuration = 10f;
    [SerializeField] private AnimationCurve vignetteCurve;
    [SerializeField] private AnimationCurve saturationCurve;
    [SerializeField] private AnimationCurve zoomCurve;
    [SerializeField] private AnimationCurve particlesCurve;

    private ColorAdjustments colorAdj;
    private Vignette vignette;
    private bool running;
    private float startTime;
    private float lastMusicTime;
    private float baseZoom;
    [SerializeField] private bool debugPulse = false;
    [SerializeField] private float debugPulseMinVignette = 0.2f;
    [SerializeField] private float debugPulseMaxVignette = 0.4f;
    [SerializeField] private float debugPulsePeriod = 4f;
    [SerializeField] private bool autoConfigureCameraPostFX;
    [SerializeField] private bool enforceVignetteAlpha = false;
    [SerializeField] private bool applyDefaultOnStart = false;
    [SerializeField] private EffectPreset[] presets;

    private void Awake()
    {
        if (volume == null)
            volume = GetComponent<Volume>();
        if (volume == null && GlobalPostFX.Instance != null)
            volume = GlobalPostFX.Instance.volume;
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out colorAdj);
            volume.profile.TryGet(out vignette);
            var profile = volume.profile;
            if (colorAdj == null)
            {
                colorAdj = profile.Add<ColorAdjustments>(true);
                colorAdj.saturation.Override(defaultSaturation);
            }
            if (vignette == null)
            {
                vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(defaultVignette);
                var c = vignette.color.value; c.a = 1f; vignette.color.Override(c);
            }
            if (vignette != null)
            {
                if (enforceVignetteAlpha)
                {
                    Color c = vignette.color.value;
                    if (c.a < 0.95f)
                    {
                        c.a = 1f;
                        vignette.color.Override(c);
                    }
                }
                if (applyDefaultOnStart)
                {
                    vignette.intensity.Override(defaultVignette);
                }
            }
        }
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null)
        {
            baseZoom = useOrthographicZoom ? targetCamera.orthographicSize : targetCamera.fieldOfView;
            EnsureURPPostProcessing(targetCamera);
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

    public void StartVignettePulse(Color color, float minIntensity, float maxIntensity, float period)
    {
        debugPulseMinVignette = Mathf.Clamp01(minIntensity);
        debugPulseMaxVignette = Mathf.Clamp01(maxIntensity);
        debugPulsePeriod = Mathf.Max(0.01f, period);
        debugPulse = true;
        if (vignette != null)
        {
            var c = color;
            c.a = 1f;
            vignette.color.Override(c);
        }
    }

    public void StopVignettePulse()
    {
        debugPulse = false;
    }

    public void ResetEffects(float duration = 0.5f)
    {
        if (colorAdj != null) StartCoroutine(LerpSaturation(defaultSaturation, duration));
        if (vignette != null) StartCoroutine(LerpVignette(defaultVignette, duration));
        if (targetCamera != null)
        {
            if (useOrthographicZoom) targetCamera.orthographicSize = baseZoom; else targetCamera.fieldOfView = baseZoom;
        }
    }

    public void ApplyPreset(string presetName)
    {
        if (presets == null || presets.Length == 0) return;
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i].name == presetName)
            {
                ApplyPresetIndex(i);
                return;
            }
        }
    }

    public void ApplyPresetIndex(int index)
    {
        if (presets == null || index < 0 || index >= presets.Length) return;
        var p = presets[index];
        if (colorAdj != null) StartCoroutine(LerpSaturation(p.saturation, p.duration));
        if (vignette != null) StartCoroutine(LerpVignette(p.vignetteIntensity, p.duration));
        if (targetCamera != null)
        {
            float targetZoom = useOrthographicZoom ? Mathf.Max(0.001f, baseZoom + p.zoom) : Mathf.Max(1f, baseZoom + p.zoom);
            StartCoroutine(LerpZoom(targetZoom, p.duration));
        }
    }

    public void ApplyPresetIndexNoZoom(int index)
    {
        if (presets == null || index < 0 || index >= presets.Length) return;
        var p = presets[index];
        if (colorAdj != null) StartCoroutine(LerpSaturation(p.saturation, p.duration));
        if (vignette != null) StartCoroutine(LerpVignette(p.vignetteIntensity, p.duration));
        // Intencionalmente no ajustar zoom
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

    private IEnumerator LerpZoom(float target, float duration)
    {
        if (targetCamera == null) yield break;
        float start = useOrthographicZoom ? targetCamera.orthographicSize : targetCamera.fieldOfView;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float val = Mathf.Lerp(start, target, t / duration);
            if (useOrthographicZoom) targetCamera.orthographicSize = val; else targetCamera.fieldOfView = val;
            yield return null;
        }
        if (useOrthographicZoom) targetCamera.orthographicSize = target; else targetCamera.fieldOfView = target;
    }

    private void OnEnable()
    {
        if (!running)
        {
            if (useMusicSource && autoStartOnMusicPlay && musicSource != null && musicSource.isPlaying)
                StartTimeline();
            else if (autoStartOnEnable && !useMusicSource)
                StartTimeline();
        }
    }

    public void StartTimeline()
    {
        running = true;
        startTime = Time.time;
        lastMusicTime = useMusicSource && musicSource != null ? musicSource.time : 0f;
    }

    public void StopTimeline()
    {
        running = false;
    }

    private void Update()
    {
        float tGlobal = useMusicSource && musicSource != null ? musicSource.time : (Time.time - startTime);
        if (!running && !debugPulse) return;

        float t = useMusicSource && musicSource != null ? musicSource.time : (Time.time - startTime);
        if (useMusicSource && musicSource != null && loopTimeline && musicSource.loop)
        {
            if (t < lastMusicTime) { startTime = Time.time; }
            lastMusicTime = t;
        }
        if (!useMusicSource && loopTimeline)
        {
            if (timelineDuration > 0f) t = Mathf.Repeat(t, timelineDuration);
        }

        if (vignette != null && debugPulse)
        {
            float p = Mathf.Clamp01((Mathf.Sin((tGlobal / Mathf.Max(0.001f, debugPulsePeriod)) * Mathf.PI * 2f) * 0.5f) + 0.5f);
            float v = Mathf.Lerp(debugPulseMinVignette, debugPulseMaxVignette, p);
            vignette.intensity.Override(Mathf.Clamp01(v));
        }
        else if (vignette != null && vignetteCurve != null)
        {
            float v = vignetteCurve.Evaluate(t);
            vignette.intensity.Override(Mathf.Clamp01(v));
        }
        if (colorAdj != null && saturationCurve != null)
        {
            float s = saturationCurve.Evaluate(t);
            colorAdj.saturation.Override(s);
        }
        if (targetCamera != null && zoomCurve != null)
        {
            float z = zoomCurve.Evaluate(t);
            if (useOrthographicZoom) targetCamera.orthographicSize = Mathf.Max(0.01f, z); else targetCamera.fieldOfView = Mathf.Clamp(z, 1f, 179f);
        }
        if (particleTargets != null && particlesCurve != null)
        {
            float p = particlesCurve.Evaluate(t);
            for (int i = 0; i < particleTargets.Length; i++)
            {
                var ps = particleTargets[i];
                if (ps == null) continue;
                var em = ps.emission;
                em.rateOverTime = new ParticleSystem.MinMaxCurve(p);
            }
        }
    }

    private void EnsureURPPostProcessing(Camera cam)
    {
        var data = cam.GetComponent<UniversalAdditionalCameraData>();
        if (data != null)
        {
            data.renderPostProcessing = true;
            // Asegurar que vea el Volume global
            if (GlobalPostFX.Instance != null && GlobalPostFX.Instance.volume != null)
            {
                var layer = GlobalPostFX.Instance.volume.gameObject.layer;
                data.volumeLayerMask |= (1 << layer);
            }
        }
    }
}
