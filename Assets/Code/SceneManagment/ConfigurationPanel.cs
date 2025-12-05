using UnityEngine;
using TMPro;

public class ConfigurationPanel : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject configurationRoot;
    [SerializeField] private GameObject preferencesPanel;
    [SerializeField] private GameObject keybindsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private MenuOverlayManager overlayManager;

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

        if (overlayManager != null) overlayManager.OpenOptions();
        if (configurationRoot != null) configurationRoot.SetActive(true);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        EnsureRaycaster();

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

        if (overlayManager != null) overlayManager.CloseOptions();
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
            if (rc == null) rc = canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            rc.enabled = true;
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

        // Fijar el MenuNavigator activo para el puntero compartido
        if (overlayManager != null && tabs != null && currentTabIndex >= 0 && currentTabIndex < tabs.Length)
        {
            var root = tabs[currentTabIndex];
            if (root != null)
            {
                var nav = root.GetComponent<MenuNavigator>();
                if (nav == null) nav = root.GetComponentInChildren<MenuNavigator>(true);
                if (nav != null) overlayManager.SetOptionsNavigator(nav);
            }
        }
    }
}
