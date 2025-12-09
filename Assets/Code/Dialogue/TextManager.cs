
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
    

    private static TextManager instance;
    private static bool isOpen;
    private Coroutine typingCoroutine;
    

    private void Awake()
    {
        if (instance == null)
            instance = this;
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
        closeButton.onClick.AddListener(CloseDialogue);
    }

    public void ShowDialogue(string text)
    {
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
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        isOpen = false;
    }

    public void SetTypeSpeed(float speed)
    {
        typeSpeed = Mathf.Max(0.001f, speed);
    }

    
}
