
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
    [Header("Velocidad de Texto")]
    [SerializeField] private float typeSpeed = 0.05f;

    private static TextManager instance;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public static TextManager Instance => instance;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        closeButton.onClick.AddListener(CloseDialogue);
    }

    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);

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
    }

    public void SetTypeSpeed(float speed)
    {
        typeSpeed = Mathf.Max(0.001f, speed);
    }
}
