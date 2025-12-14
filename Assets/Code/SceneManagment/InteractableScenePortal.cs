using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractableScenePortal : MonoBehaviour
{
    [Header("Interacción")]
    [SerializeField] public float interactionDistance = 2f;
    [SerializeField] public KeyCode interactKey = KeyCode.E;
    [SerializeField] public bool requirePlayerInRange = true;
    [SerializeField] public bool cooldownAfterUse = true;
    [SerializeField] public float cooldownDuration = 1f;

    [Header("Cambio de escena")]
    [SerializeField] public string targetSceneName = "";
    [SerializeField] public bool useFade = true;
    [SerializeField] public bool showWindowsDialogOnInteract = false;
    [SerializeField] public string windowsDialogMessage = "¿Ir al final del juego?";
    [SerializeField] public bool goToLastSceneInBuild = true;

    [Header("Transformaciones al interactuar")]
    [SerializeField] public bool enlargeOnInteract = true;
    [SerializeField] public float enlargeMultiplier = 1.2f;
    [SerializeField] public bool changeLayerOnInteract = false;
    [SerializeField] public string targetLayerName = "";

    private Transform player;
    private bool used = false;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (used) return;
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        if (requirePlayerInRange)
        {
            if (player == null) return;
            float d = Vector3.Distance(transform.position, player.position);
            if (d > interactionDistance) return;
        }
        if (Input.GetKeyDown(interactKey))
        {
            StartCoroutine(HandleInteraction());
        }
    }

    private IEnumerator HandleInteraction()
    {
        used = true;
        if (enlargeOnInteract)
        {
            transform.localScale = originalScale * Mathf.Max(0.01f, enlargeMultiplier);
        }
        if (changeLayerOnInteract && !string.IsNullOrEmpty(targetLayerName))
        {
            int layer = LayerMask.NameToLayer(targetLayerName);
            if (layer >= 0) gameObject.layer = layer;
        }

        if (useFade && FadeController.Instance != null)
        {
            FadeController.Instance.ActivarFadeOut();
            yield return new WaitForSecondsRealtime(1f);
        }

        if (showWindowsDialogOnInteract)
        {
            yield return ShowWindowsDialogAndWait();
        }

        if (goToLastSceneInBuild)
        {
            int lastIndex = Mathf.Max(0, SceneManager.sceneCountInBuildSettings - 1);
            if (FadeController.Instance != null && useFade)
            {
                SceneManager.LoadScene(lastIndex);
            }
            else
            {
                SceneManager.LoadScene(lastIndex);
            }
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (FadeController.Instance != null && useFade)
            {
                FadeController.Instance.CambiarEscenaConFade(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }

        if (cooldownAfterUse)
        {
            yield return new WaitForSeconds(cooldownDuration);
            used = false;
            transform.localScale = originalScale;
        }
    }

    private IEnumerator ShowWindowsDialogAndWait()
    {
        GameObject canvasGO = new GameObject("WindowsDialogCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("WindowsDialogPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(420f, 160f);
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.92f, 0.92f, 0.94f, 1f);

        GameObject textGO = new GameObject("Message");
        textGO.transform.SetParent(panelGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.7f);
        textRT.anchorMax = new Vector2(0.5f, 0.7f);
        textRT.anchoredPosition = new Vector2(0f, 0f);
        textRT.sizeDelta = new Vector2(380f, 60f);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = windowsDialogMessage;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        GameObject okGO = new GameObject("OKButton");
        okGO.transform.SetParent(panelGO.transform, false);
        var okRT = okGO.AddComponent<RectTransform>();
        okRT.anchorMin = new Vector2(0.5f, 0.3f);
        okRT.anchorMax = new Vector2(0.5f, 0.3f);
        okRT.anchoredPosition = new Vector2(0f, -20f);
        okRT.sizeDelta = new Vector2(140f, 40f);
        var okImg = okGO.AddComponent<Image>();
        okImg.color = new Color(0.8f, 0.8f, 0.85f, 1f);
        var okBtn = okGO.AddComponent<Button>();
        var okTxtGO = new GameObject("OKText");
        okTxtGO.transform.SetParent(okGO.transform, false);
        var okTxt = okTxtGO.AddComponent<TextMeshProUGUI>();
        var okTxtRT = okTxtGO.GetComponent<RectTransform>();
        okTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
        okTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
        okTxtRT.anchoredPosition = Vector2.zero;
        okTxtRT.sizeDelta = new Vector2(120f, 30f);
        okTxt.text = "OK";
        okTxt.fontSize = 20;
        okTxt.alignment = TextAlignmentOptions.Center;
        okTxt.color = Color.black;

        bool confirmed = false;
        okBtn.onClick.AddListener(() => { confirmed = true; });

        while (!confirmed)
        {
            yield return null;
        }
        Destroy(canvasGO);
        yield return null;
    }
}
