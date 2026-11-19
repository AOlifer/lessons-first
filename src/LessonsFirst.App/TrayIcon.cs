using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using LessonsFirst.App.Interop;
using System.Windows.Controls;

namespace LessonsFirst.App;

/// <summary>Иконка в системном трее через Shell_NotifyIcon (без внешних пакетов).</summary>
internal sealed class TrayIcon : IDisposable
{
    private const uint WM_TRAYICON = 0x8000; // WM_APP
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint NIM_ADD = 0;
    private const uint NIM_MODIFY = 1;
    private const uint NIM_DELETE = 2;
    private const uint NIF_MESSAGE = 1;
    private const uint NIF_ICON = 2;
    private const uint NIF_TIP = 4;
    private const uint NIF_INFO = 0x10;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll", EntryPoint = "DestroyIcon")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private readonly HwndSource _source;
    private readonly IntPtr _iconHandle;
    private readonly ContextMenu _menu;
    private NOTIFYICONDATA _data;

    /// <summary>Левый клик по иконке.</summary>
    public event Action? Clicked;

    public TrayIcon(ContextMenu menu)
    {
        _menu = menu;

        // Окно-приёмник сообщений трея (message-only).
        var parameters = new HwndSourceParameters("LessonsFirstTray") { ParentWindow = new IntPtr(-3) };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        _iconHandle = CreateIcon();
        _data = new NOTIFYICONDATA
        {
            hWnd = _source.Handle,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _iconHandle,
            szTip = "Сначала уроки!",
        };
        Shell_NotifyIcon(NIM_ADD, ref _data);
    }

    public void ShowBalloon(string title, string text)
    {
        var data = _data;
        data.uFlags = NIF_INFO;
        data.szInfoTitle = title;
        data.szInfo = text;
        Shell_NotifyIcon(NIM_MODIFY, ref data);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_TRAYICON)
            return IntPtr.Zero;

        var mouse = unchecked((uint)(long)lParam);
        if (mouse is WM_LBUTTONUP)
        {
            Clicked?.Invoke();
            handled = true;
        }
        else if (mouse is WM_RBUTTONUP)
        {
            _menu.IsOpen = true;
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static IntPtr CreateIcon()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.FromArgb(37, 64, 143));
            using var font = new Font("Segoe UI", 13, System.Drawing.FontStyle.Bold);
            var size = g.MeasureString("У!", font);
            g.DrawString("У!", font, Brushes.White, (32 - size.Width) / 2, (32 - size.Height) / 2 + 1);
        }

        return bitmap.GetHicon();
    }

    public void Dispose()
    {
        Shell_NotifyIcon(NIM_DELETE, ref _data);
        DestroyIcon(_iconHandle);
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }
}
