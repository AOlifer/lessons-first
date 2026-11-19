using System.Windows;
using System.Windows.Input;
using LessonsFirst.App.Services;

namespace LessonsFirst.App.Windows;

public partial class PinSetupWindow : Window
{
    private readonly AuthService _auth;

    /// <summary>Вызывается после успешной установки PIN.</summary>
    public event Action? PinSet;

    public PinSetupWindow(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Save_Click(sender, e);
            e.Handled = true;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var pin = PinBox.Password;
        if (pin.Trim().Length < 4)
        {
            ErrorText.Text = "PIN должен содержать минимум 4 символа.";
            return;
        }

        if (pin != PinRepeatBox.Password)
        {
            ErrorText.Text = "PIN-коды не совпадают.";
            return;
        }

        _auth.SetPin(pin);
        Close();
        PinSet?.Invoke();
    }
}
