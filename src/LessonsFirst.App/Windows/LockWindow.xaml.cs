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
/// Неверный ответ (любой предмет) оставляет то же задание для повторной попытки
/// и показывает подсказку о способе решения; задание с ошибкой больше не
/// зачитывается (даже после верного ответа) и добавляет одну задачу к цели:
/// сколько заданий не удалось решить с первого раза — столько дополнительных.
/// Верный ответ без ошибок увеличивает прогресс ровно один раз (один TaskId —
/// один зачёт); пояснение шаблона (проверочное слово) показывается после
/// верного ответа. Некорректный ввод подсказывает формат и не считается ошибкой.
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
    private DateTime _lockedAt;
    private bool _answeringLocked;   // ждём перехода к следующему заданию
    private bool _unlocked;

    // Выбранное в формах выбора (SingleChoice / TokenSelection / BoundarySelection)
    private string? _selectedOptionId;
    private readonly HashSet<string> _selectedTokenIds = new();
    private readonly HashSet<string> _selectedBoundaryIds = new();
    private readonly List<Button> _optionButtons = new();
    private readonly Dictionary<string, Button> _tokenButtons = new();
    private readonly Dictionary<string, Button> _boundaryButtons = new();

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

        // Границы в DIP до первой отрисовки — чтобы окно не мигало в дефолтном размере.
        var (px, py, pw, ph) = Win32.GetPrimaryScreenBounds();
        var scale = System.Windows.Media.VisualTreeHelper.GetDpi(this).PixelsPerDip;
        Left = px / scale;
        Top = py / scale;
        Width = pw / scale;
        Height = ph / scale;

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
        if (!_unlocked && _current is { Form: AnswerForm.Integer or AnswerForm.Decimal or AnswerForm.ShortText })
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
        _lockedAt = DateTime.Now;
        _unlocked = false;
        _progress = new SessionProgress(_settings.Settings.RequiredCorrect);
        // Уровень и предметный режим фиксируются на начало сессии.
        _tasks.Configure(_settings.Settings.GradeBand, _settings.Settings.SubjectMode);
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
        ShowTask(_tasks.Next());
    }

    /// <summary>Визуальная проверка (harness): показать конкретное задание,
    /// не трогая прогресс и генераторы. Только для тестов рендера.</summary>
    internal void ShowReviewTask(SchoolTask task) => ShowTask(task);

    private void ShowTask(SchoolTask task)
    {
        _current = task;
        _answeringLocked = false;

        SubjectText.Text = _current.Subject;
        PromptText.Text = _current.Prompt;
        HintText.Text = _settings.Settings.ShowHints ? _current.Hint ?? "" : "";
        FeedbackText.Text = "";
        FeedbackText.Foreground = (Brush)FindResource("PaperBrush");
        ProgressText.Text = $"Верных ответов: {_progress.Correct} из {_progress.Required}";

        OptionsPanel.Children.Clear();
        ComparisonPanel.Children.Clear();
        TokensPanel.Children.Clear();
        _selectedOptionId = null;
        _selectedTokenIds.Clear();
        _selectedBoundaryIds.Clear();
        _optionButtons.Clear();
        _tokenButtons.Clear();
        _boundaryButtons.Clear();

        SinglePanel.Visibility = Visibility.Collapsed;
        FractionPanel.Visibility = Visibility.Collapsed;
        FractionHintText.Visibility = Visibility.Collapsed;
        PairPanel.Visibility = Visibility.Collapsed;
        OptionsPanel.Visibility = Visibility.Collapsed;
        ComparisonPanel.Visibility = Visibility.Collapsed;
        InputPanel.Visibility = Visibility.Collapsed;
        SelectionPanel.Visibility = Visibility.Collapsed;
        ChoiceConfirmPanel.Visibility = Visibility.Collapsed;

        switch (_current.Form)
        {
            case AnswerForm.Choice:
                OptionsPanel.Visibility = Visibility.Visible;
                ChoiceConfirmPanel.Visibility = Visibility.Visible;
                foreach (var option in _current.Options ?? Array.Empty<ChoiceOption>())
                {
                    var button = new Button
                    {
                        Content = option.Text,
                        FontSize = 26,
                        MinWidth = 150,
                        Height = 64,
                        Margin = new Thickness(8),
                        Tag = option.Id,
                    };
                    button.Click += Option_Click;
                    OptionsPanel.Children.Add(button);
                    _optionButtons.Add(button);
                }

                if (_optionButtons.Count > 0)
                {
                    Keyboard.Focus(_optionButtons[0]);
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
                AnswerBox.MaxLength = 20;
                AnswerBox.Text = string.Empty;
                Keyboard.Focus(AnswerBox);
                break;

            case AnswerForm.ShortText:
                InputPanel.Visibility = Visibility.Visible;
                SinglePanel.Visibility = Visibility.Visible;
                AnswerBox.MaxLength = Math.Clamp(_current.InputMaxLength, 1, 20);
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

            case AnswerForm.TokenSelection:
                SelectionPanel.Visibility = Visibility.Visible;
                BuildTokenButtons();
                if (_tokenButtons.TryGetValue("t0", out var firstToken))
                {
                    Keyboard.Focus(firstToken);
                }

                break;

            case AnswerForm.BoundarySelection:
                SelectionPanel.Visibility = Visibility.Visible;
                BuildBoundaryButtons();
                if (_boundaryButtons.TryGetValue("b0", out var firstBoundary))
                {
                    Keyboard.Focus(firstBoundary);
                }

                break;
        }
    }

    /// <summary>Слова предложения: выбор кликом/клавиатурой, порядок не важен.</summary>
    private void BuildTokenButtons()
    {
        foreach (var token in _current.Tokens ?? Array.Empty<TaskToken>())
        {
            var button = new Button
            {
                Content = token.Text,
                FontSize = 24,
                MinWidth = 72,
                Height = 58,
                Margin = new Thickness(6),
                Tag = token.TokenId,
            };
            button.Click += Token_Click;
            TokensPanel.Children.Add(button);
            _tokenButtons[token.TokenId] = button;
        }
    }

    /// <summary>Слова с кнопками-границами между ними: выбор мест для запятых.</summary>
    private void BuildBoundaryButtons()
    {
        var tokens = _current.Tokens ?? Array.Empty<TaskToken>();
        for (var i = 0; i < tokens.Count; i++)
        {
            var word = new TextBlock
            {
                Text = tokens[i].Text,
                FontSize = 26,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PaperBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2, 0, 2, 0),
            };
            TokensPanel.Children.Add(word);

            if (i + 1 < tokens.Count)
            {
                var gap = new Button
                {
                    Content = "␣",
                    FontSize = 20,
                    Width = 52,
                    Height = 44,
                    Margin = new Thickness(1),
                    Tag = $"b{i}",
                    ToolTip = "Поставить запятую здесь",
                };
                gap.Click += Boundary_Click;
                TokensPanel.Children.Add(gap);
                _boundaryButtons[$"b{i}"] = gap;
            }
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

        var optionId = (string)((Button)sender).Tag;
        _selectedOptionId = optionId;
        foreach (var button in _optionButtons)
        {
            Highlight(button, (string)button.Tag == optionId);
        }
    }

    private void Token_Click(object sender, RoutedEventArgs e)
    {
        if (_answeringLocked)
        {
            return;
        }

        var tokenId = (string)((Button)sender).Tag;
        if (!_selectedTokenIds.Add(tokenId))
        {
            _selectedTokenIds.Remove(tokenId);
        }

        foreach (var (id, button) in _tokenButtons)
        {
            Highlight(button, _selectedTokenIds.Contains(id));
        }
    }

    private void Boundary_Click(object sender, RoutedEventArgs e)
    {
        if (_answeringLocked)
        {
            return;
        }

        var boundaryId = (string)((Button)sender).Tag;
        if (!_selectedBoundaryIds.Add(boundaryId))
        {
            _selectedBoundaryIds.Remove(boundaryId);
        }

        foreach (var (id, button) in _boundaryButtons)
        {
            Highlight(button, _selectedBoundaryIds.Contains(id));
        }
    }

    private static void Highlight(Button button, bool selected)
    {
        button.Background = selected
            ? (Brush)Application.Current.FindResource("LimeBrush")
            : (Brush)Application.Current.FindResource("PeriwinkleBrush");
        button.Foreground = selected
            ? (Brush)Application.Current.FindResource("MidnightBrush")
            : (Brush)Application.Current.FindResource("PaperBrush");
    }

    private void SelectionSubmit_Click(object sender, RoutedEventArgs e) => SubmitSelection();

    private void ChoiceSubmit_Click(object sender, RoutedEventArgs e) => SubmitChoice();

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
            else if (ChoiceConfirmPanel.Visibility == Visibility.Visible)
            {
                SubmitChoice();
            }
            else if (SelectionPanel.Visibility == Visibility.Visible)
            {
                SubmitSelection();
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
            AnswerForm.ShortText => AnswerValidator.CheckShortText(
                AnswerBox.Text, _current.AcceptedAnswers!, _current.Policy ?? LetterPolicyDefault, _current.InputMaxLength),
            _ => AnswerCheckResult.InvalidFormat,
        };

        var formatHint = _current.Form switch
        {
            AnswerForm.Integer => "Введите целое число, например 12",
            AnswerForm.Decimal => "Введите десятичную дробь через запятую, например 7,2",
            AnswerForm.Fraction => "Введите числитель и знаменатель; знаменатель не может быть нулём",
            AnswerForm.FactorPair => "Введите два целых числа от 2 до 9",
            AnswerForm.ShortText => $"Впиши ответ русскими буквами (до {_current.InputMaxLength} знаков)",
            _ => null,
        };

        Evaluate(result, formatHint);
    }

    private static readonly AnswerPolicy LetterPolicyDefault = new() { CaseSensitive = false };

    /// <summary>SingleChoice: Enter или «Проверить» подтверждают текущий выбор.</summary>
    private void SubmitChoice()
    {
        if (_answeringLocked || ChoiceConfirmPanel.Visibility != Visibility.Visible)
        {
            return;
        }

        if (_selectedOptionId is null)
        {
            FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
            FeedbackText.Text = "Сначала выбери один вариант.";
            return;
        }

        Evaluate(AnswerValidator.CheckChoice(_selectedOptionId, _current.CorrectOptionId), formatHint: null);
    }

    /// <summary>Выделение слов или границ: Enter или «Проверить» подтверждают выбор.</summary>
    private void SubmitSelection()
    {
        if (_answeringLocked || SelectionPanel.Visibility != Visibility.Visible)
        {
            return;
        }

        if (_current.Form == AnswerForm.TokenSelection)
        {
            if (_selectedTokenIds.Count == 0)
            {
                FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
                FeedbackText.Text = "Сначала выбери слова: щёлкай по ним.";
                return;
            }

            Evaluate(
                AnswerValidator.CheckTokenSelection(_selectedTokenIds, _current.ExpectedTokenIds!),
                formatHint: "Выбери все подходящие слова");
            return;
        }

        // BoundarySelection: пустой набор — неверный ответ (целевая запятая всегда есть).
        Evaluate(
            AnswerValidator.CheckBoundarySelection(_selectedBoundaryIds, _current.ExpectedBoundaryIds!),
            formatHint: "Щёлкни по местам между словами, где нужны запятые");
    }

    private async void Evaluate(AnswerCheckResult result, string? formatHint)
    {
        if (_answeringLocked || _unlocked)
        {
            return;
        }

        var task = _current;

        switch (result)
        {
            case AnswerCheckResult.InvalidFormat:
                FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
                FeedbackText.Text = $"Некорректный ввод. {formatHint}";
                FocusFirstField();
                return; // прогресс не меняется, задание остаётся

            case AnswerCheckResult.Wrong:
                // Любой предмет: то же задание остаётся для повторной попытки, но уже
                // не зачтётся — цель выросла на одну задачу. Подсказка о способе решения.
                _progress.RegisterMistake(task.TaskId);
                FeedbackText.Foreground = (Brush)FindResource("DangerBrush");
                FeedbackText.Text = "✗ Неверно, попробуй ещё раз. Это задание уже не зачтётся — нужно будет решить ещё одно. Подсказка сверху.";
                if (!string.IsNullOrEmpty(task.Hint))
                {
                    HintText.Text = task.Hint;
                }

                ProgressText.Text = $"Верных ответов: {_progress.Correct} из {_progress.Required}";
                FocusFirstField();
                return;
        }

        // Верный ответ: зачёт ровно один раз на TaskId; проваленное ранее — без зачёта.
        _answeringLocked = true;
        var counted = _progress.RegisterCorrect(task.TaskId);
        FeedbackText.Foreground = (Brush)FindResource("LimeBrush");
        FeedbackText.Text = _progress.Completed
            ? $"Отлично! Всё решено!{ExplanationSuffix(task)}"
            : counted
                ? $"✓ Верно!{ExplanationSuffix(task)}"
                : $"✓ Верно, но из-за ошибки задание не зачитывается.{ExplanationSuffix(task)}";
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

    private static string ExplanationSuffix(SchoolTask task) =>
        string.IsNullOrEmpty(task.Explanation) ? "" : $" {task.Explanation}";

    private void FocusFirstField()
    {
        switch (_current.Form)
        {
            case AnswerForm.Integer:
            case AnswerForm.Decimal:
            case AnswerForm.ShortText:
                Keyboard.Focus(AnswerBox);
                break;
            case AnswerForm.Fraction:
                Keyboard.Focus(NumeratorBox);
                break;
            case AnswerForm.FactorPair:
                Keyboard.Focus(FirstFactorBox);
                break;
            case AnswerForm.Choice:
                Keyboard.Focus(_optionButtons.FirstOrDefault(b => (string)b.Tag == _selectedOptionId) ?? _optionButtons.FirstOrDefault());
                break;
            case AnswerForm.TokenSelection:
                Keyboard.Focus(_tokenButtons.GetValueOrDefault("t0"));
                break;
            case AnswerForm.BoundarySelection:
                Keyboard.Focus(_boundaryButtons.GetValueOrDefault("b0"));
                break;
        }
    }

    /// <summary>Все задачи решены: показываем поздравление, замок снимем после его закрытия.</summary>
    private void ShowSuccessAndUnlock()
    {
        _unlocked = true;
        _hook.Uninstall();
        _focusGuard.Stop();

        var elapsed = DateTime.Now - _lockedAt;
        var success = new SuccessWindow
        {
            StatsText = { Text = $"Верных ответов: {_progress.Correct} из {_progress.Required} · Ошибок: {_progress.Mistakes} · Время: {elapsed.Minutes:00}:{elapsed.Seconds:00}" },
        };
        success.Closed += (_, _) => Unlocked?.Invoke();
        // Сначала показываем экран успеха (замок ещё закрывает экран), только потом прячем замок —
        // иначе между окнами мигает рабочий стол.
        success.Show();
        Hide();
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
                app.OpenSettings(pinVerified: true);
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
