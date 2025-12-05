using System;
using System.Runtime.InteropServices;

public static class WindowEffects
{
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);

    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_SHOWWINDOW = 0x0040;

    public static void MoveTo(int x, int y)
    {
        var h = GetActiveWindow();
        SetWindowPos(h, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
    }

    public static RECT GetRect()
    {
        var h = GetActiveWindow();
        GetWindowRect(h, out var r);
        return r;
    }

    public static void SetTopMost(bool top)
    {
        var h = GetActiveWindow();
        SetWindowPos(h, top ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
        SetForegroundWindow(h);
    }
}

