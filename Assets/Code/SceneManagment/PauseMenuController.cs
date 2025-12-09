using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

    public class PauseMenuController : MonoBehaviour
    {
    public static PauseMenuController Instance { get; private set; }
    [SerializeField] private CanvasGroup pauseGroup;
    [SerializeField] private ConfigurationPanel optionsConfig;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private bool forceInteractive = true;
    [SerializeField] private bool disableInMainMenu = true;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool paused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        var root = transform.root != null ? transform.root.gameObject : gameObject;
        if (root.scene.name != "DontDestroyOnLoad")
        {
            DontDestroyOnLoad(root);
        }
        HidePauseInstant();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (paused) ClosePause(); else OpenPause();
    }

    public void OpenPause()
    {
        Time.timeScale = 0f;
        if (pauseGroup != null)
        {
            pauseGroup.alpha = 1f;
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
        }
        EnsureEventSystem();
        EnsureRaycaster();
        if (forceInteractive) EnsureChildrenInteractive();
        EnsurePauseBackgroundUnblocked();
        if (pauseGroup != null && pauseGroup.GetComponent<MenuPanel>() == null)
            pauseGroup.gameObject.AddComponent<MenuPanel>();
        AutoWirePauseButtons();
        var es = EventSystem.current;
        if (es != null) es.sendNavigationEvents = true;
        paused = true;
    }

    public void ClosePause()
    {
        if (optionsConfig != null) optionsConfig.Close();
        if (pauseGroup != null)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;
        }
        Time.timeScale = 1f;
        paused = false;
    }

    public void OnResume()
    {
        ClosePause();
    }

    public void OnOptions()
    {
        Debug.Log("[Pause] Options pressed. paused=" + paused + " optionsConfig=" + (optionsConfig != null));
        if (optionsConfig == null)
        {
            LinkOptionsPanel();
            Debug.Log("[Pause] Options auto-linked: " + (optionsConfig != null));
        }
        if (optionsConfig == null) return;

        if (!paused) OpenPause();

        if (optionsConfig.IsOpen)
        {
            optionsConfig.Close();
            Debug.Log("[Pause] Options closed");
        }
        else
        {
            optionsConfig.Open();
            Debug.Log("[Pause] Options opened");
        }
    }

    private void LinkOptionsPanel()
    {
        // Buscar por tipo incluyendo objetos inactivos
        optionsConfig = FindFirstObjectByType<ConfigurationPanel>(FindObjectsInactive.Include);
        if (optionsConfig != null) return;

        // Buscar por nombre común
        var allRoots = gameObject.scene.GetRootGameObjects();
        for (int i = 0; i < allRoots.Length && optionsConfig == null; i++)
        {
            var go = allRoots[i].transform.Find("Options");
            if (go != null)
            {
                optionsConfig = go.GetComponent<ConfigurationPanel>();
                if (optionsConfig != null) break;
            }
        }

        // Fallback: buscar cualquier objeto activo/inactivo con nombre parecido
        if (optionsConfig == null)
        {
            var cfgs = Resources.FindObjectsOfTypeAll<ConfigurationPanel>();
            if (cfgs != null && cfgs.Length > 0)
                optionsConfig = cfgs[0];
        }
    }

    public void CloseOptions()
    {
        if (optionsConfig != null && optionsConfig.IsOpen)
        {
            optionsConfig.Close();
        }
    }

    private void HidePauseInstant()
    {
        if (pauseGroup != null)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;
        }
        paused = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HidePauseInstant();
        if (disableInMainMenu && scene.name == mainMenuSceneName)
        {
            if (pauseGroup != null) pauseGroup.gameObject.SetActive(false);
        }
        else
        {
            if (pauseGroup != null) pauseGroup.gameObject.SetActive(true);
        }
    }

    public void SetOptionsConfig(ConfigurationPanel cfg)
    {
        optionsConfig = cfg;
    }

    private void EnsureEventSystem()
    {
        var es = EventSystem.current;
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
        else
        {
            if (es.GetComponent<StandaloneInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void EnsureRaycaster()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var rc = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (rc == null) canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    private void EnsureChildrenInteractive()
    {
        var groups = GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].alpha = 1f;
            groups[i].interactable = true;
            groups[i].blocksRaycasts = true;
        }

        var buttons = GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
            var img = buttons[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
        }

        var toggles = GetComponentsInChildren<UnityEngine.UI.Toggle>(true);
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].interactable = true;
            var img = toggles[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
        }
    }

    private void AutoWirePauseButtons()
    {
        var root = pauseGroup != null ? pauseGroup.gameObject : gameObject;
        var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var b = buttons[i];
            var n = b.gameObject.name.ToLowerInvariant();
            string label = null;
            var t = b.GetComponentInChildren<TMP_Text>();
            if (t != null) label = t.text.ToLowerInvariant();
            else
            {
                var ut = b.GetComponentInChildren<UnityEngine.UI.Text>();
                if (ut != null) label = ut.text.ToLowerInvariant();
            }
            int existing = b.onClick.GetPersistentEventCount();

            if ((n.Contains("pause")) || (label != null && label.Contains("pause")))
            {
                if (existing == 0) b.onClick.AddListener(OpenPause);
            }
            else if (n.Contains("resume") || n.Contains("continuar") || (label != null && (label.Contains("resume") || label.Contains("continuar") || label.Contains("seguir"))))
            {
                if (existing == 0) b.onClick.AddListener(OnResume);
            }
            else if (n.Contains("options") || n.Contains("opciones") || n.Contains("config") || n.Contains("settings") || (label != null && (label.Contains("options") || label.Contains("opciones") || label.Contains("config") || label.Contains("settings"))))
            {
                if (existing == 0) b.onClick.AddListener(OnOptions);
            }
            else if (n.Contains("restart") || n.Contains("reset") || n.Contains("reiniciar") || n.Contains("retry") || (label != null && (label.Contains("restart") || label.Contains("reiniciar") || label.Contains("reset") || label.Contains("retry"))))
            {
                if (existing == 0) b.onClick.AddListener(RestartLevel);
            }
            else if (n.Contains("checkpoint") || n.Contains("continue") || (label != null && (label.Contains("checkpoint") || label.Contains("continue") || label.Contains("continuar"))))
            {
                if (existing == 0) b.onClick.AddListener(LoadCheckpoint);
            }
            else if (n.Contains("menu") || n.Contains("main") || (label != null && (label.Contains("menu") || label.Contains("main"))))
            {
                if (existing == 0) b.onClick.AddListener(GoToMainMenu);
            }
            Debug.Log("[PauseWire] " + b.gameObject.name + " wired");
        }
    }

    private void EnsurePauseBackgroundUnblocked()
    {
        if (pauseGroup == null) return;
        var tr = pauseGroup.transform.Find("Background");
        if (tr == null) return;
        var img = tr.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.raycastTarget = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
        paused = false;
    }

    public void LoadCheckpoint()
    {
        Time.timeScale = 1f;
        var datos = ControladorDatosJuego.Instance;
        if (datos == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            paused = false;
            return;
        }
        datos.CargarDatos();
        var escena = datos.datosjuego.escenaActual;
        if (!string.IsNullOrEmpty(escena))
        {
            SceneManager.LoadScene(escena);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        paused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        paused = false;
    }
}
