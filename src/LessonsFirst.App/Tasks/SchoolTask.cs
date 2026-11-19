namespace LessonsFirst.App.Tasks;

/// <summary>
/// Одна школьная задача: либо ввод ответа с клавиатуры (математика),
/// либо выбор из вариантов (русский язык).
/// </summary>
public sealed class SchoolTask
{
    public required string Subject { get; init; }

    /// <summary>Текст задания, например «Сколько будет 7 × 8?» или «Выбери букву: к_рова».</summary>
    public required string Prompt { get; init; }

    public string? Hint { get; init; }

    /// <summary>true — ответ вводится текстом; false — выбор кнопки из <see cref="Options"/>.</summary>
    public bool IsTextInput { get; init; }

    /// <summary>Канонический правильный ответ (для ввода — строка, для выбора — текст варианта).</summary>
    public required string CorrectAnswer { get; init; }

    /// <summary>Варианты ответа (уже перемешаны). null для текстового ввода.</summary>
    public IReadOnlyList<string>? Options { get; init; }

    /// <summary>Ответ — дробь вида «3/8» (сравнение с упрощением).</summary>
    public bool IsFraction { get; init; }

    public bool CheckAnswer(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var value = input.Trim();

        if (Options is not null)
            return string.Equals(value, CorrectAnswer, StringComparison.OrdinalIgnoreCase);

        if (IsFraction)
            return TryParseFraction(value, out var num, out var den)
                && TryParseFraction(CorrectAnswer, out var cNum, out var cDen)
                && num * cDen == cNum * den;

        // Числовой ответ сравниваем как число, чтобы «07» == «7».
        if (int.TryParse(value, out var answer) && int.TryParse(CorrectAnswer, out var canonical))
            return answer == canonical;

        return string.Equals(value, CorrectAnswer, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseFraction(string value, out int numerator, out int denominator)
    {
        numerator = denominator = 0;
        var parts = value.Split('/', StringSplitOptions.TrimEntries);
        if (parts.Length == 2
            && int.TryParse(parts[0], out numerator)
            && int.TryParse(parts[1], out denominator)
            && denominator != 0)
            return true;

        return false;
    }
}
