using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using System.IO;

public class OverlayController : MonoBehaviour
{
    [SerializeField] private RawImage overlay;
    [SerializeField] private TextMeshProUGUI overlayText;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture videoTarget;

    public void ShowImage(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (!File.Exists(path)) return;
        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(tex, bytes);
        if (overlay != null)
        {
            overlay.texture = tex;
            overlay.color = Color.white;
        }
        if (videoPlayer != null) videoPlayer.Stop();
    }

    public void ShowVideo(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (!File.Exists(path)) return;
        if (videoPlayer == null || overlay == null || videoTarget == null) return;
        videoPlayer.targetTexture = videoTarget;
        overlay.texture = videoTarget;
        overlay.color = Color.white;
        videoPlayer.url = path;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    public void ShowText(string text)
    {
        if (overlayText == null) return;
        overlayText.text = text;
        overlayText.alpha = 1f;
    }

    public void HideAll()
    {
        if (overlay != null) overlay.color = new Color(1f, 1f, 1f, 0f);
        if (overlayText != null) overlayText.alpha = 0f;
        if (videoPlayer != null) videoPlayer.Stop();
    }

    public void SetOpacity(float a)
    {
        if (overlay == null) return;
        overlay.color = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
    }
}

