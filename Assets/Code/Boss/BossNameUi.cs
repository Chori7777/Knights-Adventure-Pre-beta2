using System.Collections;
using TMPro;
using UnityEngine;

public class BossNameUI : MonoBehaviour
{
    public static BossNameUI Instance;
    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI nombreTexto;
    [SerializeField] private AudioClip bossIntroSound; 

    [Header("Duración")]
    [SerializeField] private float fadeInTime = 0.8f;
    [SerializeField] private float visibleTime = 1.5f;
    [SerializeField] private float fadeOutTime = 0.8f;

    private Coroutine currentRoutine;

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
            canvasGroup.alpha = 0f;
    }

    public void MostrarNombre(string nombre)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(MostrarNombreRutina(nombre));
    }

    private IEnumerator MostrarNombreRutina(string nombre)
    {
        if (nombreTexto != null)
            nombreTexto.text = nombre;

        if (bossIntroSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(bossIntroSound, 0.5f, 1f);
        }

        // Fade In
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }

        yield return new WaitForSeconds(visibleTime);

        // Fade Out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        currentRoutine = null;
    }
}
