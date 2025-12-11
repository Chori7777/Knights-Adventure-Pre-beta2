using UnityEngine;
using System.Collections;

public class TrueFinalBossVisualEffects : MonoBehaviour
{
    [SerializeField] private CameraEffectsController cameraFX;
    [SerializeField] private int redPresetIndex = 0;

    public void ActivateRedBackground()
    {
        if (cameraFX != null) cameraFX.ApplyPresetIndex(redPresetIndex);
    }

    public void PlayPreset(int presetIndex)
    {
        if (cameraFX != null) cameraFX.ApplyPresetIndex(presetIndex);
    }

    public void ResetEffects(float duration)
    {
        if (cameraFX != null) cameraFX.ResetEffects(duration);
    }

    public void ShakeCamera(float duration, float intensity)
    {
        StartCoroutine(ShakeRoutine(duration, intensity));
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
}

