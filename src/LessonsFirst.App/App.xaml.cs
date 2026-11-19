using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using LessonsFirst.App.Interop;
using LessonsFirst.App.Services;
using LessonsFirst.App.Tasks;
using LessonsFirst.App.Windows;
using Microsoft.Win32;

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
    private readonly List<CoverWindow> _covers = new();
    private List<Win32.MonitorBounds> _secondaryMonitors = new();
    private string _coverSignature = "";

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
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

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
        EnsureCovers();
        ShowCovers();

        if (_lockWindow is null)
        {
            _lockWindow = new LockWindow(_settings, _auth, _tasks);
            _lockWindow.Unlocked += HideCovers;
            _lockWindow.Show();
        }
        else
        {
            _lockWindow.ResetProgress();
            _lockWindow.Show();
            _lockWindow.Activate();
        }
    }

    /// <summary>Пересоздаёт заглушки, если состав/расположение дополнительных мониторов изменился.</summary>
    private void EnsureCovers()
    {
        var secondary = Win32.GetMonitors().Where(m => !m.IsPrimary).ToList();
        var signature = string.Join("|", secondary.Select(m => $"{m.X},{m.Y},{m.Width},{m.Height}"));
        if (signature == _coverSignature)
            return;

        foreach (var cover in _covers)
            cover.AllowClose();
        _covers.Clear();

        _secondaryMonitors = secondary;
        _coverSignature = signature;
        foreach (var _ in secondary)
            _covers.Add(new CoverWindow());
    }

    private void ShowCovers()
    {
        for (var i = 0; i < _covers.Count && i < _secondaryMonitors.Count; i++)
        {
            var m = _secondaryMonitors[i];
            _covers[i].ShowCover(m.X, m.Y, m.Width, m.Height);
        }
    }

    private void HideCovers()
    {
        foreach (var cover in _covers)
            cover.HideCover();
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _coverSignature = ""; // форсируем пересборку под новую топологию
            EnsureCovers();
            if (_lockWindow is { IsVisible: true })
                ShowCovers();
        });
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
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        foreach (var cover in _covers)
            cover.AllowClose();
        _covers.Clear();
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
