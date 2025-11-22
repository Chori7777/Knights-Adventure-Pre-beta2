
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button closeButton;
    [SerializeField] private float typeSpeed = 0.05f;

    private static DialogueManager instance;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public static DialogueManager Instance => instance;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        closeButton.onClick.AddListener(CloseDialogue);
    }

    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);

        // Si hay una corrutina escribiendo,detener
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // Iniciamos la corrutina de escribir
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
}