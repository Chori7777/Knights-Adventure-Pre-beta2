using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlackScreenManager : MonoBehaviour
{
    public static BlackScreenManager Instance { get; private set; }

    [Header("Configuración Pantalla Negra")]
    [SerializeField] private Image blackScreenImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear automáticamente un Canvas + Image si no está asignado
        if (blackScreenImage == null)
        {
            var canvasGO = new GameObject("BlackScreenCanvas");
            DontDestroyOnLoad(canvasGO);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var imgGO = new GameObject("BlackScreenImage");
            imgGO.transform.SetParent(canvasGO.transform, false);
            var rt = imgGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            blackScreenImage = imgGO.AddComponent<Image>();
            blackScreenImage.color = new Color(0, 0, 0, 1);
        }

        // Asegurarse que la pantalla negra esté oculta al inicio
        blackScreenImage.gameObject.SetActive(false);
    }

    public void ShowBlackScreen()
    {
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.color = new Color(0, 0, 0, 1); // Negro sólido alfa 255
        }
    }

    public void HideBlackScreen()
    {
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(false);
        }
    }

    public IEnumerator ShowBlackScreenForDuration(float duration)
    {
        ShowBlackScreen();
        yield return new WaitForSeconds(duration);
        HideBlackScreen();
    }

    public IEnumerator ShowBlackScreenWithCallback(float duration, System.Action callback)
    {
        ShowBlackScreen();
        yield return new WaitForSeconds(duration);
        HideBlackScreen();
        callback?.Invoke();
    }
}
