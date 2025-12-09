using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class KeybindsSettings : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private TextMeshProUGUI leftPrimary;
    [SerializeField] private TextMeshProUGUI leftSecondary;
    [SerializeField] private TextMeshProUGUI rightPrimary;
    [SerializeField] private TextMeshProUGUI rightSecondary;
    [SerializeField] private TextMeshProUGUI jumpPrimary;
    [SerializeField] private TextMeshProUGUI jumpSecondary;
    [SerializeField] private TextMeshProUGUI itemPrimary;
    [SerializeField] private TextMeshProUGUI itemSecondary;
    [SerializeField] private TextMeshProUGUI attackPrimary;
    [SerializeField] private TextMeshProUGUI attackSecondary;
    [SerializeField] private TextMeshProUGUI shieldPrimary;
    [SerializeField] private TextMeshProUGUI shieldSecondary;

    [Header("Mensaje de espera")]
    [SerializeField] private TextMeshProUGUI waitingMessageText;
    [SerializeField] private GameObject waitingMessagePanel;

    private InputBindings inputBindingsComponent;
    private bool isWaitingForKey = false;

    private void Awake()
    {
        // Buscar InputBindings en la escena
        inputBindingsComponent = FindFirstObjectByType<InputBindings>();

        if (inputBindingsComponent == null)
        {
            Debug.LogWarning("⚠️ No se encontró InputBindings, creando uno...");
            GameObject go = new GameObject("InputBindings");
            inputBindingsComponent = go.AddComponent<InputBindings>();
        }

        // Ocultar mensaje de espera
        if (waitingMessagePanel != null)
            waitingMessagePanel.SetActive(false);
    }

    private void OnEnable()
    {
        Debug.Log("🎮 [Keybinds] Panel activado - Refrescando UI");
        EnsureInteractive();
        AutoWireButtons();
        RefreshUI();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isWaitingForKey = false;

        if (waitingMessagePanel != null)
            waitingMessagePanel.SetActive(false);
    }

    private void EnsureInteractive()
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        Debug.Log("🎮 [Keybinds] CanvasGroup interactivo");
    }

    private void AutoWireButtons()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            var n = b.gameObject.name.ToLowerInvariant();

            // Limpiar listeners previos sólo de rebind
            b.onClick.RemoveAllListeners();

            if (n.Contains("left1") || n == "left1") b.onClick.AddListener(() => StartRebind("MoveLeft", 0, "MOVER IZQUIERDA (Primario)"));
            else if (n.Contains("left2") || n == "left2") b.onClick.AddListener(() => StartRebind("MoveLeft", 1, "MOVER IZQUIERDA (Secundario)"));
            else if (n.Contains("right1") || n == "right1") b.onClick.AddListener(() => StartRebind("MoveRight", 0, "MOVER DERECHA (Primario)"));
            else if (n.Contains("right2") || n == "right2") b.onClick.AddListener(() => StartRebind("MoveRight", 1, "MOVER DERECHA (Secundario)"));
            else if (n.Contains("jump1") || n == "jump1") b.onClick.AddListener(() => StartRebind("Jump", 0, "SALTAR (Primario)"));
            else if (n.Contains("jump2") || n == "jump2") b.onClick.AddListener(() => StartRebind("Jump", 1, "SALTAR (Secundario)"));
            else if (n.Contains("attack1") || n == "attack1") b.onClick.AddListener(() => StartRebind("Action1Attack", 0, "ATACAR (Primario)"));
            else if (n.Contains("attack2") || n == "attack2") b.onClick.AddListener(() => StartRebind("Action1Attack", 1, "ATACAR (Secundario)"));
            else if (n.Contains("shield1") || n == "shield1" || n == "shiel1") b.onClick.AddListener(() => StartRebind("Action2Shield", 0, "ESCUDO (Primario)"));
            else if (n.Contains("shield2") || n == "shield2" || n == "shiel2") b.onClick.AddListener(() => StartRebind("Action2Shield", 1, "ESCUDO (Secundario)"));
            else if (n.Contains("potion1") || n == "potion1" || n.Contains("item1")) b.onClick.AddListener(() => StartRebind("UseItem", 0, "USAR ÍTEM (Primario)"));
            else if (n.Contains("potion2") || n == "potion2" || n.Contains("item2")) b.onClick.AddListener(() => StartRebind("UseItem", 1, "USAR ÍTEM (Secundario)"));
            else if (n.Contains("confirm")) b.onClick.AddListener(ConfirmChanges);
            else if (n.Contains("reset") || n.Contains("restart")) b.onClick.AddListener(ResetToDefaults);

            Debug.Log($"🎮 [Keybinds] Auto-wire botón: {b.gameObject.name}");
        }
    }


    private void RefreshUI()
    {
        SetPair(leftPrimary, leftSecondary, InputBindings.GameAction.MoveLeft);
        SetPair(rightPrimary, rightSecondary, InputBindings.GameAction.MoveRight);
        SetPair(jumpPrimary, jumpSecondary, InputBindings.GameAction.Jump);
        SetPair(itemPrimary, itemSecondary, InputBindings.GameAction.UseItem);
        SetPair(attackPrimary, attackSecondary, InputBindings.GameAction.Action1Attack);
        SetPair(shieldPrimary, shieldSecondary, InputBindings.GameAction.Action2Shield);
    }

    private void SetPair(TextMeshProUGUI p, TextMeshProUGUI s, InputBindings.GameAction a)
    {
        if (p != null) p.text = InputBindings.GetPrimary(a).ToString();
        if (s != null) s.text = InputBindings.GetSecondary(a).ToString();
    }

    // ✅ MÉTODOS PÚBLICOS PARA BOTONES (con feedback visual)
    public void RebindLeftPrimary() => StartRebind("MoveLeft", 0, "MOVER IZQUIERDA (Primario)");
    public void RebindLeftSecondary() => StartRebind("MoveLeft", 1, "MOVER IZQUIERDA (Secundario)");
    public void RebindRightPrimary() => StartRebind("MoveRight", 0, "MOVER DERECHA (Primario)");
    public void RebindRightSecondary() => StartRebind("MoveRight", 1, "MOVER DERECHA (Secundario)");
    public void RebindJumpPrimary() => StartRebind("Jump", 0, "SALTAR (Primario)");
    public void RebindJumpSecondary() => StartRebind("Jump", 1, "SALTAR (Secundario)");
    public void RebindItemPrimary() => StartRebind("UseItem", 0, "USAR ÍTEM (Primario)");
    public void RebindItemSecondary() => StartRebind("UseItem", 1, "USAR ÍTEM (Secundario)");
    public void RebindAttackPrimary() => StartRebind("Action1Attack", 0, "ATACAR (Primario)");
    public void RebindAttackSecondary() => StartRebind("Action1Attack", 1, "ATACAR (Secundario)");
    public void RebindShieldPrimary() => StartRebind("Action2Shield", 0, "ESCUDO (Primario)");
    public void RebindShieldSecondary() => StartRebind("Action2Shield", 1, "ESCUDO (Secundario)");

    private void StartRebind(string accion, int slot, string actionName)
    {
        Debug.Log($"🎯 [Keybinds] Iniciando rebind: {actionName} (slot {slot})");

        if (inputBindingsComponent == null)
        {
            Debug.LogError("❌ InputBindings es null");
            return;
        }

        isWaitingForKey = true;

        // Mostrar mensaje
        if (waitingMessagePanel != null)
        {
            waitingMessagePanel.SetActive(true);
            if (waitingMessageText != null)
            {
                waitingMessageText.text = $"Presiona una tecla para:\n<b>{actionName}</b>\n\n(ESC para cancelar)";
            }
        }

        // Iniciar escucha
        inputBindingsComponent.StartRebind(accion, slot);

        // Esperar a que termine
        StartCoroutine(WaitForRebindComplete());
    }

    private IEnumerator WaitForRebindComplete()
    {
        // Esperar hasta que InputBindings ya no esté escuchando
        float timeout = 10f; // 10 segundos máximo
        float elapsed = 0f;

        while (isWaitingForKey && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;

            // ESC para cancelar
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inputBindingsComponent != null)
                    inputBindingsComponent.CancelRebind();

                isWaitingForKey = false;
                break;
            }

            // Si InputBindings ya dejó de escuchar, terminar
            if (inputBindingsComponent != null && !inputBindingsComponent.IsListening)
            {
                isWaitingForKey = false;
                break;
            }

            // Verificar si se asignó una tecla (detectar cambio)
            // InputBindings debería cambiar su estado interno

            yield return null;
        }

        // Ocultar mensaje
        if (waitingMessagePanel != null)
            waitingMessagePanel.SetActive(false);

        isWaitingForKey = false;

        // Refrescar UI para mostrar nueva tecla
        yield return new WaitForSecondsRealtime(0.1f);
        RefreshUI();

        Debug.Log("✅ [Keybinds] Rebind completado");
    }

    public void ConfirmChanges()
    {
        InputBindings.SaveAll();
        Debug.Log("💾 Keybinds guardados");
    }

    public void ResetToDefaults()
    {
        InputBindings.ResetToDefaults();
        RefreshUI();
        Debug.Log("🔄 Keybinds reseteados a default");
    }
}
