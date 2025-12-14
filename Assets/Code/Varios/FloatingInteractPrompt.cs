using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingInteractPrompt : MonoBehaviour
{
    [Header("Apariencia")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private string promptText = "[E]";
    [SerializeField] private string sortingLayerName = "";
    [SerializeField] private int sortingOrder = 2000;
    [SerializeField] private bool keepConstantWorldScale = true;

    [Header("Lógica de interacción")]
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool showOnlyWhenInRange = true;
    [SerializeField] private bool hideWhenUsed = false;

    private TextMeshPro tmp;
    private Transform player;
    private Vector3 originalScale;
    private bool used = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        var go = new GameObject("FloatingInteractText");
        go.transform.SetParent(transform, false);
        tmp = go.AddComponent<TextMeshPro>();
        tmp.color = textColor;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        if (fontAsset != null) tmp.font = fontAsset;
        var mr = tmp.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            if (!string.IsNullOrEmpty(sortingLayerName)) mr.sortingLayerName = sortingLayerName;
            mr.sortingOrder = sortingOrder;
        }
        UpdatePromptImmediate();
    }

    private void Update()
    {
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        bool inRange = true;
        if (showOnlyWhenInRange && player != null)
        {
            inRange = Vector3.Distance(transform.position, player.position) <= interactionDistance;
        }
        tmp.enabled = inRange && (!hideWhenUsed || !used);
        if (tmp.enabled) UpdatePromptImmediate();
    }

    private void LateUpdate()
    {
        if (tmp == null) return;
        tmp.transform.position = transform.position + worldOffset;
        if (keepConstantWorldScale)
        {
            float sx = transform.lossyScale.x;
            float sy = transform.lossyScale.y;
            if (Mathf.Abs(sx) < 1e-4f) sx = 1f;
            if (Mathf.Abs(sy) < 1e-4f) sy = 1f;
            float signX = sx < 0f ? -1f : 1f;
            tmp.transform.localScale = new Vector3(signX / Mathf.Abs(sx), 1f / Mathf.Abs(sy), 1f);
        }
    }

    private void UpdatePromptImmediate()
    {
        tmp.text = promptText;
    }

    public void MarkUsed()
    {
        used = true;
        if (hideWhenUsed && tmp != null) tmp.enabled = false;
    }

    public void SetPromptKey(KeyCode key)
    {
        interactKey = key;
        promptText = $"[{key}]";
        UpdatePromptImmediate();
    }
}
