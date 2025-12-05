using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ConfigurationPanel : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject configurationRoot;
    [SerializeField] private GameObject preferencesPanel;
    [SerializeField] private GameObject keybindsPanel;
    [SerializeField] private GameObject audioPanel;

    [Header("Tab Navigation Keys")]
    [SerializeField] private KeyCode prevTabKey = KeyCode.Q;
    [SerializeField] private KeyCode nextTabKey = KeyCode.E;
    [SerializeField] private TextMeshProUGUI currentTabLabel;
 

    private int currentTabIndex = 0;
    private GameObject[] tabs;
    private bool isInitialized = false;

    // ✅ ARREGLO CRÍTICO: Awake para inicializar tabs ANTES de que se active
    private void Awake()
    {
        InitializeTabs();
    }

    private void InitializeTabs()
    {
        tabs = new GameObject[] { preferencesPanel, keybindsPanel, audioPanel };

        // ✅ CRÍTICO: Desactivar TODOS los tabs al inicio
        foreach (var tab in tabs)
        {
            if (tab != null)
            {
                tab.SetActive(false);
                Debug.Log($"🔧 [Init] Tab desactivado: {tab.name}");
            }
        }

        isInitialized = true;
        Debug.Log($"✅ [ConfigPanel] Tabs inicializados: {tabs.Length}");
    }

    public void Open()
    {
        Debug.Log("🔓 [ConfigPanel] === ABRIENDO OPTIONS ===");

        // Verificar que tabs esté inicializado
        if (!isInitialized || tabs == null)
        {
            Debug.LogWarning("⚠️ Tabs no estaban inicializados, inicializando ahora...");
            InitializeTabs();
        }

        if (configurationRoot != null) configurationRoot.SetActive(true);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        EnsureEventSystem();
        EnsureRaycaster();
        EnsureButtonsInteractive();

        // ✅ ARREGLO: Resetear a la primera pestaña SIEMPRE
        currentTabIndex = 0;

        // ✅ ARREGLO: FORZAR desactivación de todos antes de mostrar
        foreach (var tab in tabs)
        {
            if (tab != null) tab.SetActive(false);
        }

        // Ahora sí, mostrar Preferencias
        ShowPreferences();
        FixVisibility();

        Debug.Log($"✅ [ConfigPanel] Abierto correctamente en tab {currentTabIndex}");
    }

    public void Close()
    {
        Debug.Log("🔒 [ConfigPanel] Cerrando...");

        if (configurationRoot != null) configurationRoot.SetActive(false);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);

        // ✅ Al cerrar, desactivar todos los tabs
        if (tabs != null)
        {
            foreach (var tab in tabs)
            {
                if (tab != null) tab.SetActive(false);
            }
        }
    }

    private void EnsureRaycaster()
    {
        if (configurationRoot == null) return;
        var canvas = configurationRoot.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var rc = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (rc == null)
            {
                rc = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log($"[OptionsUI] GraphicRaycaster agregado a canvas {canvas.gameObject.name}");
            }
            rc.enabled = true;
            Debug.Log($"[OptionsUI] GraphicRaycaster habilitado en canvas {canvas.gameObject.name}");
        }
    }

    private void EnsureEventSystem()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[OptionsUI] EventSystem creado con StandaloneInputModule");
        }
        else
        {
            var module = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (module == null)
            {
                es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[OptionsUI] StandaloneInputModule agregado al EventSystem existente");
            }
            es.sendNavigationEvents = false;
            Debug.Log("[OptionsUI] Navegación por teclado desactivada (solo mouse)");
        }
    }

    private void ShowCurrentTab()
    {
        if (tabs == null || tabs.Length == 0)
        {
            Debug.LogError("❌ [ConfigPanel] No hay tabs para mostrar");
            return;
        }

        Debug.Log($"📂 [ConfigPanel] === CAMBIANDO A TAB {currentTabIndex} ===");

        // ✅ PASO 1: Desactivar TODOS primero (evita superposición)
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] != null)
            {
                tabs[i].SetActive(false);
                Debug.Log($"  ❌ Tab {i} ({tabs[i].name}) desactivado");
            }
        }

        // ✅ PASO 2: Activar solo el actual
        if (currentTabIndex >= 0 && currentTabIndex < tabs.Length && tabs[currentTabIndex] != null)
        {
            tabs[currentTabIndex].SetActive(true);
            Debug.Log($"  ✅ Tab {currentTabIndex} ({tabs[currentTabIndex].name}) ACTIVADO");
            if (currentTabLabel != null) currentTabLabel.text = tabs[currentTabIndex].name;
            if (audioPanel != null && tabs[currentTabIndex] == audioPanel) EnsureAudioInteractive();
        }
        else
        {
            Debug.LogError($"❌ Índice inválido: {currentTabIndex}");
        }
    }

    private void Update()
    {
        // Solo procesar si el panel está activo
        if (configurationRoot == null || !configurationRoot.activeSelf)
            return;

        // Verificar inicialización
        if (tabs == null || tabs.Length == 0)
        {
            Debug.LogWarning("⚠️ Tabs no inicializado en Update");
            InitializeTabs();
            return;
        }

        // Q = anterior
        if (Input.GetKeyDown(prevTabKey))
        {
            Debug.Log($"⬅️ [Q] Presionado - Tab actual: {currentTabIndex}");
            PrevTab();
        }

        // E = siguiente
        if (Input.GetKeyDown(nextTabKey))
        {
            Debug.Log($"➡️ [E] Presionado - Tab actual: {currentTabIndex}");
            NextTab();
        }

        // Flechas izquierda/derecha para cambiar de tab cuando no hay control horizontal seleccionado
        var es = UnityEngine.EventSystems.EventSystem.current;
        var go = es != null ? es.currentSelectedGameObject : null;
        bool hasHorizontalControl = false;
        if (go != null)
        {
            if (go.GetComponent<UnityEngine.UI.Slider>() != null) hasHorizontalControl = true;
            if (go.GetComponent<TMPro.TMP_Dropdown>() != null) hasHorizontalControl = true;
            if (go.GetComponent<UnityEngine.UI.Toggle>() != null) hasHorizontalControl = true;
        }
        if (!hasHorizontalControl)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) NextTab();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) PrevTab();
        }
    }

    public void PrevTab()
    {
        if (tabs == null || tabs.Length == 0) return;

        int oldIndex = currentTabIndex;
        currentTabIndex--;

        if (currentTabIndex < 0)
            currentTabIndex = tabs.Length - 1;

        Debug.Log($"⬅️ Tab anterior: {oldIndex} → {currentTabIndex}");
        ShowCurrentTab();
        FixVisibility();
    }

    public void NextTab()
    {
        if (tabs == null || tabs.Length == 0) return;

        int oldIndex = currentTabIndex;
        currentTabIndex++;

        if (currentTabIndex >= tabs.Length)
            currentTabIndex = 0;

        Debug.Log($"➡️ Tab siguiente: {oldIndex} → {currentTabIndex}");
        ShowCurrentTab();
        FixVisibility();
    }

    // Métodos para botones UI
    public void ShowPreferences()
    {
        currentTabIndex = 0;
        ShowCurrentTab();
    }

    public void ShowKeybinds()
    {
        currentTabIndex = 1;
        ShowCurrentTab();
    }

    public void ShowAudio()
    {
        currentTabIndex = 2;
        ShowCurrentTab();
    }

    public void ConfirmChanges()
    {
        Debug.Log("💾 Confirmando cambios...");

        var pref = preferencesPanel?.GetComponent<PreferenceSettings>();
        if (pref != null) pref.ApplyAll();

        var keyb = keybindsPanel?.GetComponent<KeybindsSettings>();
        if (keyb != null) keyb.ConfirmChanges();

        Close();
    }

    public void RestartValues()
    {
        Debug.Log("🔄 Reseteando valores...");

        var pref = preferencesPanel?.GetComponent<PreferenceSettings>();
        if (pref != null) pref.ResetToDefaults();

        var keyb = keybindsPanel?.GetComponent<KeybindsSettings>();
        if (keyb != null) keyb.ResetToDefaults();
    }

    private void FixVisibility()
    {
        if (tabs == null) return;
        for (int i = 0; i < tabs.Length; i++)
        {
            var tab = tabs[i];
            if (tab == null) continue;
            var cg = tab.GetComponent<CanvasGroup>();
            if (cg == null) cg = tab.AddComponent<CanvasGroup>();
            bool active = (i == currentTabIndex);
            cg.alpha = active ? 1f : 0f;
            cg.interactable = active;
            cg.blocksRaycasts = active;
            tab.SetActive(active);
        }

        if (currentTabLabel != null && tabs != null && currentTabIndex >= 0 && currentTabIndex < tabs.Length)
        {
            var go = tabs[currentTabIndex];
            currentTabLabel.text = go != null ? go.name : string.Empty;
        }

        
    }

    private void EnsureAudioInteractive()
    {
        var canvas = configurationRoot != null ? configurationRoot.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>() : null;
        if (canvas == null && configurationRoot != null)
        {
            var c = configurationRoot.GetComponentInParent<Canvas>();
            if (c != null)
            {
                var rc = c.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (rc == null) rc = c.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                rc.enabled = true;
                Debug.Log($"[OptionsUI] Audio: Raycaster habilitado en canvas {c.gameObject.name}");
            }
        }

        var cg = audioPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = audioPanel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        Debug.Log("[OptionsUI] Audio: CanvasGroup activo e interactivo");

        var sliders = audioPanel.GetComponentsInChildren<UnityEngine.UI.Slider>(true);
        Debug.Log($"[OptionsUI] Audio: {sliders.Length} sliders detectados");
        for (int i = 0; i < sliders.Length; i++)
        {
            var s = sliders[i];
            s.interactable = true;
            var h = s.handleRect;
            if (h != null)
            {
                var g = h.GetComponent<UnityEngine.UI.Graphic>();
                if (g != null) g.raycastTarget = true;
                Debug.Log($"[OptionsUI] Slider '{s.gameObject.name}': handle raycastTarget ON");
            }
            var bg = s.transform.Find("Background");
            if (bg != null)
            {
                var gb = bg.GetComponent<UnityEngine.UI.Graphic>();
                if (gb != null) gb.raycastTarget = true;
                Debug.Log($"[OptionsUI] Slider '{s.gameObject.name}': background raycastTarget ON");
            }
            else
            {
                Debug.LogWarning($"[OptionsUI] Slider '{s.gameObject.name}': Background no encontrado");
            }

            LogRaycastAt(s.GetComponent<RectTransform>(), "Slider");
        }

        var buttons = audioPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
            var img = buttons[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
            Debug.Log($"[OptionsUI] Audio: botón '{buttons[i].gameObject.name}' interactivo (listeners={buttons[i].onClick.GetPersistentEventCount()})");
        }

        var bgRoot = configurationRoot != null ? configurationRoot.transform.Find("Background") : null;
        if (bgRoot != null)
        {
            var bgImg = bgRoot.GetComponent<UnityEngine.UI.Image>();
            if (bgImg != null)
            {
                bgImg.raycastTarget = false;
                Debug.Log("[OptionsUI] Fondo de Options Interfaz deja de bloquear raycasts");
            }
        }

        LogRaycastAt(audioPanel.GetComponent<RectTransform>(), "Audio_Panel");
    }

    private void EnsureButtonsInteractive()
    {
        if (configurationRoot == null) return;
        var buttons = configurationRoot.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            b.interactable = true;
            var img = b.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
            Debug.Log($"[OptionsUI] Botón '{b.gameObject.name}' habilitado (listeners={b.onClick.GetPersistentEventCount()})");
        }

        var toggles = configurationRoot.GetComponentsInChildren<UnityEngine.UI.Toggle>(true);
        for (int i = 0; i < toggles.Length; i++)
        {
            var t = toggles[i];
            t.interactable = true;
            var img = t.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
            Debug.Log($"[OptionsUI] Toggle '{t.gameObject.name}' habilitado");
        }
    }

    private void LogRaycastAt(RectTransform rt, string context)
    {
        if (rt == null) return;
        var es = EventSystem.current;
        if (es == null) return;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        var ped = new PointerEventData(es);
        ped.position = screenPos;
        var results = new List<RaycastResult>();
        es.RaycastAll(ped, results);
        string log = "[OptionsUI] Raycast " + context + " -> ";
        for (int i = 0; i < results.Count && i < 5; i++)
        {
            log += results[i].gameObject.name + (i < results.Count - 1 ? ", " : "");
        }
        Debug.Log(log);
    }
}
