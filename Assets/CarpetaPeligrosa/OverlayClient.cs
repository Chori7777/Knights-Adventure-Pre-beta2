using System.Net.Sockets;
using System.Text;

public static class OverlayClient
{
    public static void Send(string cmd)
    {
        using var c = new TcpClient("127.0.0.1", 7777);
        var data = Encoding.UTF8.GetBytes(cmd);
        c.GetStream().Write(data, 0, data.Length);
    }

    public static void ShowImage(string path)
    {
        Send("SHOW_IMAGE \"" + path + "\"");
    }

    public static void ShowVideo(string path)
    {
        Send("SHOW_VIDEO \"" + path + "\"");
    }

    public static void ShowText(string text)
    {
        Send("SHOW_TEXT " + text);
    }

    public static void HideAll()
    {
        Send("HIDE_ALL");
    }

    public static void SetOpacity(float a)
    {
        Send("OPACITY " + a.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}

