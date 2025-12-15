using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ConfigurationPanel : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject configurationRoot;
    [SerializeField] private GameObject preferencesPanel;
    [SerializeField] private GameObject keybindsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private bool overrideSorting = true;
    [SerializeField] private int sortingOrder = 2000;
    [SerializeField] private string sortingLayerName = "";
    [SerializeField] private bool attachToMainCamera = true;
    [Header("Audio UI (Labels opcionales)")]
    [SerializeField] private TextMeshProUGUI masterLabel;
    [SerializeField] private TextMeshProUGUI musicLabel;
    [SerializeField] private TextMeshProUGUI sfxLabel;

    [Header("Tab Navigation Keys")]
    [SerializeField] private KeyCode prevTabKey = KeyCode.Q;
    [SerializeField] private KeyCode nextTabKey = KeyCode.E;
    [SerializeField] private TextMeshProUGUI currentTabLabel;
    [SerializeField] private bool allowKeyboardTabSwitch = false;
    [Header("Auto Ocultacion")]

    private int currentTabIndex = 0;
    private GameObject[] tabs;
    private bool isInitialized = false;
    

    // ✅ ARREGLO CRÍTICO: Awake para inicializar tabs ANTES de que se active
    private void Awake()
    {
        InitializeTabs();
    }

    // Deshabilitado: evitar ocultar automáticamente el panel

    private void InitializeTabs()
    {
        tabs = new GameObject[] { preferencesPanel, keybindsPanel, audioPanel };

        // ✅ CRÍTICO: Desactivar TODOS los tabs al inicio
        foreach (var tab in tabs)
        {
            if (tab != null)
            {
                tab.SetActive(false);
                Debug.Log($"[ConfigPanel] Tab desactivado: {tab.name}");
            }
        }

        isInitialized = true;
        Debug.Log($"[ConfigPanel] Tabs inicializados: {tabs.Length}");
    }

    public void Open()
    {
        Debug.Log("[ConfigPanel] Abriendo opciones");

        // Verificar que tabs esté inicializado
        if (!isInitialized || tabs == null)
        {
            Debug.LogWarning("[ConfigPanel] Tabs no estaban inicializados, inicializando ahora");
            InitializeTabs();
        }

        if (configurationRoot != null) configurationRoot.SetActive(true);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        EnsureRootCanvasSorting();
        if (configurationRoot != null) configurationRoot.transform.SetAsLastSibling();
        EnsureEventSystem();
        EnsureRaycaster();
        EnsureButtonsInteractive();
        AutoWireTabButtons();
        AutoWireActionButtons();
        EnsureRootCanvasGroupInteractive();
        EnsureBackgroundUnblocked();
        InitAudioLabels();
        var sty = configurationRoot != null ? configurationRoot.GetComponentInParent<AutoSelectableStyling>() : null;
        if (sty != null)
        {
            sty.SetTargetRoot(configurationRoot.transform);
            sty.AplicarEstilosATodos();
        }

        currentTabIndex = 2;

        // ✅ ARREGLO: FORZAR desactivación de todos antes de mostrar
        foreach (var tab in tabs)
        {
            if (tab != null) tab.SetActive(false);
        }

        if (tabs != null)
        {
            if (currentTabIndex < 0 || currentTabIndex >= tabs.Length || tabs[currentTabIndex] == null)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    if (tabs[i] != null)
                    {
                        currentTabIndex = i;
                        break;
                    }
                }
            }
        }
        ShowCurrentTab();
        FixVisibility();

        Debug.Log($"[ConfigPanel] Abierto correctamente en tab {currentTabIndex}");
    }

    public void Close()
    {
        Debug.Log("[ConfigPanel] Cerrando");

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
                Debug.Log($"[ConfigPanel] GraphicRaycaster agregado a canvas {canvas.gameObject.name}");
            }
            rc.enabled = true;
            Debug.Log($"[ConfigPanel] GraphicRaycaster habilitado en canvas {canvas.gameObject.name}");
        }
    }

    private void EnsureRootCanvasSorting()
    {
        if (configurationRoot == null) return;
        var canvas = configurationRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = configurationRoot.AddComponent<Canvas>();
            configurationRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        if (overrideSorting)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                canvas.sortingLayerName = sortingLayerName;
            }
        }
        if (attachToMainCamera && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;
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
            Debug.Log("[ConfigPanel] EventSystem creado con StandaloneInputModule");
        }
        else
        {
            var module = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (module == null)
            {
                es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[ConfigPanel] StandaloneInputModule agregado al EventSystem existente");
            }
            es.sendNavigationEvents = false;
            Debug.Log("[ConfigPanel] Navegación por teclado desactivada (solo mouse)");
        }
    }

    // Ocultación automática deshabilitada

    private void ShowCurrentTab()
    {
        if (tabs == null || tabs.Length == 0)
        {
            Debug.LogError("[ConfigPanel] No hay tabs para mostrar");
            return;
        }

        Debug.Log($"[ConfigPanel] Cambiando a tab {currentTabIndex}");

        // ✅ PASO 1: Desactivar TODOS primero (evita superposición)
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] != null)
            {
                tabs[i].SetActive(false);
                Debug.Log($"[ConfigPanel] Tab {i} ({tabs[i].name}) desactivado");
            }
        }

        // ✅ PASO 2: Activar solo el actual
        if (currentTabIndex >= 0 && currentTabIndex < tabs.Length && tabs[currentTabIndex] != null)
        {
            tabs[currentTabIndex].SetActive(true);
            Debug.Log($"[ConfigPanel] Tab {currentTabIndex} ({tabs[currentTabIndex].name}) activado");
            if (currentTabLabel != null) currentTabLabel.text = tabs[currentTabIndex].name;
            if (audioPanel != null && tabs[currentTabIndex] == audioPanel) EnsureAudioInteractive();
        }
        else
        {
            Debug.LogError($"[ConfigPanel] Índice inválido: {currentTabIndex}");
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
            Debug.LogWarning("[ConfigPanel] Tabs no inicializados en Update");
            InitializeTabs();
            return;
        }

        if (allowKeyboardTabSwitch)
        {
            if (Input.GetKeyDown(prevTabKey))
            {
                Debug.Log($"[ConfigPanel] Q presionado - tab actual: {currentTabIndex}");
                PrevTab();
            }

            if (Input.GetKeyDown(nextTabKey))
            {
                Debug.Log($"[ConfigPanel] E presionado - tab actual: {currentTabIndex}");
                NextTab();
            }

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
    }

    public void PrevTab()
    {
        if (tabs == null || tabs.Length == 0) return;

        int oldIndex = currentTabIndex;
        currentTabIndex--;

        if (currentTabIndex < 0)
            currentTabIndex = tabs.Length - 1;

        Debug.Log($"[ConfigPanel] Tab anterior: {oldIndex} -> {currentTabIndex}");
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

        Debug.Log($"[ConfigPanel] Tab siguiente: {oldIndex} -> {currentTabIndex}");
        ShowCurrentTab();
        FixVisibility();
    }

    // Métodos para botones UI
    public void ShowPreferences()
    {
        currentTabIndex = 0;
        ShowCurrentTab();
        FixVisibility();
    }

    public void ShowKeybinds()
    {
        currentTabIndex = 1;
        ShowCurrentTab();
        FixVisibility();
    }

    public void ShowAudio()
    {
        currentTabIndex = 2;
        ShowCurrentTab();
        FixVisibility();
        InitAudioLabels();
    }

    public void ConfirmChanges()
    {
        Debug.Log("[ConfigPanel] Confirmando cambios");

        var pref = preferencesPanel?.GetComponent<PreferenceSettings>();
        if (pref != null) pref.ApplyAll();

        var keyb = keybindsPanel?.GetComponent<KeybindsSettings>();
        if (keyb != null) keyb.ConfirmChanges();

        Close();
    }

    public void RestartValues()
    {
        Debug.Log("[ConfigPanel] Reseteando valores");

        var pref = preferencesPanel?.GetComponent<PreferenceSettings>();
        if (pref != null) pref.ResetToDefaults();

        var keyb = keybindsPanel?.GetComponent<KeybindsSettings>();
        if (keyb != null) keyb.ResetToDefaults();

        var am = AudioManager.Instance;
        if (am != null)
        {
            am.SetMasterVolume(1f);
            am.SetMusicVolume(1f);
            am.SetSFXVolume(1f);
        }

        if (audioPanel != null)
        {
            var sliders = audioPanel.GetComponentsInChildren<UnityEngine.UI.Slider>(true);
            for (int i = 0; i < sliders.Length; i++)
            {
                var s = sliders[i];
                var n = s.gameObject.name.ToLowerInvariant();
                if (am == null) continue;
                if (n.Contains("master")) s.value = am.masterVolume;
                else if (n.Contains("music")) s.value = am.musicVolume;
                else if (n.Contains("sfx") || n.Contains("effects")) s.value = am.sfxVolume;
            }
            UpdateAudioLabels();
        }
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

    private void InitAudioLabels()
    {
        if (masterLabel != null)
        {
            masterLabel.richText = false;
            var cg = masterLabel.GetComponentInParent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        }
        if (musicLabel != null)
        {
            musicLabel.richText = false;
            var cg = musicLabel.GetComponentInParent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        }
        if (sfxLabel != null)
        {
            sfxLabel.richText = false;
            var cg = sfxLabel.GetComponentInParent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        }
        UpdateAudioLabels();
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
                Debug.Log($"[ConfigPanel] Audio: Raycaster habilitado en canvas {c.gameObject.name}");
            }
        }

        var cg = audioPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = audioPanel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        Debug.Log("[ConfigPanel] Audio: CanvasGroup activo e interactivo");

        var sliders = audioPanel.GetComponentsInChildren<UnityEngine.UI.Slider>(true);
        Debug.Log($"[ConfigPanel] Audio: {sliders.Length} sliders detectados");
        for (int i = 0; i < sliders.Length; i++)
        {
            var s = sliders[i];
            s.interactable = true;
            if (s.handleRect == null)
            {
                var handleGo = new GameObject("Handle");
                var hrt = handleGo.AddComponent<RectTransform>();
                hrt.SetParent(s.transform, false);
                hrt.sizeDelta = new Vector2(16f, 16f);
                var himg = handleGo.AddComponent<UnityEngine.UI.Image>();
                himg.color = Color.white;
                himg.raycastTarget = true;
                s.handleRect = hrt;
                Debug.Log($"[ConfigPanel] Slider '{s.gameObject.name}': se creó Handle y se asignó");
            }
            var h = s.handleRect;
            if (h != null)
            {
                var g = h.GetComponent<UnityEngine.UI.Graphic>();
                if (g != null) g.raycastTarget = true;
                Debug.Log($"[ConfigPanel] Slider '{s.gameObject.name}': handle raycastTarget ON");
            }
            var bg = s.transform.Find("Background");
            if (bg != null)
            {
                var gb = bg.GetComponent<UnityEngine.UI.Graphic>();
                if (gb != null) gb.raycastTarget = true;
                Debug.Log($"[ConfigPanel] Slider '{s.gameObject.name}': background raycastTarget ON");
            }
            else
            {
                Debug.LogWarning($"[ConfigPanel] Slider '{s.gameObject.name}': Background no encontrado");
            }

            LogRaycastAt(s.GetComponent<RectTransform>(), "Slider");
        }
        var am = AudioManager.Instance;
        if (am != null)
        {
            for (int i = 0; i < sliders.Length; i++)
            {
                var s = sliders[i];
                var n = s.gameObject.name.ToLowerInvariant();
                if (n.Contains("master")) s.value = am.masterVolume;
                else if (n.Contains("music")) s.value = am.musicVolume;
                else if (n.Contains("sfx") || n.Contains("effects")) s.value = am.sfxVolume;
            }
            UpdateAudioLabels();
        }

        var buttons = audioPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
            var img = buttons[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
        Debug.Log($"[ConfigPanel] Audio: botón '{buttons[i].gameObject.name}' interactivo (listeners={buttons[i].onClick.GetPersistentEventCount()})");
        }

        EnsureBackgroundUnblocked();

        LogRaycastAt(audioPanel.GetComponent<RectTransform>(), "Audio_Panel");

        UpdateAudioLabels();
    }

    private void EnsureButtonsInteractive()
    {
        if (configurationRoot == null) return;
        if (buttonClickSound == null) { /* no-op, optional sound */ }
        var buttons = configurationRoot.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            b.interactable = true;
            var img = b.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
            Debug.Log($"[ConfigPanel] Botón '{b.gameObject.name}' habilitado (listeners={b.onClick.GetPersistentEventCount()})");
            WireButtonClickSound(b);
        }

        var toggles = configurationRoot.GetComponentsInChildren<UnityEngine.UI.Toggle>(true);
        for (int i = 0; i < toggles.Length; i++)
        {
            var t = toggles[i];
            t.interactable = true;
            var img = t.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
            Debug.Log($"[ConfigPanel] Toggle '{t.gameObject.name}' habilitado");
        }
    }

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    private System.Collections.Generic.HashSet<int> wiredButtonsSound = new System.Collections.Generic.HashSet<int>();

    private void WireButtonClickSound(UnityEngine.UI.Button b)
    {
        int id = b.GetInstanceID();
        if (wiredButtonsSound.Contains(id)) return;
        b.onClick.AddListener(PlayClickSound);
        wiredButtonsSound.Add(id);
    }

    private void PlayClickSound()
    {
        if (buttonClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound, 0.7f);
        }
    }

    private void AutoWireTabButtons()
    {
        var root = configurationRoot != null ? configurationRoot : gameObject;
        var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            var n = b.gameObject.name.ToLowerInvariant();
            int existing = b.onClick.GetPersistentEventCount();

            if (n.Contains("preferences") || n.Contains("preferencias") || n.Contains("prefs"))
            {
                if (existing == 0) b.onClick.AddListener(ShowPreferences);
            }
            else if (n.Contains("keybind") || n.Contains("bindings") || n.Contains("keys"))
            {
                if (existing == 0) b.onClick.AddListener(ShowKeybinds);
            }
            else if (n.Contains("audio"))
            {
                if (existing == 0) b.onClick.AddListener(ShowAudio);
            }
        }
    }

    private void AutoWireActionButtons()
    {
        var root = configurationRoot != null ? configurationRoot : gameObject;
        var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            var n = b.gameObject.name.ToLowerInvariant();
            int existing = b.onClick.GetPersistentEventCount();

            if (n.Contains("restart") || n.Contains("reset"))
            {
                if (existing == 0) b.onClick.AddListener(RestartValues);
            }
            else if (n.Contains("confirm") || n.Contains("apply"))
            {
                if (existing == 0) b.onClick.AddListener(ConfirmChanges);
            }
            else if (n.Contains("newgameplus") || n.Contains("ngplus") || n.Contains("ng+") || n.Contains("reiniciar ng") || n.Contains("reiniciar new game plus"))
            {
                if (existing == 0) b.onClick.AddListener(ResetNewGamePlusProgressButton);
            }
        }
    }

    private void EnsureRootCanvasGroupInteractive()
    {
        if (configurationRoot == null) return;
        var cgs = configurationRoot.GetComponentsInParent<CanvasGroup>(true);
        for (int i = 0; i < cgs.Length; i++)
        {
            var cg = cgs[i];
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            Debug.Log($"[ConfigPanel] CanvasGroup '{cg.gameObject.name}' en cadena activado");
        }
    }


    private void EnsureBackgroundUnblocked()
    {
        if (configurationRoot == null) return;
        var bgRoot = configurationRoot.transform.Find("Background");
        if (bgRoot != null)
        {
            var bgImg = bgRoot.GetComponent<UnityEngine.UI.Image>();
            if (bgImg != null)
            {
                bgImg.raycastTarget = false;
                Debug.Log("[ConfigPanel] Background general no bloquea raycasts");
            }
        }
    }

    private const float volumeStep = 0.1f;


    private void AdjustMaster(float delta)
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        am.SetMasterVolume(Mathf.Clamp01(am.masterVolume + delta));
        UpdateAudioLabels();
    }

    private void AdjustMusic(float delta)
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        am.SetMusicVolume(Mathf.Clamp01(am.musicVolume + delta));
        UpdateAudioLabels();
    }

    private void AdjustSFX(float delta)
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        am.SetSFXVolume(Mathf.Clamp01(am.sfxVolume + delta));
        UpdateAudioLabels();
    }

    private void UpdateAudioLabels()
    {
        if (audioPanel == null) return;
        var am = AudioManager.Instance;
        if (am == null) return;
        if (masterLabel != null) masterLabel.text = Mathf.RoundToInt(am.masterVolume * 100f) + "%";
        if (musicLabel != null) musicLabel.text = Mathf.RoundToInt(am.musicVolume * 100f) + "%";
        if (sfxLabel != null) sfxLabel.text = Mathf.RoundToInt(am.sfxVolume * 100f) + "%";
    }

    // Métodos públicos para wiring directo en el Inspector
    public void MasterUp() { AdjustMaster(volumeStep); }
    public void MasterDown() { AdjustMaster(-volumeStep); }
    public void MusicUp() { AdjustMusic(volumeStep); }
    public void MusicDown() { AdjustMusic(-volumeStep); }
    public void SFXUp() { AdjustSFX(volumeStep); }
    public void SFXDown() { AdjustSFX(-volumeStep); }
    public void ResetNewGamePlusProgressButton()
    {
        var ctrl = ControladorDatosJuego.Instance;
        if (ctrl != null)
        {
            ctrl.ResetNewGamePlusProgress();
            ctrl.SetStartModeVariant(0);
        }
        ChangeScene.MainMenuVariation = 0;
        SceneManager.LoadScene("MainMenu");
    }
    public void StartNewGameOriginalButton()
    {
        var cs = FindFirstObjectByType<ChangeScene>(FindObjectsInactive.Include);
        if (cs != null)
        {
            cs.NewGameForceOriginal();
            return;
        }
        var ctrl = ControladorDatosJuego.Instance;
        if (ctrl != null)
        {
            ctrl.SetStartModeVariant(0);
            ctrl.ResetearDatos();
            ctrl.datosjuego.jefesDerrotados.Clear();
        }
        ChangeScene.MainMenuVariation = 0;
        SceneManager.LoadScene("TheForest");
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
        string log = "[ConfigPanel] Raycast " + context + " -> ";
        for (int i = 0; i < results.Count && i < 5; i++)
        {
            log += results[i].gameObject.name + (i < results.Count - 1 ? ", " : "");
        }
        Debug.Log(log);
    }
    public bool IsOpen => configurationRoot != null && configurationRoot.activeSelf;
}
