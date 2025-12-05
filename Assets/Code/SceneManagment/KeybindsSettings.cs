using UnityEngine;
using TMPro;
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
        RefreshUI();
        StartCoroutine(RefreshUILoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isWaitingForKey = false;

        if (waitingMessagePanel != null)
            waitingMessagePanel.SetActive(false);
    }

    // ✅ Loop para actualizar UI constantemente (por si cambia mientras espera)
    private IEnumerator RefreshUILoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.2f);

            // Solo refrescar si NO está esperando una tecla
            if (!isWaitingForKey)
            {
                RefreshUI();
            }
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