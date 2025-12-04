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
        if (optionsPanel != null) optionsPanel.gameObject.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.gameObject.SetActive(keepMainVisible);

        if (optionsNavigator != null)
        {
            optionsNavigator.SetPointer(sharedPointer);
            optionsNavigator.RefreshItems();
            optionsNavigator.FocusFirst();
        }

        if (blockMainRaycastsWhenOptions)
        {
            if (mainRaycaster != null) mainRaycaster.enabled = false;
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
            }
        }

        if (optionsRaycaster != null) optionsRaycaster.enabled = true;
        if (optionsCanvasGroup != null)
        {
            optionsCanvasGroup.interactable = true;
            optionsCanvasGroup.blocksRaycasts = true;
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.gameObject.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.gameObject.SetActive(true);

        if (mainNavigator != null)
        {
            mainNavigator.SetPointer(sharedPointer);
            mainNavigator.RefreshItems();
            mainNavigator.FocusFirst();
        }

        if (blockMainRaycastsWhenOptions)
        {
            if (mainRaycaster != null) mainRaycaster.enabled = true;
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.interactable = true;
                mainCanvasGroup.blocksRaycasts = true;
            }
        }

        if (optionsRaycaster != null) optionsRaycaster.enabled = false;
        if (optionsCanvasGroup != null)
        {
            optionsCanvasGroup.interactable = false;
            optionsCanvasGroup.blocksRaycasts = false;
        }
    }
}

