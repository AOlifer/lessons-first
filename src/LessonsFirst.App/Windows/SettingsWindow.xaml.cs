using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LessonsFirst.App.Services;
using LessonsFirst.App.Tasks;

namespace LessonsFirst.App.Windows;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settings;
    private readonly AuthService _auth;
    private readonly TaskProvider _tasks;
    private readonly UpdateService _updater;
    private UpdateInfo? _pendingUpdate;

    public SettingsWindow(SettingsService settings, AuthService auth, TaskProvider tasks, UpdateService updater)
    {
        InitializeComponent();
        _settings = settings;
        _auth = auth;
        _tasks = tasks;
        _updater = updater;

        GradeBox.ItemsSource = new[]
        {
            "1–2 класс",
            "3–4 класс",
            "5–6 класс",
        };
        RequiredBox.ItemsSource = new[] { "3", "5", "10", "15" };
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

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

        PinGatePanel.Visibility = Visibility.Collapsed;
        SettingsPanel.Visibility = Visibility.Visible;
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
        _settings.Save();

        _tasks.Configure(s.GradeBand);
        if (Environment.ProcessPath is { } exePath)
            AutoStartService.Sync(s.AutostartEnabled, exePath);

        Close();
    }
}
