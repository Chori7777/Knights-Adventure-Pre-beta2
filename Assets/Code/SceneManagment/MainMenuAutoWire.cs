using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MainMenuAutoWire
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        TryWireOptionsByName("Options");
        TryWireOptionsByName("Config");
        TryWireOptionsByText("Options");
        TryWireOptionsByText("Config");
    }

    private static void TryWireOptionsByName(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) EnsureButton(go);
    }

    private static void TryWireOptionsByText(string contains)
    {
        var texts = Object.FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            if (!string.IsNullOrEmpty(t.text) && t.text.IndexOf(contains, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                EnsureButton(t.gameObject);
            }
        }
    }

    private static void EnsureButton(GameObject target)
    {
        var btn = target.GetComponent<Button>();
        if (btn == null) btn = target.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OpenOptions);

        var canvas = target.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var rc = canvas.GetComponent<GraphicRaycaster>();
            if (rc == null) rc = canvas.gameObject.AddComponent<GraphicRaycaster>();
            rc.enabled = true;
        }
    }

    private static void OpenOptions()
    {
        var ctrl = Object.FindFirstObjectByType<OptionsMenuController>();
        if (ctrl != null)
        {
            ctrl.Show();
            return;
        }

        var overlay = Object.FindFirstObjectByType<MenuOverlayManager>();
        if (overlay != null)
        {
            overlay.OpenOptions();
        }
    }
}
