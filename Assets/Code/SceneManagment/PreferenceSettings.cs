using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreferenceSettings : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown fpsDropdown;
    [SerializeField] private Slider textSpeedSlider;

    private Resolution[] availableResolutions;

    private void Awake()
    {
        PopulateResolutions();
        LoadPrefs();
        ApplyAll();
        HookEvents();
    }

    private void PopulateResolutions()
    {
        availableResolutions = Screen.resolutions;
        if (resolutionDropdown == null) return;
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            var r = availableResolutions[i];
            string option = r.width + "x" + r.height + " @" + r.refreshRateRatio.value + "Hz";
            options.Add(option);
            if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height) currentIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void HookEvents()
    {
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (vsyncToggle != null) vsyncToggle.onValueChanged.AddListener(SetVsync);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (fpsDropdown != null) fpsDropdown.onValueChanged.AddListener(SetFpsCap);
        if (textSpeedSlider != null) textSpeedSlider.onValueChanged.AddListener(SetTextSpeed);
    }

    private void LoadPrefs()
    {
        bool fs = PlayerPrefs.GetInt("pref_fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        int vs = PlayerPrefs.GetInt("pref_vsync", QualitySettings.vSyncCount > 0 ? 1 : 0);
        int resIndex = PlayerPrefs.GetInt("pref_res_index", resolutionDropdown != null ? resolutionDropdown.value : 0);
        int fps = PlayerPrefs.GetInt("pref_fps_cap", Application.targetFrameRate > 0 ? Application.targetFrameRate : 60);
        float ts = PlayerPrefs.GetFloat("pref_text_speed", 0.05f);
        if (fullscreenToggle != null) fullscreenToggle.isOn = fs;
        if (vsyncToggle != null) vsyncToggle.isOn = vs == 1;
        if (resolutionDropdown != null) resolutionDropdown.value = Mathf.Clamp(resIndex, 0, (availableResolutions?.Length ?? 1) - 1);
        if (fpsDropdown != null) fpsDropdown.value = FpsIndexFromValue(fps);
        if (textSpeedSlider != null) textSpeedSlider.value = ts;
    }

    public void ApplyAll()
    {
        SetFullscreen(fullscreenToggle != null && fullscreenToggle.isOn);
        SetVsync(vsyncToggle != null && vsyncToggle.isOn);
        if (resolutionDropdown != null) SetResolution(resolutionDropdown.value);
        if (fpsDropdown != null) SetFpsCap(fpsDropdown.value);
        if (textSpeedSlider != null) SetTextSpeed(textSpeedSlider.value);
    }

    public void ResetToDefaults()
    {
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;
        if (vsyncToggle != null) vsyncToggle.isOn = true;
        if (resolutionDropdown != null) resolutionDropdown.value = Mathf.Clamp(resolutionDropdown.value, 0, (availableResolutions?.Length ?? 1) - 1);
        if (fpsDropdown != null) fpsDropdown.value = FpsIndexFromValue(60);
        if (textSpeedSlider != null) textSpeedSlider.value = 0.05f;
        ApplyAll();
    }

    private void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt("pref_fullscreen", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetVsync(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
        PlayerPrefs.SetInt("pref_vsync", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetResolution(int index)
    {
        if (availableResolutions == null || availableResolutions.Length == 0) return;
        index = Mathf.Clamp(index, 0, availableResolutions.Length - 1);
        var r = availableResolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("pref_res_index", index);
        PlayerPrefs.Save();
    }

    private void SetFpsCap(int index)
    {
        int fps = FpsValueFromIndex(index);
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("pref_fps_cap", fps);
        PlayerPrefs.Save();
    }

    private void SetTextSpeed(float speed)
    {
        if (TextManager.Instance != null) TextManager.Instance.SetTypeSpeed(speed);
        PlayerPrefs.SetFloat("pref_text_speed", speed);
        PlayerPrefs.Save();
    }

    private int FpsIndexFromValue(int fps)
    {
        switch (fps)
        {
            case 30: return 0;
            case 60: return 1;
            case 120: return 2;
            case 144: return 3;
            case 240: return 4;
            default: return 1;
        }
    }

    private int FpsValueFromIndex(int index)
    {
        switch (index)
        {
            case 0: return 30;
            case 1: return 60;
            case 2: return 120;
            case 3: return 144;
            case 4: return 240;
            default: return 60;
        }
    }
}

