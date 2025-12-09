using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuSelectionStyling : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Graphic target;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float selectedScale = 1f;
    [SerializeField] private float tweenDuration = 0.15f;

    [SerializeField] private bool pulse;
    [SerializeField] private float pulseDuration = 1.5f;
    [SerializeField] private float pulseScale = 1.05f;

    [SerializeField] private bool rotate;
    [SerializeField] private float rotateAmount = 2f;
    [SerializeField] private float rotateDuration = 3f;

    [SerializeField] private bool shake;
    [SerializeField] private float shakeDuration = 0.6f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    private Coroutine tweenRoutine;
    private Coroutine pulseRoutine;
    private Coroutine rotateRoutine;
    private Coroutine shakeRoutine;
    private RectTransform rt;
    private Vector3 baseScale;
    private Vector3 baseEuler;
    private Vector2 baseAnchored;
    
    private bool isSelected = false;
    private bool isHovered = false;

    public void Configure(Graphic g, Color cNormal, Color cSelected, float sNormal, float sSelected, float duration)
    {
        target = g;
        normalColor = cNormal;
        selectedColor = cSelected;
        normalScale = sNormal;
        selectedScale = sSelected;
        tweenDuration = duration;
        ApplyImmediate(false);
    }

    private void Awake()
    {
        rt = transform as RectTransform;
        baseScale = transform.localScale;
        baseEuler = transform.localEulerAngles;
        baseAnchored = rt != null ? rt.anchoredPosition : Vector2.zero;
        ApplyImmediate(false);
    }

    private void OnDisable()
    {
        StopAll();
        ResetTransforms();
        isSelected = false;
        isHovered = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        UpdateState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateState();
    }

    private void UpdateState()
    {
        bool active = isSelected || isHovered;

        byte mode = 0;
        if (isSelected) mode = 2;
        else if (isHovered) mode = 1;

        StartTween(mode);

        if (active)
            StartEffects();
        else
        {
            StopAll();
            ResetTransforms();
        }
    }

    private void StartTween(byte mode)
    {
        if (tweenRoutine != null) StopCoroutine(tweenRoutine);
        tweenRoutine = StartCoroutine(Tween(mode));
    }

    private IEnumerator Tween(byte mode)
    {
        float t = 0f;
        Color c0 = target != null ? target.color : Color.white;
        Color c1 = normalColor;
        if (mode == 1) c1 = hoverColor;
        else if (mode == 2) c1 = selectedColor;
        Vector3 s0 = transform.localScale;
        Vector3 s1 = baseScale * ((mode == 0) ? normalScale : selectedScale);
        while (t < tweenDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / tweenDuration);
            if (target != null) target.color = Color.Lerp(c0, c1, k);
            transform.localScale = Vector3.Lerp(s0, s1, k);
            yield return null;
        }
        if (target != null) target.color = c1;
        transform.localScale = s1;
    }

    private void StartEffects()
    {
        if (pulse)
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseLoop());
        }
        if (rotate)
        {
            if (rotateRoutine != null) StopCoroutine(rotateRoutine);
            rotateRoutine = StartCoroutine(RotateLoop());
        }
        if (shake)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeLoop());
        }
    }

    private IEnumerator PulseLoop()
    {
        float half = Mathf.Max(0.01f, pulseDuration * 0.5f);
        Vector3 sBase = baseScale * selectedScale;
        Vector3 sUp = sBase * pulseScale;
        while (true)
        {
            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                transform.localScale = Vector3.Lerp(sBase, sUp, k);
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                transform.localScale = Vector3.Lerp(sUp, sBase, k);
                yield return null;
            }
        }
    }

    private IEnumerator RotateLoop()
    {
        float dur = Mathf.Max(0.01f, rotateDuration);
        float half = dur * 0.5f;
        Vector3 eBase = baseEuler;
        Vector3 eUp = eBase + new Vector3(0f, 0f, rotateAmount);
        while (true)
        {
            float t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                transform.localEulerAngles = Vector3.Lerp(eBase, eUp, k);
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                transform.localEulerAngles = Vector3.Lerp(eUp, eBase, k);
                yield return null;
            }
        }
    }

    private IEnumerator ShakeLoop()
    {
        if (rt == null) yield break;
        float dur = Mathf.Max(0.01f, shakeDuration);
        float timer = 0f;
        while (true)
        {
            timer += Time.unscaledDeltaTime;
            float f = Mathf.PingPong(timer * shakeVibrato, 1f);
            float angle = Random.Range(-shakeRandomness, shakeRandomness) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (shakeStrength * f * 0.01f);
            rt.anchoredPosition = baseAnchored + offset;
            if (timer >= dur)
            {
                timer = 0f;
                rt.anchoredPosition = baseAnchored;
            }
            yield return null;
        }
    }

    private void StopAll()
    {
        if (tweenRoutine != null) StopCoroutine(tweenRoutine);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        if (rotateRoutine != null) StopCoroutine(rotateRoutine);
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        tweenRoutine = null;
        pulseRoutine = null;
        rotateRoutine = null;
        shakeRoutine = null;
    }

    private void ResetTransforms()
    {
        transform.localScale = baseScale * normalScale;
        transform.localEulerAngles = baseEuler;
        if (rt != null) rt.anchoredPosition = baseAnchored;
        if (target != null) target.color = normalColor;
    }

    private void ApplyImmediate(bool selected)
    {
        if (target != null) target.color = selected ? selectedColor : normalColor;
        transform.localScale = baseScale * (selected ? selectedScale : normalScale);
        transform.localEulerAngles = baseEuler;
        if (rt != null) rt.anchoredPosition = baseAnchored;
    }
}
