using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class BorderlessTopmost : MonoBehaviour
{
    [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] static extern int GetSystemMetrics(int nIndex);

    const int GWL_STYLE = -16;
    const int WS_POPUP = unchecked((int)0x80000000);
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint SWP_SHOWWINDOW = 0x0040;
    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;

    [SerializeField] private bool coverPrimaryScreen = true;
    [SerializeField] private bool setTopMost = true;

    void Start()
    {
        var hwnd = GetActiveWindow();
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP);
        if (coverPrimaryScreen)
        {
            int w = GetSystemMetrics(SM_CXSCREEN);
            int h = GetSystemMetrics(SM_CYSCREEN);
            SetWindowPos(hwnd, setTopMost ? HWND_TOPMOST : IntPtr.Zero, 0, 0, w, h, SWP_SHOWWINDOW);
        }
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
    }
}

