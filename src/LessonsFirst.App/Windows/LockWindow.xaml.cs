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
/// Математика: неверный ответ оставляет то же задание; некорректный ввод
/// подсказывает формат; один TaskId засчитывается один раз.
/// </summary>
public partial class LockWindow : Window
{
    private readonly SettingsService _settings;
    private readonly AuthService _auth;
    private readonly TaskProvider _tasks;

    private readonly LowLevelKeyboardHook _hook = new();
    private readonly DispatcherTimer _focusGuard;
    private SessionProgress _progress = new(5);

    private SchoolTask _current = null!;
    private int _mistakes;
    private DateTime _lockedAt;
    private bool _answeringLocked;   // ждём перехода к следующему заданию
    private bool _unlocked;

    /// <summary>Замок снят (решены задачи или введён PIN родителя).</summary>
    public event Action? Unlocked;

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

        // Прячем окно из Alt+Tab (WS_EX_TOOLWINDOW).
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = Win32.GetWindowLong(hwnd, Win32.GWL_EXSTYLE);
        Win32.SetWindowLong(hwnd, Win32.GWL_EXSTYLE, exStyle | Win32.WS_EX_TOOLWINDOW);

        ApplyFullScreenBounds();
    }

    /// <summary>Растягивает замок на главный монитор (координаты могли измениться после сна).</summary>
    private void ApplyFullScreenBounds()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var (x, y, w, h) = Win32.GetPrimaryScreenBounds();
        Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, x, y, w, h, Win32.SWP_SHOWWINDOW);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        if (!_unlocked && _current is { Form: AnswerForm.Integer or AnswerForm.Decimal })
        {
            Keyboard.Focus(AnswerBox);
        }
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
        _mistakes = 0;
        _lockedAt = DateTime.Now;
        _unlocked = false;
        _progress = new SessionProgress(_settings.Settings.RequiredCorrect);
        _tasks.Configure(_settings.Settings.GradeBand);
        _hook.Install();
        _focusGuard.Start();
        PinPanel.Visibility = Visibility.Collapsed;
        PinHelpText.Visibility = Visibility.Collapsed;
        ApplyFullScreenBounds();
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
        HintText.Text = _settings.Settings.ShowHints ? _current.Hint ?? "" : "";
        FeedbackText.Text = "";
        FeedbackText.Foreground = (Brush)FindResource("PaperBrush");
        ProgressText.Text = $"Верных ответов: {_progress.Correct} из {_progress.Required}";

        OptionsPanel.Children.Clear();
        ComparisonPanel.Children.Clear();

        SinglePanel.Visibility = Visibility.Collapsed;
        FractionPanel.Visibility = Visibility.Collapsed;
        FractionHintText.Visibility = Visibility.Collapsed;
        PairPanel.Visibility = Visibility.Collapsed;
        OptionsPanel.Visibility = Visibility.Collapsed;
        ComparisonPanel.Visibility = Visibility.Collapsed;
        InputPanel.Visibility = Visibility.Collapsed;

        switch (_current.Form)
        {
            case AnswerForm.Choice:
                OptionsPanel.Visibility = Visibility.Visible;
                foreach (var option in _current.Options ?? Array.Empty<string>())
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

                break;

            case AnswerForm.Comparison:
                ComparisonPanel.Visibility = Visibility.Visible;
                AddComparisonButton("<", ComparisonRelation.Less);
                AddComparisonButton("=", ComparisonRelation.Equal);
                AddComparisonButton(">", ComparisonRelation.Greater);
                break;

            case AnswerForm.Integer:
            case AnswerForm.Decimal:
                InputPanel.Visibility = Visibility.Visible;
                SinglePanel.Visibility = Visibility.Visible;
                AnswerBox.Text = string.Empty;
                Keyboard.Focus(AnswerBox);
                break;

            case AnswerForm.Fraction:
                InputPanel.Visibility = Visibility.Visible;
                FractionPanel.Visibility = Visibility.Visible;
                FractionHintText.Visibility = Visibility.Visible;
                NumeratorBox.Text = string.Empty;
                DenominatorBox.Text = string.Empty;
                Keyboard.Focus(NumeratorBox);
                break;

            case AnswerForm.FactorPair:
                InputPanel.Visibility = Visibility.Visible;
                PairPanel.Visibility = Visibility.Visible;
                FirstFactorBox.Text = string.Empty;
                SecondFactorBox.Text = string.Empty;
                Keyboard.Focus(FirstFactorBox);
                break;
        }
    }

    private void AddComparisonButton(string symbol, ComparisonRelation relation)
    {
        var button = new Button
        {
            Content = symbol,
            FontSize = 30,
            Width = 120,
            Height = 68,
            Margin = new Thickness(10),
        };
        button.Click += (_, _) => SubmitRelation(relation);
        ComparisonPanel.Children.Add(button);
    }

    private void Option_Click(object sender, RoutedEventArgs e)
    {
        if (_answeringLocked)
        {
            return;
        }

        var option = (string)((Button)sender).Tag;
        Evaluate(AnswerValidator.CheckChoice(option, _current.CorrectOption), formatHint: null);
    }

    private void SubmitRelation(ComparisonRelation relation)
    {
        if (_answeringLocked)
        {
            return;
        }

        Evaluate(AnswerValidator.CheckComparison(relation, _current.ExpectedRelation), formatHint: null);
    }

    private void Submit_Click(object sender, RoutedEventArgs e) => Submit();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (PinPanel.Visibility == Visibility.Visible)
            {
                TryPin(openSettings: false);
            }
            else if (InputPanel.Visibility == Visibility.Visible)
            {
                Submit();
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Escape && PinPanel.Visibility == Visibility.Visible)
        {
            PinPanel.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
    }

    private void Submit()
    {
        if (_answeringLocked || InputPanel.Visibility != Visibility.Visible)
        {
            return;
        }

        var result = _current.Form switch
        {
            AnswerForm.Integer => AnswerValidator.CheckInteger(AnswerBox.Text, _current.ExpectedInteger),
            AnswerForm.Decimal => AnswerValidator.CheckDecimal(AnswerBox.Text, _current.ExpectedDecimalValue),
            AnswerForm.Fraction => AnswerValidator.CheckFraction(
                NumeratorBox.Text, DenominatorBox.Text, _current.ExpectedFraction),
            AnswerForm.FactorPair => AnswerValidator.CheckFactorPair(
                FirstFactorBox.Text, SecondFactorBox.Text, _current.ExpectedFactors),
            _ => AnswerCheckResult.InvalidFormat,
        };

        var formatHint = _current.Form switch
        {
            AnswerForm.Integer => "Введите целое число, например 12",
            AnswerForm.Decimal => "Введите десятичную дробь через запятую, например 7,2",
            AnswerForm.Fraction => "Введите числитель и знаменатель; знаменатель не может быть нулём",
            AnswerForm.FactorPair => "Введите два целых числа от 2 до 9",
            _ => null,
        };

        Evaluate(result, formatHint);
    }

    private async void Evaluate(AnswerCheckResult result, string? formatHint)
    {
        if (_answeringLocked || _unlocked)
        {
            return;
        }

        var task = _current;
        var isMath = task.Form != AnswerForm.Choice;

        switch (result)
        {
            case AnswerCheckResult.InvalidFormat:
                FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
                FeedbackText.Text = $"Некорректный ввод. {formatHint}";
                FocusFirstField();
                return; // прогресс не меняется, задание остаётся

            case AnswerCheckResult.Wrong:
                _mistakes++;
                FeedbackText.Foreground = (Brush)FindResource("DangerBrush");

                if (isMath)
                {
                    // Математика: тот же вопрос остаётся для повторной попытки, прогресс не меняется.
                    FeedbackText.Text = "✗ Неверно, попробуй ещё раз. Задание то же.";
                    FocusFirstField();
                    return;
                }

                // Русский язык: новая задача (существующее поведение).
                FeedbackText.Text = "✗ Неверно. Попробуй другую задачу!";
                _answeringLocked = true;
                await Task.Delay(900);
                if (!_unlocked)
                {
                    ShowNextTask();
                }

                return;
        }

        // Верный ответ: зачёт ровно один раз на TaskId.
        _answeringLocked = true;
        _progress.RegisterCorrect(task.TaskId);
        FeedbackText.Foreground = (Brush)FindResource("LimeBrush");
        FeedbackText.Text = _progress.Completed ? "Отлично! Всё решено!" : "✓ Верно!";
        ProgressText.Text = $"Верных ответов: {_progress.Correct} из {_progress.Required}";

        await Task.Delay(900);

        // Защита от отложенных событий старого задания.
        if (_unlocked || !ReferenceEquals(_current, task))
        {
            return;
        }

        if (_progress.Completed)
        {
            ShowSuccessAndUnlock();
        }
        else
        {
            ShowNextTask();
        }
    }

    private void FocusFirstField()
    {
        switch (_current.Form)
        {
            case AnswerForm.Integer:
            case AnswerForm.Decimal:
                Keyboard.Focus(AnswerBox);
                break;
            case AnswerForm.Fraction:
                Keyboard.Focus(NumeratorBox);
                break;
            case AnswerForm.FactorPair:
                Keyboard.Focus(FirstFactorBox);
                break;
        }
    }

    /// <summary>Все задачи решены: показываем поздравление, замок снимем после его закрытия.</summary>
    private void ShowSuccessAndUnlock()
    {
        _unlocked = true;
        _hook.Uninstall();
        _focusGuard.Stop();
        Hide();

        var elapsed = DateTime.Now - _lockedAt;
        var success = new SuccessWindow
        {
            StatsText = { Text = $"Верных ответов: {_progress.Correct} · Ошибок: {_mistakes} · Время: {elapsed.Minutes:00}:{elapsed.Seconds:00}" },
        };
        success.Closed += (_, _) => Unlocked?.Invoke();
        success.Show();
    }

    /// <summary>PIN родителя: замок снимается сразу, без поздравления.</summary>
    private void Unlock()
    {
        _unlocked = true;
        _hook.Uninstall();
        _focusGuard.Stop();
        Hide();
        Unlocked?.Invoke();
    }

    private void GuardFocus()
    {
        if (_unlocked || IsActive)
        {
            return;
        }

        var hwnd = new WindowInteropHelper(this).Handle;
        Win32.SetForegroundWindow(hwnd);
        Activate();
    }

    private void ParentLogin_Click(object sender, RoutedEventArgs e)
    {
        PinPanel.Visibility = Visibility.Visible;
        PinHelpText.Visibility = Visibility.Visible;
        PinBox.Clear();
        Keyboard.Focus(PinBox);
    }

    private void PinCancel_Click(object sender, RoutedEventArgs e)
    {
        PinPanel.Visibility = Visibility.Collapsed;
        PinHelpText.Visibility = Visibility.Collapsed;
    }

    private void PinUnlock_Click(object sender, RoutedEventArgs e) => TryPin(openSettings: false);

    private void PinSettings_Click(object sender, RoutedEventArgs e) => TryPin(openSettings: true);

    private async void TryPin(bool openSettings)
    {
        if (_auth.VerifyPin(PinBox.Password))
        {
            Unlock();
            if (openSettings && Application.Current is App app)
            {
                app.OpenSettings();
            }

            return;
        }

        FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
        FeedbackText.Text = "Неверный PIN";
        PinBox.Clear();
        await Task.Delay(400);
        Keyboard.Focus(PinBox);
    }
}
