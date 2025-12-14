
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TextManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button closeButton;
    [SerializeField] private bool overrideSorting = true;
    [SerializeField] private int sortingOrder = 1000;
    [SerializeField] private string sortingLayerName = "";
    [SerializeField] private bool attachToMainCamera = true;
    [Header("Velocidad de Texto")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private bool allowSkipTyping = true;
    [SerializeField] private float dotPause = 0.5f;
    [SerializeField] private float commaPause = 0.25f;
    [SerializeField] private float minDisplayTime = 1.0f;
    

    private static TextManager instance;
    private static bool isOpen;
    private Coroutine typingCoroutine;
    private string currentText;
    private Coroutine sequenceCoroutine;
    

    private void Awake()
    {
        if (instance == null)
            instance = this;
        typeSpeed = PlayerPrefs.GetFloat("pref_text_speed", typeSpeed);
    }

    public static TextManager Instance => instance;
    public static bool IsOpen => isOpen;

    public float GetTypeSpeed()
    {
        return typeSpeed;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(CloseOrSkip);
    }

    public void ShowDialogue(string text)
    {
        currentText = text;
        Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = dialoguePanel.AddComponent<Canvas>();
            dialoguePanel.AddComponent<GraphicRaycaster>();
        }
        if (overrideSorting)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                canvas.sortingLayerName = sortingLayerName;
            }
        }
        if (attachToMainCamera && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;
        }

        dialoguePanel.SetActive(true);
        isOpen = true;
        dialoguePanel.transform.SetAsLastSibling();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
        typingCoroutine = null;
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        isOpen = false;
    }

    private void CloseOrSkip()
    {
        if (typingCoroutine != null && allowSkipTyping)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            dialogueText.text = currentText;
            return;
        }
        CloseDialogue();
    }

    public void SetTypeSpeed(float speed)
    {
        typeSpeed = Mathf.Max(0.001f, speed);
    }

    public IEnumerator PlaySequenceAndWait(string[] lines)
    {
        if (lines == null || lines.Length == 0) yield break;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            ShowDialogue(line);
            int dotCount = 0;
            int commaCount = 0;
            for (int c = 0; c < line.Length; c++)
            {
                char ch = line[c];
                if (ch == '.') dotCount++;
                else if (ch == ',') commaCount++;
            }
            float baseTime = line.Length * typeSpeed;
            float punctuationTime = dotCount * dotPause + commaCount * commaPause;
            float displayTime = baseTime + punctuationTime;
            if (displayTime < minDisplayTime) displayTime = minDisplayTime;
            float elapsed = 0f;
            while (elapsed < displayTime)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        CloseDialogue();
    }
    
    
}
