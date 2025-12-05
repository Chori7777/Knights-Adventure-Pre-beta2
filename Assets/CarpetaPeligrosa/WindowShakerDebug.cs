using UnityEngine;

public class WindowShakerDebug : MonoBehaviour
{
    [SerializeField] private int amplitudeX = 40;
    [SerializeField] private int amplitudeY = 20;
    [SerializeField] private float speed = 3f;
    private int baseX;
    private int baseY;

    void Start()
    {
        var r = WindowEffects.GetRect();
        baseX = r.Left;
        baseY = r.Top;
    }

    void Update()
    {
        if (Screen.fullScreenMode != FullScreenMode.Windowed) return;
        int x = baseX + (int)(Mathf.Sin(Time.time * speed) * amplitudeX);
        int y = baseY + (int)(Mathf.Cos(Time.time * speed * 0.8f) * amplitudeY);
        WindowEffects.MoveTo(x, y);
    }
}

