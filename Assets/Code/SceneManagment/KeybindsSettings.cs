using UnityEngine;
using TMPro;

public class KeybindsSettings : MonoBehaviour
{
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

    private float nextRefresh;

    private void OnEnable()
    {
        RefreshUI();
        nextRefresh = Time.unscaledTime + 0.2f;
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextRefresh)
        {
            RefreshUI();
            nextRefresh = Time.unscaledTime + 0.2f;
        }
    }

    public void RebindLeftPrimary() { InputBindingsStart("MoveLeft", 0); }
    public void RebindLeftSecondary() { InputBindingsStart("MoveLeft", 1); }
    public void RebindRightPrimary() { InputBindingsStart("MoveRight", 0); }
    public void RebindRightSecondary() { InputBindingsStart("MoveRight", 1); }
    public void RebindJumpPrimary() { InputBindingsStart("Jump", 0); }
    public void RebindJumpSecondary() { InputBindingsStart("Jump", 1); }
    public void RebindItemPrimary() { InputBindingsStart("UseItem", 0); }
    public void RebindItemSecondary() { InputBindingsStart("UseItem", 1); }
    public void RebindAttackPrimary() { InputBindingsStart("Action1Attack", 0); }
    public void RebindAttackSecondary() { InputBindingsStart("Action1Attack", 1); }
    public void RebindShieldPrimary() { InputBindingsStart("Action2Shield", 0); }
    public void RebindShieldSecondary() { InputBindingsStart("Action2Shield", 1); }

    private void InputBindingsStart(string accion, int slot)
    {
        var bindings = Object.FindFirstObjectByType<InputBindings>();
        if (bindings != null) bindings.StartRebind(accion, slot);
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

    public void ConfirmChanges()
    {
        InputBindings.SaveAll();
    }

    public void ResetToDefaults()
    {
        InputBindings.ResetToDefaults();
        RefreshUI();
    }
}
