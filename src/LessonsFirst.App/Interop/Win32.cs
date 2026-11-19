using System.Runtime.InteropServices;

namespace LessonsFirst.App.Interop;

internal static class Win32
{
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public const uint SWP_SHOWWINDOW = 0x0040;

    public const int SM_CXSCREEN = 0;
    public const int SM_CYSCREEN = 1;
    public const int SM_XVIRTUALSCREEN = 76;
    public const int SM_YVIRTUALSCREEN = 77;
    public const int SM_CXVIRTUALSCREEN = 78;
    public const int SM_CYVIRTUALSCREEN = 79;

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>Прямоугольник всего виртуального экрана (все мониторы вместе).</summary>
    public static (int X, int Y, int Width, int Height) GetVirtualScreenBounds() =>
        (GetSystemMetrics(SM_XVIRTUALSCREEN),
            GetSystemMetrics(SM_YVIRTUALSCREEN),
            GetSystemMetrics(SM_CXVIRTUALSCREEN),
            GetSystemMetrics(SM_CYVIRTUALSCREEN));

    /// <summary>Прямоугольник главного монитора в физических пикселях (начало — 0,0).</summary>
    public static (int X, int Y, int Width, int Height) GetPrimaryScreenBounds() =>
        (0, 0, GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
}
