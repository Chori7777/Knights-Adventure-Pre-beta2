using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class OverlayServer : MonoBehaviour
{
    [SerializeField] private int port = 7777;
    [SerializeField] private OverlayController controller;
    private TcpListener server;
    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object locker = new object();

    void Start()
    {
        server = new TcpListener(IPAddress.Loopback, port);
        server.Start();
        server.BeginAcceptTcpClient(OnClient, null);
    }

    void Update()
    {
        lock (locker)
        {
            while (actions.Count > 0)
            {
                var a = actions.Dequeue();
                a?.Invoke();
            }
        }
    }

    void OnClient(IAsyncResult ar)
    {
        try
        {
            var client = server.EndAcceptTcpClient(ar);
            server.BeginAcceptTcpClient(OnClient, null);
            var s = client.GetStream();
            byte[] buf = new byte[8192];
            int n = s.Read(buf, 0, buf.Length);
            string cmd = Encoding.UTF8.GetString(buf, 0, n).Trim();
            Enqueue(Parse(cmd));
            client.Close();
        }
        catch
        {
        }
    }

    Action Parse(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return null;
        if (cmd.StartsWith("SHOW_IMAGE "))
        {
            string path = cmd.Substring(11).Trim('"');
            return () => controller?.ShowImage(path);
        }
        if (cmd.StartsWith("SHOW_VIDEO "))
        {
            string path = cmd.Substring(11).Trim('"');
            return () => controller?.ShowVideo(path);
        }
        if (cmd.StartsWith("SHOW_TEXT "))
        {
            string t = cmd.Substring(10);
            return () => controller?.ShowText(t);
        }
        if (cmd.StartsWith("HIDE_ALL"))
        {
            return () => controller?.HideAll();
        }
        if (cmd.StartsWith("OPACITY "))
        {
            string v = cmd.Substring(8);
            if (float.TryParse(v, out var a))
            {
                return () => controller?.SetOpacity(a);
            }
        }
        return null;
    }

    void Enqueue(Action a)
    {
        if (a == null) return;
        lock (locker) actions.Enqueue(a);
    }
}

