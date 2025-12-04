using UnityEngine;

public class ConfigurationPanel : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject configurationRoot;
    [SerializeField] private GameObject preferencesPanel;
    [SerializeField] private GameObject keybindsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private MenuOverlayManager overlayManager;
    [SerializeField] private KeyCode prevTabKey = KeyCode.Q;
    [SerializeField] private KeyCode nextTabKey = KeyCode.E;

    private int currentTabIndex;
    private GameObject[] tabs;

    public void Open()
    {
        if (overlayManager != null) overlayManager.OpenOptions();
        if (configurationRoot != null) configurationRoot.SetActive(true);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        tabs = new GameObject[] { preferencesPanel, keybindsPanel, audioPanel };
        currentTabIndex = 0;
        ShowPreferences();
    }

    public void Close()
    {
        if (overlayManager != null) overlayManager.CloseOptions();
        if (configurationRoot != null) configurationRoot.SetActive(false);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
    }

    public void ShowPreferences()
    {
        SetActivePanel(preferencesPanel);
    }

    public void ShowKeybinds()
    {
        SetActivePanel(keybindsPanel);
    }

    public void ShowAudio()
    {
        SetActivePanel(audioPanel);
    }

    private void SetActivePanel(GameObject go)
    {
        if (preferencesPanel != null) preferencesPanel.SetActive(go == preferencesPanel);
        if (keybindsPanel != null) keybindsPanel.SetActive(go == keybindsPanel);
        if (audioPanel != null) audioPanel.SetActive(go == audioPanel);
    }

    private void Update()
    {
        if (configurationRoot != null && configurationRoot.activeSelf)
        {
            if (Input.GetKeyDown(prevTabKey)) PrevTab();
            if (Input.GetKeyDown(nextTabKey)) NextTab();
        }
    }

    public void PrevTab()
    {
        if (tabs == null || tabs.Length == 0) return;
        currentTabIndex = (currentTabIndex - 1 + tabs.Length) % tabs.Length;
        SetActivePanel(tabs[currentTabIndex]);
    }

    public void NextTab()
    {
        if (tabs == null || tabs.Length == 0) return;
        currentTabIndex = (currentTabIndex + 1) % tabs.Length;
        SetActivePanel(tabs[currentTabIndex]);
    }

    public void ConfirmChanges()
    {
        var pref = preferencesPanel != null ? preferencesPanel.GetComponent<PreferenceSettings>() : null;
        if (pref != null) pref.ApplyAll();

        var keyb = keybindsPanel != null ? keybindsPanel.GetComponent<KeybindsSettings>() : null;
        if (keyb != null) keyb.ConfirmChanges();

        Close();
    }

    public void RestartValues()
    {
        var pref = preferencesPanel != null ? preferencesPanel.GetComponent<PreferenceSettings>() : null;
        if (pref != null) pref.ResetToDefaults();

        var keyb = keybindsPanel != null ? keybindsPanel.GetComponent<KeybindsSettings>() : null;
        if (keyb != null) keyb.ResetToDefaults();
    }
}
