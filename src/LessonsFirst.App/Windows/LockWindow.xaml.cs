using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using LessonsFirst.App.Interop;
using LessonsFirst.App.Services;
using LessonsFirst.App.Tasks;

namespace LessonsFirst.App.Windows;

/// <summary>
/// Полноэкранный несворачиваемый замок: закрывает все мониторы, глушит системные
/// горячие клавиши, требует N правильных ответов или PIN родителя.
/// </summary>
public partial class LockWindow : Window
{
    private readonly SettingsService _settings;
    private readonly AuthService _auth;
    private readonly TaskProvider _tasks;

    private readonly LowLevelKeyboardHook _hook = new();
    private readonly DispatcherTimer _focusGuard;

    private SchoolTask _current = null!;
    private int _correctCount;
    private bool _answeringLocked;   // ждём показа следующей задачи
    private bool _unlocked;

    public LockWindow(SettingsService settings, AuthService auth, TaskProvider tasks)
    {
        _settings = settings;
        _auth = auth;
        _tasks = tasks;

        InitializeComponent();

        _focusGuard = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };
        _focusGuard.Tick += (_, _) => GuardFocus();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;

        // Прячем окно из Alt+Tab (WS_EX_TOOLWINDOW) и растягиваем на весь виртуальный экран.
        var exStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);
        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE, exStyle | Win32.WS_EX_TOOLWINDOW);

        var (x, y, w, h) = Win32.GetVirtualScreenBounds();
        Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, x, y, w, h, Win32.SWP_SHOWWINDOW);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        if (!_unlocked && _current is { IsTextInput: true })
            Keyboard.Focus(AnswerBox);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_unlocked)
        {
            // Замок нельзя закрыть — ни Alt+F4 (глушим хуком), ни программно.
            e.Cancel = true;
            return;
        }

        _hook.Uninstall();
        _focusGuard.Stop();
        base.OnClosing(e);
    }

    public void ResetProgress()
    {
        _correctCount = 0;
        _unlocked = false;
        _tasks.Configure(_settings.Settings.GradeBand);
        _hook.Install();
        _focusGuard.Start();
        PinPanel.Visibility = Visibility.Collapsed;
        ShowNextTask();
        GuardFocus();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        ResetProgress();
    }

    private void ShowNextTask()
    {
        _current = _tasks.Next();
        _answeringLocked = false;

        SubjectText.Text = _current.Subject;
        PromptText.Text = _current.Prompt;
        HintText.Text = _current.Hint ?? "";
        FeedbackText.Text = "";
        FeedbackText.Foreground = Brushes.White;
        ProgressText.Text = $"Правильных ответов: {_correctCount} из {_settings.Settings.RequiredCorrect}";

        OptionsPanel.Children.Clear();
        if (_current.Options is { Count: > 0 })
        {
            InputPanel.Visibility = Visibility.Collapsed;
            OptionsPanel.Visibility = Visibility.Visible;
            foreach (var option in _current.Options)
            {
                var button = new Button
                {
                    Content = option,
                    FontSize = 26,
                    MinWidth = 150,
                    Height = 64,
                    Margin = new Thickness(8),
                    Tag = option,
                };
                button.Click += Option_Click;
                OptionsPanel.Children.Add(button);
            }
        }
        else
        {
            OptionsPanel.Visibility = Visibility.Collapsed;
            InputPanel.Visibility = Visibility.Visible;
            AnswerBox.Text = string.Empty;
            Keyboard.Focus(AnswerBox);
        }
    }

    private void Option_Click(object sender, RoutedEventArgs e)
    {
        if (_answeringLocked)
            return;
        var option = (string)((Button)sender).Tag;
        Check(option);
    }

    private void Submit_Click(object sender, RoutedEventArgs e) => SubmitAnswer();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (PinPanel.Visibility == Visibility.Visible)
            {
                TryPin();
            }
            else if (InputPanel.Visibility == Visibility.Visible)
            {
                SubmitAnswer();
            }

            e.Handled = true;
        }
    }

    private void SubmitAnswer()
    {
        if (_answeringLocked || InputPanel.Visibility != Visibility.Visible)
            return;
        Check(AnswerBox.Text);
    }

    private async void Check(string answer)
    {
        if (_answeringLocked)
            return;

        _answeringLocked = true;
        var correct = _current.CheckAnswer(answer);

        if (correct)
        {
            _correctCount++;
            FeedbackText.Foreground = (Brush)FindResource("LimeBrush");
            FeedbackText.Text = _correctCount >= _settings.Settings.RequiredCorrect
                ? "Отлично! Всё решено!"
                : "✓ Верно!";
        }
        else
        {
            FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
            FeedbackText.Text = "✗ Неверно. Попробуй другую задачу!";
        }

        await Task.Delay(900);

        if (_correctCount >= _settings.Settings.RequiredCorrect)
        {
            Unlock();
        }
        else if (!_unlocked)
        {
            ShowNextTask();
        }
    }

    private void Unlock()
    {
        _unlocked = true;
        _hook.Uninstall();
        _focusGuard.Stop();
        Hide();
    }

    private void GuardFocus()
    {
        if (_unlocked || IsActive)
            return;

        var hwnd = new WindowInteropHelper(this).Handle;
        Win32.SetForegroundWindow(hwnd);
        Activate();
    }

    private void ParentLogin_Click(object sender, RoutedEventArgs e)
    {
        PinPanel.Visibility = Visibility.Visible;
        PinBox.Clear();
        Keyboard.Focus(PinBox);
    }

    private void PinCancel_Click(object sender, RoutedEventArgs e)
    {
        PinPanel.Visibility = Visibility.Collapsed;
    }

    private void PinUnlock_Click(object sender, RoutedEventArgs e) => TryPin();

    private async void TryPin()
    {
        if (_auth.VerifyPin(PinBox.Password))
        {
            Unlock();
            return;
        }

        FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
        FeedbackText.Text = "Неверный PIN";
        PinBox.Clear();
        await Task.Delay(400);
        Keyboard.Focus(PinBox);
    }
}
