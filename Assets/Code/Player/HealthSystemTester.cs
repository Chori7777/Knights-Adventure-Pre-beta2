using UnityEngine;
using TMPro;

/// <summary>
/// Script de testing para el sistema de vida.
/// ELIMINAR EN LA BUILD FINAL.
/// </summary>
public class HealthSystemTester : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private playerLife playerLifeScript;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Testing UI en pantalla")]
    [SerializeField] private bool showDebugUI = true;
    private string debugInfo = "";

    private void Start()
    {
        // Buscar jugador automáticamente si no está asignado
        if (playerLifeScript == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerLifeScript = player.GetComponent<playerLife>();
            }
        }

        if (playerLifeScript == null)
        {
            Debug.LogError("❌ [HealthSystemTester] No se encontró playerLife");
        }

        UpdateDebugInfo();
    }

    private void Update()
    {
        if (playerLifeScript == null) return;

        // ========== CONTROLES DE TESTING ==========

        // F1 - Quitar 1 de vida
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestDamage();
        }

        // F2 - Curar 1 de vida
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestHeal();
        }

        // F3 - Curación completa
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestFullHeal();
        }

        // F4 - AUMENTAR vida máxima (+1)
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TestIncreaseMaxHealth();
        }

        // F5 - DISMINUIR vida máxima (-1) [solo para testing]
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TestDecreaseMaxHealth();
        }

        // F6 - Matar jugador (testing muerte)
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestKill();
        }

        // F7 - Resetear a valores iniciales
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestReset();
        }

        UpdateDebugInfo();
    }

    // ========== MÉTODOS DE TESTING ==========

    private void TestDamage()
    {
        if (playerLifeScript.Health > 0)
        {
            playerLifeScript.TakeDamage(transform.position, 1);
            Debug.Log($"💔 Daño aplicado. Vida: {playerLifeScript.Health}/{playerLifeScript.MaxHealth}");
        }
        else
        {
            Debug.Log("💀 Jugador ya está muerto");
        }
    }

    private void TestHeal()
    {
        if (playerLifeScript.Health < playerLifeScript.MaxHealth)
        {
            playerLifeScript.Heal(1);
            Debug.Log($"💚 +1 vida. Vida: {playerLifeScript.Health}/{playerLifeScript.MaxHealth}");
        }
        else
        {
            Debug.Log("❤️ Vida ya está al máximo");
        }
    }

    private void TestFullHeal()
    {
        playerLifeScript.HealFull();
        Debug.Log($"✨ Curación completa. Vida: {playerLifeScript.Health}/{playerLifeScript.MaxHealth}");
    }

    private void TestIncreaseMaxHealth()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔧 [TEST] Aumentando vida máxima...");

        int oldMax = playerLifeScript.MaxHealth;
        int oldHealth = playerLifeScript.Health;

        // PASO 1: Aumentar vida máxima
        int newMax = oldMax + 1;
        playerLifeScript.SetMaxHealth(newMax);
        Debug.Log($"✅ [TEST] Vida máxima: {oldMax} → {newMax}");

        // PASO 2: Curar hasta el nuevo máximo
        playerLifeScript.SetHealth(newMax);
        Debug.Log($"✅ [TEST] Vida actual: {oldHealth} → {newMax}");

        // PASO 3: FORZAR actualización de UI (CRÍTICO)
        if (PlayerHealthUI.Instance != null)
        {
            Debug.Log("🔄 [TEST] Forzando actualización de UI...");
            PlayerHealthUI.Instance.ForceRefresh();
            Debug.Log("✅ [TEST] UI actualizada");
        }
        else
        {
            Debug.LogError("❌ [TEST] PlayerHealthUI.Instance es NULL");
        }

        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    private void TestDecreaseMaxHealth()
    {
        if (playerLifeScript.MaxHealth > 1)
        {
            int newMax = playerLifeScript.MaxHealth - 1;
            playerLifeScript.SetMaxHealth(newMax);
            Debug.Log($"⬇️ Vida máxima reducida. Ahora: {playerLifeScript.Health}/{playerLifeScript.MaxHealth}");
        }
        else
        {
            Debug.Log("⚠️ No se puede reducir más la vida máxima");
        }
    }

    private void TestKill()
    {
        playerLifeScript.TakeDamage(transform.position, playerLifeScript.MaxHealth);
        Debug.Log("☠️ Jugador eliminado");
    }

    private void TestReset()
    {
        playerLifeScript.SetMaxHealth(5);
        playerLifeScript.SetHealth(5);
        playerLifeScript.SetPotions(3);
        Debug.Log("🔄 Sistema de vida reseteado a valores iniciales (5/5 vida, 3 pociones)");
    }

    // ========== DEBUG UI ==========

    private void UpdateDebugInfo()
    {
        if (playerLifeScript == null) return;

        debugInfo = $"=== HEALTH SYSTEM TESTER ===\n\n";
        debugInfo += $"❤️ Vida: {playerLifeScript.Health}/{playerLifeScript.MaxHealth}\n";
        debugInfo += $"🧪 Pociones: {playerLifeScript.Potions}/{playerLifeScript.MaxPotions}\n\n";
        debugInfo += "--- CONTROLES ---\n";
        debugInfo += "F1 - Daño (-1 vida)\n";
        debugInfo += "F2 - Curar (+1 vida)\n";
        debugInfo += "F3 - Curación completa\n";
        debugInfo += "F4 - ⭐ AUMENTAR VIDA MÁXIMA (+1) ⭐\n";
        debugInfo += "F5 - Reducir vida máxima (-1)\n";
        debugInfo += "F6 - Matar jugador\n";
        debugInfo += "F7 - Reset a valores iniciales\n\n";
        debugInfo += "--- ESTADO ACTUAL ---\n";
        debugInfo += $"Vida máxima actual: {playerLifeScript.MaxHealth}\n";
        debugInfo += $"Aumentos realizados: {playerLifeScript.MaxHealth - 5}\n";

        if (debugText != null)
        {
            debugText.text = debugInfo;
        }
    }

    private void OnGUI()
    {
        if (!showDebugUI || playerLifeScript == null) return;

        // UI simple en pantalla
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 10, 10);

        GUI.Box(new Rect(10, 10, 300, 250), "");
        GUI.Label(new Rect(20, 20, 280, 230), debugInfo, style);
    }

    // ========== SIMULACIÓN DE MEJORA DE VIDA ==========

    /// <summary>
    /// Simula la compra de una mejora de vida (como en una tienda)
    /// </summary>
    public void SimulateHealthUpgrade()
    {
        // Aumentar vida máxima
        int newMax = playerLifeScript.MaxHealth + 1;
        playerLifeScript.SetMaxHealth(newMax);

        // Curar el nuevo punto de vida
        playerLifeScript.HealFull();

        Debug.Log($"💰 ¡Mejora comprada! Nueva vida máxima: {playerLifeScript.MaxHealth}");
    }

    // ========== VALIDACIÓN VISUAL ==========

    /// <summary>
    /// Ejecuta una secuencia de prueba automática
    /// </summary>
    [ContextMenu("Ejecutar Prueba Automática")]
    public void RunAutomatedTest()
    {
        StartCoroutine(AutomatedTestSequence());
    }

    private System.Collections.IEnumerator AutomatedTestSequence()
    {
        Debug.Log("🧪 === INICIANDO PRUEBA AUTOMÁTICA ===");

        // Reset inicial
        TestReset();
        yield return new WaitForSeconds(1f);

        // Probar daño progresivo
        Debug.Log("📉 Probando daño progresivo...");
        for (int i = 0; i < 5; i++)
        {
            TestDamage();
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);

        // Probar curación
        Debug.Log("📈 Probando curación...");
        for (int i = 0; i < 5; i++)
        {
            TestHeal();
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);

        // Probar aumento de vida máxima
        Debug.Log("⬆️ Probando aumento de vida máxima...");
        for (int i = 0; i < 3; i++)
        {
            TestIncreaseMaxHealth();
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);

        // Probar daño con vida extendida
        Debug.Log("📉 Probando daño con vida extendida...");
        for (int i = 0; i < 8; i++)
        {
            TestDamage();
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("✅ === PRUEBA AUTOMÁTICA COMPLETADA ===");
    }
}

// ========== INSTRUCCIONES DE USO ==========
//
// 1. Crea un GameObject vacío llamado "HealthTester"
// 2. Añade este script
// 3. (Opcional) Crea un TextMeshProUGUI y asígnalo a debugText
// 4. Dale play y usa las teclas F1-F7
//
// PARA PROBAR MEJORAS DE VIDA:
// - Presiona F4 varias veces y observa cómo crece la espada
// - Presiona F1 varias veces y verifica que se vacíe de arriba a abajo
//
// PARA PRUEBA AUTOMÁTICA:
// - Click derecho en el componente → "Ejecutar Prueba Automática"