using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using LessonsFirst.App.Interop;

namespace LessonsFirst.App.Windows;

/// <summary>
/// Полноэкранное поздравление после решения всех задач.
/// Закрывается кнопкой или сама через несколько секунд, после чего замок считается снятым.
/// </summary>
public partial class SuccessWindow : Window
{
    private readonly DispatcherTimer _autoClose;

    public SuccessWindow()
    {
        InitializeComponent();

        _autoClose = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        _autoClose.Tick += (_, _) => Close();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Границы в DIP до первой отрисовки — иначе окно успевает мигнуть
        // в дефолтном размере перед растягиванием на весь экран.
        var (px, py, pw, ph) = Win32.GetPrimaryScreenBounds();
        var scale = System.Windows.Media.VisualTreeHelper.GetDpi(this).PixelsPerDip;
        Left = px / scale;
        Top = py / scale;
        Width = pw / scale;
        Height = ph / scale;

        ApplyBounds();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        _autoClose.Start();
    }

    private void ApplyBounds()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);
        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE, exStyle | Win32.WS_EX_TOOLWINDOW);

        var (x, y, w, h) = Win32.GetPrimaryScreenBounds();
        Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, x, y, w, h, Win32.SWP_SHOWWINDOW);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
