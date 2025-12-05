using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseGroup;
    [SerializeField] private MenuOverlayManager overlay;
    [SerializeField] private OptionsMenuController optionsController;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    private bool paused;

    private void Awake()
    {
        HidePauseInstant();
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
        var es = EventSystem.current;
        if (es != null) es.sendNavigationEvents = true;
        paused = true;
    }

    public void ClosePause()
    {
        if (optionsController != null) optionsController.Hide();
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
        if (!paused) OpenPause();
        if (overlay != null) overlay.SetKeepMainVisible(true);
        if (optionsController != null) optionsController.Show();
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
}
