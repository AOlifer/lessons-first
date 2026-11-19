namespace LessonsFirst.App.Tasks;

/// <summary>Форма ответа: определяет интерфейс ввода на экране задания.</summary>
public enum AnswerForm
{
    /// <summary>Выбор из вариантов (русский язык).</summary>
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
}

/// <summary>Отношение сравнения.</summary>
public enum ComparisonRelation
{
    Less,
    Equal,
    Greater,
}

/// <summary>
/// Задание для экрана замка. Единая модель для математики и русского языка.
/// Математические задания содержат структурированный ожидаемый ответ
/// (целое / decimal / дробь / пара множителей / отношение) — без разбора текста.
/// </summary>
public sealed class SchoolTask
{
    public required string Subject { get; init; }

    public required string Prompt { get; init; }

    /// <summary>Тематическая подсказка (способ решения, без готового ответа).</summary>
    public string? Hint { get; init; }

    /// <summary>Идентификатор шаблона («A1»…«C12») либо «RU» для русского языка.</summary>
    public string TemplateId { get; init; } = "";

    /// <summary>Уникальный идентификатор экземпляра: один TaskId засчитывается один раз.</summary>
    public Guid TaskId { get; init; } = Guid.NewGuid();

    public AnswerForm Form { get; init; }

    // —— Варианты выбора (русский язык) ——
    public IReadOnlyList<string>? Options { get; init; }

    public string? CorrectOption { get; init; }

    // —— Структурированные ожидаемые ответы (математика) ——
    public int ExpectedInteger { get; init; }

    public decimal ExpectedDecimalValue { get; init; }

    /// <summary>Ожидаемая дробь в несократимом виде, знаменатель положительный.</summary>
    public (int Numerator, int Denominator) ExpectedFraction { get; init; }

    public (int First, int Second) ExpectedFactors { get; init; }

    public ComparisonRelation ExpectedRelation { get; init; }
}
