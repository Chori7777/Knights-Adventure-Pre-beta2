using UnityEngine;

public class OverlayClientDebug : MonoBehaviour
{
    [SerializeField] private string imagePath;
    [SerializeField] private string videoPath;
    [SerializeField] private string messageText = "Hola";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            OverlayClient.ShowImage(imagePath);
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            OverlayClient.ShowVideo(videoPath);
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            OverlayClient.ShowText(messageText);
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            OverlayClient.HideAll();
        }
    }
}

