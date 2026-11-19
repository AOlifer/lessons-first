using System.Windows;
using System.Windows.Interop;
using LessonsFirst.App.Interop;

namespace LessonsFirst.App.Windows;

/// <summary>
/// Тёмная заглушка на каждый дополнительный монитор: глотает клики, пока активен замок.
/// Задачи показываются только на главном экране.
/// </summary>
public partial class CoverWindow : Window
{
    private bool _allowClose;

    public CoverWindow() => InitializeComponent();

    /// <summary>Растягивает заглушку на указанный монитор (физические пиксели).</summary>
    public void SetMonitorBounds(int x, int y, int width, int height)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, x, y, width, height, Win32.SWP_SHOWWINDOW);
        }
    }

    public void ShowCover(int x, int y, int width, int height)
    {
        if (!IsLoaded)
        {
            // Границы нужны до первого показа: позиционируем окно через Left/Top.
            WindowStartupLocation = WindowStartupLocation.Manual;
            Show();
        }
        else if (!IsVisible)
        {
            Show();
        }

        SetMonitorBounds(x, y, width, height);
    }

    public void HideCover() => Hide();

    public void AllowClose()
    {
        _allowClose = true;
        Close();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Не показываемся в Alt+Tab.
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);
        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE, exStyle | Win32.WS_EX_TOOLWINDOW);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_allowClose)
            e.Cancel = true;
        else
            base.OnClosing(e);
    }
}
