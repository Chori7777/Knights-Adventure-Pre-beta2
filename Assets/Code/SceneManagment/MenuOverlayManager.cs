using UnityEngine;
using UnityEngine.UI;

public class MenuOverlayManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private RectTransform mainMenuPanel;
    [SerializeField] private RectTransform optionsPanel;

    [Header("Navigators")]
    [SerializeField] private MenuNavigator mainNavigator;
    [SerializeField] private MenuNavigator optionsNavigator;
    [SerializeField] private RectTransform sharedPointer;

    [Header("Behavior")]
    [SerializeField] private bool keepMainVisible = false;
    [SerializeField] private bool blockMainRaycastsWhenOptions = true;

    [Header("Raycast/Canvas")]
    [SerializeField] private GraphicRaycaster mainRaycaster;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private GraphicRaycaster optionsRaycaster;
    [SerializeField] private CanvasGroup optionsCanvasGroup;

    public void OpenOptions()
    {
        if (optionsCanvasGroup != null)
        {
            optionsCanvasGroup.alpha = 1f;
            optionsCanvasGroup.interactable = true;
            optionsCanvasGroup.blocksRaycasts = true;
        }
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = keepMainVisible ? 1f : 0f;
            mainCanvasGroup.interactable = keepMainVisible;
            mainCanvasGroup.blocksRaycasts = keepMainVisible && !blockMainRaycastsWhenOptions;
        }

        if (optionsNavigator != null)
        {
            optionsNavigator.SetPointer(sharedPointer);
            optionsNavigator.RefreshItems();
            optionsNavigator.FocusFirst();
        }

        if (mainRaycaster != null)
        {
            mainRaycaster.enabled = !blockMainRaycastsWhenOptions && keepMainVisible;
        }
        if (optionsRaycaster != null) optionsRaycaster.enabled = true;
    }

    public void CloseOptions()
    {
        if (optionsCanvasGroup != null)
        {
            optionsCanvasGroup.alpha = 0f;
            optionsCanvasGroup.interactable = false;
            optionsCanvasGroup.blocksRaycasts = false;
        }
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1f;
            mainCanvasGroup.interactable = true;
            mainCanvasGroup.blocksRaycasts = true;
        }

        if (mainNavigator != null)
        {
            mainNavigator.SetPointer(sharedPointer);
            mainNavigator.RefreshItems();
            mainNavigator.FocusFirst();
        }

        if (mainRaycaster != null) mainRaycaster.enabled = true;
        if (optionsRaycaster != null) optionsRaycaster.enabled = false;
    }

    public void SetKeepMainVisible(bool value)
    {
        keepMainVisible = value;
    }

    public void SetOptionsNavigator(MenuNavigator nav)
    {
        optionsNavigator = nav;
        if (sharedPointer != null && optionsNavigator != null)
        {
            optionsNavigator.SetPointer(sharedPointer);
            optionsNavigator.RefreshItems();
            optionsNavigator.FocusFirst();
        }
    }
}
