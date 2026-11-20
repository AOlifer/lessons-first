namespace LessonsFirst.App.Tasks;

/// <summary>Форма ответа: определяет интерфейс ввода на экране задания.</summary>
public enum AnswerForm
{
    /// <summary>Выбор одного варианта (SingleChoice; русский язык).</summary>
    Choice,

    /// <summary>Целое число в одном поле.</summary>
    Integer,

    /// <summary>Десятичная дробь в одном поле (запятая или точка).</summary>
    Decimal,

    /// <summary>Обыкновенная дробь: отдельные поля числителя и знаменателя.</summary>
    Fraction,

    /// <summary>Пара целых множителей: два поля, порядок не важен.</summary>
    FactorPair,

    /// <summary>Сравнение: кнопки &lt;, =, &gt;.</summary>
    Comparison,

    /// <summary>Короткий текст: буква, фрагмент (н/нн) или одно слово (русский язык).</summary>
    ShortText,

    /// <summary>Выделение слов в предложении: точное множество TokenId.</summary>
    TokenSelection,

    /// <summary>Выбор границ между словами (запятые): точное множество BoundaryId.</summary>
    BoundarySelection,
}

/// <summary>Отношение сравнения.</summary>
public enum ComparisonRelation
{
    Less,
    Equal,
    Greater,
}

/// <summary>Часть речи токена в конструкциях русского модуля.</summary>
public enum PartOfSpeech
{
    None,
    Noun,
    Adjective,
    Verb,
}

/// <summary>Вариант выбора с постоянным идентификатором: порядок перемешивается,
/// правильность определяется по Id, а не по позиции или тексту.</summary>
public sealed record ChoiceOption(string Id, string Text);

/// <summary>Слово предложения с контекстной разметкой части речи и роли.</summary>
public sealed record TaskToken(string TokenId, string Text, PartOfSpeech Pos, string Role);

/// <summary>Граница между соседними токенами: место возможной запятой.
/// Привязана к индексу токена, после которого стоит, а не к позиции в строке.</summary>
public sealed record BoundaryGap(string BoundaryId, int AfterTokenIndex);

/// <summary>
/// Политика нормализации русского ответа. NFC; внешние пробелы убираются;
/// чувствительность к регистру и различение е/ё задаются шаблоном явно;
/// знак ударения U+0301 — не буква и может игнорироваться только в ShortText.
/// </summary>
public sealed record AnswerPolicy
{
    /// <summary>Регистр значим (R12-08/09 — заглавная буква).</summary>
    public required bool CaseSensitive { get; init; }

    /// <summary>Различать е и ё. По умолчанию — да; в заданиях на буквы и орфографию — обязательно.</summary>
    public bool YoDistinct { get; init; } = true;

    /// <summary>Допустить знак ударения U+0301: не буква, игнорируется при сравнении (только ShortText).</summary>
    public bool IgnoreStressMark { get; init; }
}

/// <summary>
/// Задание для экрана замка. Единая модель для математики и русского языка.
/// Математические задания содержат структурированный ожидаемый ответ
/// (целое / decimal / дробь / пара множителей / отношение) — без разбора текста.
/// Русские задания добавляют ShortText (набор допустимых ответов + политика),
/// SingleChoice (варианты с Id), TokenSelection и BoundarySelection.
/// </summary>
public sealed record SchoolTask
{
    public required string Subject { get; init; }

    /// <summary>Уровень сложности экземпляра (фиксируется движком предмета).</summary>
    public GradeBand Difficulty { get; init; } = GradeBand.Grades34;

    public required string Prompt { get; init; }

    /// <summary>Тематическая подсказка (способ решения, без готового ответа).</summary>
    public string? Hint { get; init; }

    /// <summary>Пояснение после верного ответа (проверочное слово, восстановленное слово).</summary>
    public string? Explanation { get; init; }

    /// <summary>Идентификатор шаблона («A1»…«C12», «Russian.R12-01»…«Russian.R56-10»).</summary>
    public string TemplateId { get; init; } = "";

    /// <summary>Уникальный идентификатор экземпляра: один TaskId засчитывается один раз.</summary>
    public Guid TaskId { get; init; } = Guid.NewGuid();

    public AnswerForm Form { get; init; }

    // —— SingleChoice: варианты с постоянными Id ——
    public IReadOnlyList<ChoiceOption>? Options { get; init; }

    public string? CorrectOptionId { get; init; }

    /// <summary>Политика сравнения ответа (регистр, е/ё, знак ударения).</summary>
    public AnswerPolicy? Policy { get; init; }

    // —— ShortText: полный набор нормативно допустимых ответов ——
    public IReadOnlyList<string>? AcceptedAnswers { get; init; }

    /// <summary>Максимальная длина ввода для ShortText.</summary>
    public int InputMaxLength { get; init; } = 20;

    // —— TokenSelection: слова предложения ——
    public IReadOnlyList<TaskToken>? Tokens { get; init; }

    public IReadOnlySet<string>? ExpectedTokenIds { get; init; }

    // —— BoundarySelection: границы между словами для запятых ——
    public IReadOnlyList<BoundaryGap>? Boundaries { get; init; }

    public IReadOnlySet<string>? ExpectedBoundaryIds { get; init; }

    // —— Структурированные ожидаемые ответы (математика) ——
    public int ExpectedInteger { get; init; }

    public decimal ExpectedDecimalValue { get; init; }

    /// <summary>Ожидаемая дробь в несократимом виде, знаменатель положительный.</summary>
    public (int Numerator, int Denominator) ExpectedFraction { get; init; }

    public (int First, int Second) ExpectedFactors { get; init; }

    public ComparisonRelation ExpectedRelation { get; init; }
}
