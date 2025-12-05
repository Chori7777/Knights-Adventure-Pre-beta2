using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFloatingEffects : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Graphic targetGraphic;
    private RectTransform rt;

    [Header("Levitar")]
    [SerializeField] private bool levitate;
    [SerializeField] private Vector2 levitateAmplitude = new Vector2(0f, 8f);
    [SerializeField] private float levitatePeriod = 2.5f;

    [Header("Rotar")]
    [SerializeField] private bool rotate;
    [SerializeField] private float rotateAngle = 3f;
    [SerializeField] private float rotatePeriod = 3f;

    [Header("Vibrar")]
    [SerializeField] private bool vibrate;
    [SerializeField] private float vibrateStrength = 6f;
    [SerializeField] private int vibrateVibrato = 20;
    [SerializeField] private float vibrateRandomness = 90f;

    [Header("Escala/Pulso")]
    [SerializeField] private bool pulse;
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulsePeriod = 1.6f;

    private Vector2 baseAnchored;
    private Vector3 baseEuler;
    private Vector3 baseScale;

    private void Awake()
    {
        rt = transform as RectTransform;
        baseAnchored = rt != null ? rt.anchoredPosition : Vector2.zero;
        baseEuler = transform.localEulerAngles;
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void OnDisable()
    {
        ResetState();
    }

    private void ResetState()
    {
        if (rt != null) rt.anchoredPosition = baseAnchored;
        transform.localEulerAngles = baseEuler;
        transform.localScale = baseScale;
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        if (levitate && rt != null)
        {
            float wx = levitateAmplitude.x * Mathf.Sin((t / Mathf.Max(0.001f, levitatePeriod)) * Mathf.PI * 2f);
            float wy = levitateAmplitude.y * Mathf.Sin(((t + levitatePeriod * 0.25f) / Mathf.Max(0.001f, levitatePeriod)) * Mathf.PI * 2f);
            rt.anchoredPosition = baseAnchored + new Vector2(wx, wy);
        }

        if (rotate)
        {
            float ang = rotateAngle * Mathf.Sin((t / Mathf.Max(0.001f, rotatePeriod)) * Mathf.PI * 2f);
            transform.localEulerAngles = baseEuler + new Vector3(0f, 0f, ang);
        }

        if (pulse)
        {
            float half = Mathf.Max(0.01f, pulsePeriod * 0.5f);
            float phase = Mathf.PingPong(t, half) / half;
            float s = Mathf.Lerp(1f, pulseScale, phase);
            transform.localScale = baseScale * s;
        }

        if (vibrate && rt != null)
        {
            float f = Mathf.PingPong(t * vibrateVibrato, 1f);
            float angle = Random.Range(-vibrateRandomness, vibrateRandomness) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (vibrateStrength * f * 0.01f);
            rt.anchoredPosition = baseAnchored + offset;
        }
    }
}

