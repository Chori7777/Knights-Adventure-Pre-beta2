using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownIndicator : MonoBehaviour
{
    [Header("Configuración de Sprite")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite cooldownSprite;

    [Header("Configuración Visual")]
    [SerializeField] private Color cooldownTint = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color normalTint = Color.white;

    private Coroutine currentCooldown;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null && normalSprite == null)
        {
            normalSprite = targetImage.sprite;
        }
    }

    /// <summary>
    /// Inicia el cooldown visual
    /// </summary>
    /// <param name="duration">Duración del cooldown en segundos</param>
    public void StartCooldown(float duration)
    {
        if (currentCooldown != null)
        {
            StopCoroutine(currentCooldown);
        }

        currentCooldown = StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        // Cambiar a sprite/color de cooldown
        if (targetImage != null)
        {
            if (cooldownSprite != null)
            {
                targetImage.sprite = cooldownSprite;
            }
            targetImage.color = cooldownTint;
        }

        // Esperar la duración del cooldown
        yield return new WaitForSeconds(duration);

        // Restaurar sprite/color normal
        if (targetImage != null)
        {
            if (normalSprite != null)
            {
                targetImage.sprite = normalSprite;
            }
            targetImage.color = normalTint;
        }

        currentCooldown = null;
    }

    /// <summary>
    /// Cancela el cooldown actual y restaura el estado normal
    /// </summary>
    public void CancelCooldown()
    {
        if (currentCooldown != null)
        {
            StopCoroutine(currentCooldown);
            currentCooldown = null;
        }

        if (targetImage != null)
        {
            if (normalSprite != null)
            {
                targetImage.sprite = normalSprite;
            }
            targetImage.color = normalTint;
        }
    }

    /// <summary>
    /// Verifica si está en cooldown actualmente
    /// </summary>
    public bool IsInCooldown()
    {
        return currentCooldown != null;
    }
}