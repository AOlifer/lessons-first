using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using LessonsFirst.App.Services;
using LessonsFirst.App.Tasks;
using LessonsFirst.App.Windows;

namespace LessonsFirst.App;

public partial class App : Application
{
    private const string MutexName = "LessonsFirst_SingleInstance_3F1D";

    private Mutex? _mutex;
    private SettingsService _settings = null!;
    private AuthService _auth = null!;
    private TaskProvider _tasks = null!;
    private UpdateService _updater = null!;
    private LockService _lockService = null!;
    private TrayIcon? _tray;
    private LockWindow? _lockWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mutex = new Mutex(true, MutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            // Уже запущено: единственный экземпляр должен показать замок.
            Shutdown();
            return;
        }

        UpdateService.CleanupOldFiles();

        _settings = new SettingsService();
        _auth = new AuthService(_settings);
        _tasks = new TaskProvider();
        _tasks.Configure(_settings.Settings.GradeBand);
        _updater = new UpdateService();
        _lockService = new LockService();
        _lockService.LockRequested += ShowLock;
        _lockService.Start();

        CreateTray();
        SyncAutostart();

        if (!_auth.HasPin)
        {
            var setup = new PinSetupWindow(_auth);
            setup.PinSet += () => Dispatcher.Invoke(ShowLock);
            setup.Show();
        }
        else
        {
            ShowLock();
        }

        _ = CheckForUpdatesAsync(notify: true);
    }

    private void CreateTray()
    {
        var menu = new ContextMenu();

        var lockNow = new MenuItem { Header = "Заблокировать сейчас" };
        lockNow.Click += (_, _) => ShowLock();

        var settingsItem = new MenuItem { Header = "Настройки…" };
        settingsItem.Click += (_, _) => OpenSettings();

        var exit = new MenuItem { Header = "Выход" };
        exit.Click += (_, _) => ExitApp();

        menu.Items.Add(lockNow);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(exit);

        _tray = new TrayIcon(menu);
        _tray.Clicked += ShowLock;
    }

    private void SyncAutostart()
    {
        if (Environment.ProcessPath is { } exePath)
            AutoStartService.Sync(_settings.Settings.AutostartEnabled, exePath);
    }

    public void ShowLock()
    {
        if (_lockWindow is null)
        {
            _lockWindow = new LockWindow(_settings, _auth, _tasks);
            _lockWindow.Show();
        }
        else
        {
            _lockWindow.ResetProgress();
            _lockWindow.Show();
            _lockWindow.Activate();
        }
    }

    private void OpenSettings()
    {
        var window = new SettingsWindow(_settings, _auth, _tasks, _updater) { Owner = null };
        window.Show();
        window.Activate();
    }

    private async Task CheckForUpdatesAsync(bool notify)
    {
        var last = _settings.Settings.LastUpdateCheckUtc;
        if (notify && DateTime.UtcNow - last < TimeSpan.FromHours(20))
            return;

        _settings.Settings.LastUpdateCheckUtc = DateTime.UtcNow;
        _settings.Save();

        var info = await _updater.CheckForUpdateAsync();
        if (info is null)
            return;

        _tray?.ShowBalloon(
            "Сначала уроки!",
            $"Доступна версия {info.Tag}. Обновите приложение: правый клик по иконке → Настройки → Проверить обновления.");
    }

    private void ExitApp()
    {
        _lockService.Dispose();
        _tray?.Dispose();
        _updater.Dispose();
        _mutex?.ReleaseMutex();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _lockService?.Dispose();
        _tray?.Dispose();
        base.OnExit(e);
    }
}
