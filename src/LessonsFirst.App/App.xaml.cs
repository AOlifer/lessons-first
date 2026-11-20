using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LessonsFirst.App.Interop;
using LessonsFirst.App.Services;
using LessonsFirst.App.Tasks;
using LessonsFirst.App.Tasks.MathTasks;
using LessonsFirst.App.Tasks.RussianTasks;
using LessonsFirst.App.Windows;
using Microsoft.Win32;

namespace LessonsFirst.App;

public partial class App : Application
{
    private const string MutexName = "LessonsFirst_SingleInstance_3F1D";
    private const string ShowLockEventName = "LessonsFirst_ShowLock_Event";

    private Mutex? _mutex;
    private EventWaitHandle? _showLockSignal;
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

        AppLog.Init();
        DispatcherUnhandledException += (_, args) =>
        {
            AppLog.Error("Dispatcher", args.Exception);
            args.Handled = true; // не даём упасть: замок обязан жить
        };
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppLog.Error("Task", args.Exception);
            args.SetObserved();
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            AppLog.Error("AppDomain", (args.ExceptionObject as Exception)!);

        // Ручная визуальная проверка экрана замка: LessonsFirst.exe --visual-review [папка].
        // Запускается до мьютекса одиночного экземпляра — это dev-режим без замка и трея.
        if (e.Args.FirstOrDefault(a => a.StartsWith("--visual-review", StringComparison.OrdinalIgnoreCase)) is { } visualArg)
        {
            RunVisualReview(visualArg.Split('=', 2) is { Length: 2 } parts ? parts[1] : null);
            return;
        }

        _mutex = new Mutex(true, MutexName, out var isFirstInstance);
        var launchedByWatchdog = e.Args.Contains("--watchdog", StringComparer.OrdinalIgnoreCase);
        if (!isFirstInstance)
        {
            if (!launchedByWatchdog)
            {
                // Уже запущено, но запустил человек: просим живой экземпляр показать замок.
                try
                {
                    if (EventWaitHandle.TryOpenExisting(ShowLockEventName, out var existing))
                    {
                        existing.Set();
                        existing.Dispose();
                    }
                }
                catch
                {
                    // Не удалось сигнализировать — просто выходим.
                }

                AppLog.Write("Повторный запуск — сигнализируем живому экземпляру и выходим.");
            }
            else
            {
                // Watchdog видит живой экземпляр — тишина, ничего не показываем.
                AppLog.Write("Watchdog: приложение уже работает — тихо выходим.");
            }

            Shutdown();
            return;
        }

        // Сигнал «покажи замок» от повторных ручных запусков.
        _showLockSignal = new EventWaitHandle(false, EventResetMode.AutoReset, ShowLockEventName);
        System.Threading.ThreadPool.RegisterWaitForSingleObject(
            _showLockSignal,
            (_, _) => Dispatcher.BeginInvoke(ShowLock),
            null, -1, executeOnlyOnce: false);

        _settings = new SettingsService();
        _auth = new AuthService(_settings);
        _tasks = new TaskProvider();
        _tasks.Configure(_settings.Settings.GradeBand, _settings.Settings.SubjectMode);
        _updater = new UpdateService();
        _lockService = new LockService(_settings);
        _lockService.LockRequested += ShowLock;
        _lockService.Start();
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

        CreateTray();
        SyncAutostart();

        // Родитель недавно вышел через трей — watchdog молча живёт в трее, без замка.
        var suppressStartupLock = launchedByWatchdog && WasRecentlyParentExit();

        if (!_auth.HasPin)
        {
            var setup = new PinSetupWindow(_auth);
            setup.PinSet += () => Dispatcher.Invoke(() =>
            {
                if (_settings.Settings.LockOnStartup)
                    ShowLock();
            });
            setup.Show();
        }
        else if (_settings.Settings.LockOnStartup && !suppressStartupLock)
        {
            ShowLock();
        }

