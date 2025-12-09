using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aplica automáticamente efectos de selección a TODOS los selectables
/// hijos de este GameObject. Coloca esto en el panel padre (ej: Options Menu Root).
/// </summary>
public class AutoSelectableStyling : MonoBehaviour
{
    [Header("═══════════════════════════════")]
    [Header("ESTILOS GLOBALES")]
    [Header("═══════════════════════════════")]

    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSeleccionado = new Color(1f, 0.9f, 0.2f); // Amarillo
    [SerializeField] private float escalaNormal = 1f;
    [SerializeField] private float escalaSeleccionado = 1.1f;
    [SerializeField] private float duracionTween = 0.15f;

    [Header("═══════════════════════════════")]
    [Header("EFECTOS OPCIONALES")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool usarPulso = false;
    [SerializeField] private float pulseDuration = 1.5f;
    [SerializeField] private float pulseScale = 1.05f;

    [SerializeField] private bool usarRotacion = false;
    [SerializeField] private float rotateAmount = 2f;
    [SerializeField] private float rotateDuration = 3f;

    [SerializeField] private bool usarShake = false;
    [SerializeField] private float shakeDuration = 0.6f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    [Header("═══════════════════════════════")]
    [Header("CONFIGURACIÓN")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool aplicarEnAwake = true;
    [SerializeField] private bool aplicarEnStart = true;
    [SerializeField] private bool aplicarRecursivamente = true; // Incluir hijos de hijos

    [Header("═══════════════════════════════")]
    [Header("DEBUG")]
    [Header("═══════════════════════════════")]

    [SerializeField] private bool mostrarLogs = true;
    [SerializeField] private Transform targetRoot;

    private void Awake()
    {
        if (aplicarEnAwake)
        {
            AplicarEstilosATodos();
        }
    }

    private void Start()
    {
        if (aplicarEnStart && !aplicarEnAwake)
        {
            AplicarEstilosATodos();
        }
    }

    /// <summary>
    /// Aplica estilos a todos los Selectables encontrados
    /// </summary>
    [ContextMenu("Aplicar Estilos a Todos")]
    public void AplicarEstilosATodos()
    {
        Transform root = targetRoot != null ? targetRoot : transform;
        Selectable[] selectables;

        if (aplicarRecursivamente)
        {
            selectables = root.GetComponentsInChildren<Selectable>(true);
        }
        else
        {
            selectables = GetComponentsInDirectChildren(root);
        }

        if (selectables == null || selectables.Length == 0)
        {
            if (mostrarLogs)
                Debug.LogWarning($"⚠️ [AutoStyling] No se encontraron selectables en {(root != null ? root.gameObject.name : gameObject.name)}");
            return;
        }

        int contadorAplicados = 0;

        foreach (Selectable selectable in selectables)
        {
            if (selectable == null) continue;

            // ✅ Verificar si ya tiene el componente
            MenuSelectionStyling existente = selectable.GetComponent<MenuSelectionStyling>();

            if (existente != null)
            {
                if (mostrarLogs)
                    Debug.Log($"⏭️ [AutoStyling] {selectable.name} ya tiene MenuSelectionStyling");

                // Actualizar configuración existente
                ActualizarConfiguracion(existente);
                contadorAplicados++;
                continue;
            }

            // ✅ Añadir y configurar
            MenuSelectionStyling styling = selectable.gameObject.AddComponent<MenuSelectionStyling>();

            // Buscar el Graphic (Text, Image, etc.)
            Graphic graphic = selectable.GetComponent<Graphic>();
            if (graphic == null)
                graphic = selectable.GetComponentInChildren<Graphic>();

            if (graphic == null)
            {
                if (mostrarLogs)
                    Debug.LogWarning($"⚠️ [AutoStyling] {selectable.name} no tiene Graphic");
                continue;
            }

            // ✅ Configurar el componente mediante reflexión (acceso a campos privados)
            ConfigurarStyling(styling, graphic);

            contadorAplicados++;

            if (mostrarLogs)
                Debug.Log($"✅ [AutoStyling] Aplicado a: {selectable.name}");
        }

        if (mostrarLogs)
            Debug.Log($"🎨 [AutoStyling] Completado: {contadorAplicados}/{selectables.Length} selectables estilizados");
    }

    /// <summary>
    /// Configura el MenuSelectionStyling usando reflexión
    /// </summary>
    private void ConfigurarStyling(MenuSelectionStyling styling, Graphic graphic)
    {
        var type = typeof(MenuSelectionStyling);

        // Colores
        SetPrivateField(type, styling, "target", graphic);
        SetPrivateField(type, styling, "normalColor", colorNormal);
        SetPrivateField(type, styling, "selectedColor", colorSeleccionado);

        // Escala
        SetPrivateField(type, styling, "normalScale", escalaNormal);
        SetPrivateField(type, styling, "selectedScale", escalaSeleccionado);

        // Tween
        SetPrivateField(type, styling, "tweenDuration", duracionTween);

        // Efectos opcionales
        SetPrivateField(type, styling, "pulse", usarPulso);
        SetPrivateField(type, styling, "pulseDuration", pulseDuration);
        SetPrivateField(type, styling, "pulseScale", pulseScale);

        SetPrivateField(type, styling, "rotate", usarRotacion);
        SetPrivateField(type, styling, "rotateAmount", rotateAmount);
        SetPrivateField(type, styling, "rotateDuration", rotateDuration);

        SetPrivateField(type, styling, "shake", usarShake);
        SetPrivateField(type, styling, "shakeDuration", shakeDuration);
        SetPrivateField(type, styling, "shakeStrength", shakeStrength);
        SetPrivateField(type, styling, "shakeVibrato", shakeVibrato);
        SetPrivateField(type, styling, "shakeRandomness", shakeRandomness);
    }

    /// <summary>
    /// Actualiza la configuración de un MenuSelectionStyling existente
    /// </summary>
    private void ActualizarConfiguracion(MenuSelectionStyling styling)
    {
        var type = typeof(MenuSelectionStyling);

        // Colores
        SetPrivateField(type, styling, "normalColor", colorNormal);
        SetPrivateField(type, styling, "selectedColor", colorSeleccionado);

        // Escala
        SetPrivateField(type, styling, "normalScale", escalaNormal);
        SetPrivateField(type, styling, "selectedScale", escalaSeleccionado);

        // Tween
        SetPrivateField(type, styling, "tweenDuration", duracionTween);

        // Efectos
        SetPrivateField(type, styling, "pulse", usarPulso);
        SetPrivateField(type, styling, "pulseDuration", pulseDuration);
        SetPrivateField(type, styling, "pulseScale", pulseScale);

        SetPrivateField(type, styling, "rotate", usarRotacion);
        SetPrivateField(type, styling, "rotateAmount", rotateAmount);
        SetPrivateField(type, styling, "rotateDuration", rotateDuration);

        SetPrivateField(type, styling, "shake", usarShake);
        SetPrivateField(type, styling, "shakeDuration", shakeDuration);
        SetPrivateField(type, styling, "shakeStrength", shakeStrength);
        SetPrivateField(type, styling, "shakeVibrato", shakeVibrato);
        SetPrivateField(type, styling, "shakeRandomness", shakeRandomness);
    }

    /// <summary>
    /// Usa reflexión para setear campos privados
    /// </summary>
    private void SetPrivateField(System.Type type, object obj, string fieldName, object value)
    {
        var field = type.GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else if (mostrarLogs)
        {
            Debug.LogWarning($"⚠️ Campo '{fieldName}' no encontrado en MenuSelectionStyling");
        }
    }

    /// <summary>
    /// Obtiene selectables solo de hijos directos (no recursivo)
    /// </summary>
    private Selectable[] GetComponentsInDirectChildren(Transform root)
    {
        System.Collections.Generic.List<Selectable> result = new System.Collections.Generic.List<Selectable>();
        if (root == null) root = transform;
        foreach (Transform child in root)
        {
            Selectable selectable = child.GetComponent<Selectable>();
            if (selectable != null)
            {
                result.Add(selectable);
            }
        }

        return result.ToArray();
    }

    public void SetTargetRoot(Transform root)
    {
        targetRoot = root;
    }

    /// <summary>
    /// Remueve todos los MenuSelectionStyling de los hijos
    /// </summary>
    [ContextMenu("Remover Todos los Estilos")]
    public void RemoverTodosLosEstilos()
    {
        MenuSelectionStyling[] stylings = GetComponentsInChildren<MenuSelectionStyling>(true);

        int removidos = 0;
        foreach (var styling in stylings)
        {
            if (styling != null)
            {
                DestroyImmediate(styling);
                removidos++;
            }
        }

        if (mostrarLogs)
            Debug.Log($"🗑️ [AutoStyling] Removidos {removidos} estilos");
    }
}

// ═══════════════════════════════════════════════════════════════════
// INSTRUCCIONES DE USO
// ═══════════════════════════════════════════════════════════════════
// 
// 1. Añade este script al GameObject PADRE que contiene todos los botones/selectables
//    Ejemplo: "Options Menu Root", "Main Menu Panel", etc.
// 
// 2. Configura los colores y efectos en el Inspector
// 
// 3. OPCIÓN A - AUTOMÁTICO:
//    - Marca "Aplicar En Start" = TRUE
//    - Dale Play, y automáticamente aplicará estilos a todos
// 
// 4. OPCIÓN B - MANUAL:
//    - Click derecho en el script → "Aplicar Estilos a Todos"
//    - Útil para aplicar en el Editor sin dar Play
// 
// 5. Para REMOVER estilos:
//    - Click derecho → "Remover Todos los Estilos"
// 
// ═══════════════════════════════════════════════════════════════════
