using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public static OptionsMenuController Instance;

    [SerializeField] private CanvasGroup group;
    [SerializeField] private MenuOverlayManager overlay;

    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        HideInstant();
    }

    public void Show()
    {
        if (overlay != null) overlay.OpenOptions();
        var conf = GetComponent<ConfigurationPanel>();
        if (conf != null) conf.Open();
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        isOpen = true;
    }

    public void Hide()
    {
        if (overlay != null) overlay.CloseOptions();
        var conf = GetComponent<ConfigurationPanel>();
        if (conf != null) conf.Close();
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        isOpen = false;
    }

    public void Toggle()
    {
        if (isOpen) Hide(); else Show();
    }

    private void HideInstant()
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        isOpen = false;
    }
}