        _ = CheckForUpdatesAsync(notify: true);

        // Подсказываем, где искать значок приложения (Win11 часто прячет его под стрелкой).
        var tray = _tray;
        if (tray is not null)
        {
            var dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            dispatcherTimer.Tick += (_, _) =>
            {
                dispatcherTimer.Stop();
                tray.ShowBalloon(
                    "Сначала уроки!",
                    "Приложение работает. Его значок — в области уведомлений у часов (может быть под стрелкой ↑).");
            };
            dispatcherTimer.Start();
        }
    }

    private void CreateTray()
    {
        var menu = new ContextMenu();

        var lockNow = new MenuItem { Header = "Заблокировать сейчас" };
        lockNow.Click += (_, _) => ShowLock();

        var settingsItem = new MenuItem { Header = "Настройки…" };
        settingsItem.Click += (_, _) => OpenSettings();

        var donate = new MenuItem { Header = "☕ Поддержать проект" };
        donate.Click += (_, _) => OpenLink(AppLinks.DonateUrl);

        var issues = new MenuItem { Header = "🐛 Сообщить о проблеме" };
        issues.Click += (_, _) => OpenLink(AppLinks.IssuesUrl);

        var exit = new MenuItem { Header = "Выход" };
        exit.Click += (_, _) => ExitApp();

        menu.Items.Add(lockNow);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(donate);
        menu.Items.Add(issues);
        menu.Items.Add(new Separator());
        menu.Items.Add(exit);

        _tray = new TrayIcon(menu);
        _tray.Clicked += ShowLock;
    }

    private static void OpenLink(string url) =>
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });

    private void SyncAutostart()
    {
        if (Environment.ProcessPath is { } exePath)
            AutoStartService.Sync(_settings.Settings.AutostartEnabled, exePath);
    }

    /// <summary>Метка «родитель вышел через трей»: watchdog в течение 15 минут не возвращает замок.</summary>
    private static string ParentExitedFlagPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LessonsFirst", "parent-exited.flag");

    private static bool WasRecentlyParentExit()
    {
        try
        {
            return File.Exists(ParentExitedFlagPath)
                && DateTime.UtcNow - File.GetLastWriteTimeUtc(ParentExitedFlagPath) < TimeSpan.FromMinutes(15);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ручная визуальная проверка (запускается с аргументом --visual-review):
    /// рендерит экран замка со всеми формами ответов русского модуля
    /// (и одним математическим заданием) в PNG-файлы и завершает приложение.
    /// </summary>
    private void RunVisualReview(string? outDir)
    {
        outDir ??= Path.Combine(Path.GetTempPath(), "lf-visual-" + DateTime.Now.ToString("MMdd-HHmmss"));
        Directory.CreateDirectory(outDir);
        AppLog.Write($"Визуальная проверка: {outDir}");

        var settings = new SettingsService();
        var tasks = new TaskProvider(new Random(20261120));
        tasks.Configure(settings.Settings.GradeBand, settings.Settings.SubjectMode);

        var rng = new Random(5);
        var window = new LockWindow(settings, new AuthService(settings), tasks);
        window.Show();
        PumpUi();

        CaptureWindow(window, Path.Combine(outDir, "00-mixed-default.png"));

        var shots = new List<(string Name, SchoolTask Task)>
        {
            ("10-r12-integer-count", BuildReviewTask(RussianTemplates12.All, "Russian.R12-01", rng)),
            ("11-r12-shorttext-zhishi", BuildReviewTask(RussianTemplates12.All, "Russian.R12-05", rng)),
            ("12-r12-choice-case-sensitive", BuildReviewTask(RussianTemplates12.All, "Russian.R12-08", rng)),
            ("13-r12-choice-wordclasses", BuildReviewTask(RussianTemplates12.All, "Russian.R12-10", rng)),
            ("20-r34-shorttext-unstressed", BuildReviewTask(RussianTemplates34.All, "Russian.R34-01", rng)),
            ("21-r34-tokenselection-pos", BuildReviewTask(RussianTemplates34.All, "Russian.R34-05", rng)),
            ("22-r34-tokenselection-subject", BuildReviewTask(RussianTemplates34.All, "Russian.R34-10", rng)),
            ("23-r34-choice-case-six", BuildReviewTask(RussianTemplates34.All, "Russian.R34-09", rng)),
            ("30-r56-shorttext-wordform", BuildReviewTask(RussianTemplates56.All, "Russian.R56-01", rng)),
            ("31-r56-shorttext-double-n", BuildReviewTask(RussianTemplates56.All, "Russian.R56-07", rng)),
            ("32-r56-boundary-homogeneous", BuildReviewTask(RussianTemplates56.All, "Russian.R56-08", rng)),
            ("33-r56-boundary-address", BuildReviewTask(RussianTemplates56.All, "Russian.R56-09", rng)),
            ("34-r56-choice-features", BuildReviewTask(RussianTemplates56.All, "Russian.R56-10", rng)),
            ("40-math", new MathEngine(rng, GradeBand.Grades34).Next()),
        };

        foreach (var (name, task) in shots)
        {
            window.ShowReviewTask(task);
            PumpUi();

            // Имитируем выбор для проверки подсветки выделенных элементов.
            if (name == "12-r12-choice-case-sensitive")
            {
                ClickButtons(window.OptionsPanel, 1);
            }
            else if (name == "21-r34-tokenselection-pos")
            {
                ClickButtons(window.TokensPanel, 2);
            }
            else if (name == "32-r56-boundary-homogeneous" || name == "33-r56-boundary-address")
            {
                ClickButtons(window.TokensPanel, 1);
            }

            PumpUi();
            CaptureWindow(window, Path.Combine(outDir, name + ".png"));
        }

        AppLog.Write($"Визуальная проверка завершена: {Directory.GetFiles(outDir, "*.png").Length} снимков.");
        Shutdown();
    }

    /// <summary>Кликает первые count кнопок в панели (имитация выбора ребёнком).</summary>
    private static void ClickButtons(System.Windows.Controls.Panel panel, int count)
    {
        foreach (var button in FindDescendants<Button>(panel).Take(count))
        {
            button.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent, button));
        }
    }

    private static IEnumerable<T> FindDescendants<T>(System.Windows.DependencyObject root)
        where T : System.Windows.DependencyObject
    {
        if (root is T match)
        {
            yield return match;
        }

        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            foreach (var child in FindDescendants<T>(System.Windows.Media.VisualTreeHelper.GetChild(root, i)))
            {
                yield return child;
            }
        }
    }

    private static SchoolTask BuildReviewTask(IReadOnlyList<IRussianTemplate> all, string id, Random rng)
    {
        var template = all.Single(t => t.Id == id);
        return template.Build(rng, out _);
    }

    private static void PumpUi()
    {
        for (var i = 0; i < 3; i++)
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }
    }

    private static void CaptureWindow(Window window, string path)
    {
        window.UpdateLayout();
        var width = (int)Math.Max(400, window.ActualWidth);
        var height = (int)Math.Max(300, window.ActualHeight);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    public void ShowLock()
    {
        try
        {
            if (File.Exists(ParentExitedFlagPath))
                File.Delete(ParentExitedFlagPath);
        }
        catch
        {
            // Не критично.
        }

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

    public void OpenSettings(bool pinVerified = false)
    {
        var window = new SettingsWindow(_settings, _auth, _tasks, _updater, pinVerified) { Owner = null };
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
        try
        {
            // Запоминаем осознанный выход: watchdog не должен сразу возвращать замок.
            Directory.CreateDirectory(Path.GetDirectoryName(ParentExitedFlagPath)!);
            File.WriteAllText(ParentExitedFlagPath, DateTime.UtcNow.ToString("O"));
        }
        catch
        {
            // Не критично.
        }

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
