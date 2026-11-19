using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LessonsFirst.App.Services;
using LessonsFirst.App.Tasks;

namespace LessonsFirst.App.Windows;

public partial class SettingsWindow : Window
{
    private static readonly string[] GradeHints =
    {
        "Счёт в пределах 20 и 100, сравнение чисел, простые задачи на сложение и вычитание",
        "Таблица умножения, «на» и «в несколько раз», математический диктант, произведения и частные",
        "Умножение и деление больших чисел, уравнения, дроби, проценты, отрицательные числа",
    };

    private readonly SettingsService _settings;
    private readonly AuthService _auth;
    private readonly TaskProvider _tasks;
    private readonly UpdateService _updater;
    private UpdateInfo? _pendingUpdate;

    public SettingsWindow(SettingsService settings, AuthService auth, TaskProvider tasks, UpdateService updater, bool pinVerified = false)
    {
        InitializeComponent();
        _settings = settings;
        _auth = auth;
        _tasks = tasks;
        _updater = updater;

        GradeBox.ItemsSource = new[]
        {
            "1–2 класс — простой",
            "3–4 класс — базовый",
            "5–6 класс — сложный",
        };
        RequiredBox.ItemsSource = new[] { "3", "5", "10", "15", "20" };
        GradeBox.SelectionChanged += (_, _) =>
            GradeHintText.Text = GradeBox.SelectedIndex >= 0 && GradeBox.SelectedIndex < GradeHints.Length
                ? GradeHints[GradeBox.SelectedIndex]
                : "";

        if (pinVerified)
        {
            // PIN уже проверен (вход из экрана замка) — сразу показываем настройки.
            ShowSettings();
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (PinGatePanel.Visibility == Visibility.Visible)
        {
            Gate_Click(sender, e);
            e.Handled = true;
        }
    }

    private void Gate_Click(object sender, RoutedEventArgs e)
    {
        if (_auth.VerifyPin(PinBox.Password))
        {
            ShowSettings();
        }
        else
        {
            GateErrorText.Text = "Неверный PIN";
            PinBox.Clear();
            Keyboard.Focus(PinBox);
        }
    }

    private void ShowSettings()
    {
        var s = _settings.Settings;
        GradeBox.SelectedIndex = s.GradeBand switch
        {
            GradeBand.Grades12 => 0,
            GradeBand.Grades34 => 1,
            _ => 2,
        };
        RequiredBox.SelectedItem = s.RequiredCorrect.ToString();
        AutostartBox.IsChecked = s.AutostartEnabled;
        LockOnStartupBox.IsChecked = s.LockOnStartup;
        LockOnResumeBox.IsChecked = s.LockOnResume;
        ShowHintsBox.IsChecked = s.ShowHints;

        PinGatePanel.Visibility = Visibility.Collapsed;
        SettingsPanel.Visibility = Visibility.Visible;
    }

    private void ChangePin_Click(object sender, RoutedEventArgs e)
    {
        PinChangePanel.Visibility = Visibility.Visible;
        ChangePinButton.Visibility = Visibility.Collapsed;
        Keyboard.Focus(CurrentPinBox);
    }

    private void SavePin_Click(object sender, RoutedEventArgs e)
    {
        if (!_auth.VerifyPin(CurrentPinBox.Password))
        {
            PinChangeErrorText.Text = "Текущий PIN неверный.";
            return;
        }

        var newPin = NewPinBox.Password;
        if (newPin.Trim().Length < 4)
        {
            PinChangeErrorText.Text = "Новый PIN должен содержать минимум 4 символа.";
            return;
        }

        if (newPin != NewPinRepeatBox.Password)
        {
            PinChangeErrorText.Text = "Новый PIN и повтор не совпадают.";
            return;
        }

        _auth.SetPin(newPin);
        PinChangeErrorText.Foreground = (System.Windows.Media.Brush)FindResource("LimeBrush");
        PinChangeErrorText.Text = "PIN изменён ✓";

        CurrentPinBox.Clear();
        NewPinBox.Clear();
        NewPinRepeatBox.Clear();
    }

    private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusText.Text = "Проверяю…";
        _pendingUpdate = await _updater.CheckForUpdateAsync();

        if (_pendingUpdate is null)
        {
            UpdateStatusText.Text = $"У вас последняя версия ({_updater.CurrentVersion}).";
            return;
        }

        UpdateStatusText.Text = $"Доступна {_pendingUpdate.Tag}. Устанавливаю…";
        var ok = await _updater.DownloadAndApplyAsync(_pendingUpdate, msg => UpdateStatusText.Text = msg);
        if (!ok)
        {
            UpdateStatusText.Text += $" Скачайте вручную: {_pendingUpdate.NotesUrl}";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var s = _settings.Settings;
        s.GradeBand = GradeBox.SelectedIndex switch
        {
            0 => GradeBand.Grades12,
            1 => GradeBand.Grades34,
            _ => GradeBand.Grades56,
        };
        s.RequiredCorrect = int.Parse((string)RequiredBox.SelectedItem!);
        s.AutostartEnabled = AutostartBox.IsChecked == true;
        s.LockOnStartup = LockOnStartupBox.IsChecked == true;
        s.LockOnResume = LockOnResumeBox.IsChecked == true;
        s.ShowHints = ShowHintsBox.IsChecked == true;
        _settings.Save();

        _tasks.Configure(s.GradeBand);
        if (Environment.ProcessPath is { } exePath)
            AutoStartService.Sync(s.AutostartEnabled, exePath);

        Close();
    }

    private void Donate_Click(object sender, RoutedEventArgs e) =>
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AppLinks.DonateUrl) { UseShellExecute = true });

    private void Issues_Click(object sender, RoutedEventArgs e) =>
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AppLinks.IssuesUrl) { UseShellExecute = true });
}
