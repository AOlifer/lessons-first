using System.Globalization;
using System.Text;

namespace LessonsFirst.App.Tasks;

/// <summary>Результат проверки ответа.</summary>
public enum AnswerCheckResult
{
    /// <summary>Ответ разобран и верен.</summary>
    Correct,

    /// <summary>Ответ разобран, но неверен.</summary>
    Wrong,

    /// <summary>Ответ не удалось разобрать — подсказать формат, прогресс не меняется.</summary>
    InvalidFormat,
}

/// <summary>
/// Разбор и проверка ответов всех форм. Всё локально, точно (decimal/целые),
/// без eval и без вычисления введённых выражений.
/// Русские текстовые ответы: NFC-нормализация, внешние пробелы, регистр и е/ё —
/// по AnswerPolicy шаблона; внутренние пробелы и пунктуация не удаляются;
/// похожие латинские буквы русскими не считаются; й и и не объединяются;
/// знак ударения U+0301 — не буква.
/// </summary>
public static class AnswerValidator
{
    private const int MaxInputLength = 20;

    /// <summary>Целое число: обычный и математический минус, ведущие нули, пробелы по краям.</summary>
    public static AnswerCheckResult CheckInteger(string? input, int expected)
    {
        if (!TryParseInteger(input, out var value))
            return AnswerCheckResult.InvalidFormat;
        return value == expected ? AnswerCheckResult.Correct : AnswerCheckResult.Wrong;
    }

    /// <summary>Десятичная дробь: запятая или точка; сравнение строго через decimal.</summary>
    public static AnswerCheckResult CheckDecimal(string? input, decimal expected)
    {
        if (!TryParseDecimal(input, out var value))
            return AnswerCheckResult.InvalidFormat;
        return value == expected ? AnswerCheckResult.Correct : AnswerCheckResult.Wrong;
    }

    /// <summary>Обыкновенная дробь: любые эквивалентные записи, знаменатель положительный.</summary>
    public static AnswerCheckResult CheckFraction(string? numeratorInput, string? denominatorInput, (int Numerator, int Denominator) expected)
    {
        if (!TryParseInteger(numeratorInput, out var numerator)
            || !TryParseInteger(denominatorInput, out var denominator))
            return AnswerCheckResult.InvalidFormat;

        if (denominator == 0)
            return AnswerCheckResult.InvalidFormat;

        // Точное перекрёстное умножение (значения ограничены длиной ввода).
        return (long)numerator * expected.Denominator == (long)expected.Numerator * denominator
            ? AnswerCheckResult.Correct
            : AnswerCheckResult.Wrong;
    }

    /// <summary>Пара множителей: порядок не важен; диапазон 2…9 и произведение.</summary>
    public static AnswerCheckResult CheckFactorPair(string? firstInput, string? secondInput, (int First, int Second) expected)
    {
        if (!TryParseInteger(firstInput, out var first)
            || !TryParseInteger(secondInput, out var second))
            return AnswerCheckResult.InvalidFormat;

        const int min = 2;
        const int max = 9;
        if (first is < min or > max || second is < min or > max)
            return AnswerCheckResult.Wrong;

        return first * second == expected.First * expected.Second
            ? AnswerCheckResult.Correct
            : AnswerCheckResult.Wrong;
    }

    /// <summary>Сравнение: нажатая кнопка против ожидаемого отношения.</summary>
    public static AnswerCheckResult CheckComparison(ComparisonRelation given, ComparisonRelation expected) =>
        given == expected ? AnswerCheckResult.Correct : AnswerCheckResult.Wrong;

    /// <summary>Вариант выбора (SingleChoice): правильность по идентификатору варианта.</summary>
    public static AnswerCheckResult CheckChoice(string? selectedOptionId, string? correctOptionId) =>
        string.Equals(selectedOptionId, correctOptionId, StringComparison.Ordinal)
            ? AnswerCheckResult.Correct
            : AnswerCheckResult.Wrong;

    /// <summary>
    /// ShortText: одна буква, фрагмент (н/нн) или одно слово.
    /// Сравнение по полному набору допустимых ответов после нормализации —
    /// без приблизительного сходства. Пустой или слишком длинный ввод —
    /// некорректный формат (без исключений); латиница и посторонние символы —
    /// неверный ответ.
    /// </summary>
    public static AnswerCheckResult CheckShortText(string? input, IReadOnlyList<string> accepted, AnswerPolicy policy, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            return AnswerCheckResult.InvalidFormat;

        // Сначала нормализация (NFC, знак ударения): длина считается по нормализованному
        // тексту, чтобы знак ударения не делал верный ответ «слишком длинным».
        var normalized = NormalizeRussian(input.Trim(), policy);
        if (normalized.Length == 0)
            return AnswerCheckResult.InvalidFormat;

        if (normalized.Length > Math.Max(1, maxLength) || normalized.Length > 40)
            return AnswerCheckResult.InvalidFormat;

        if (!normalized.All(IsCyrillicLetter))
            return AnswerCheckResult.Wrong;

        foreach (var candidate in accepted)
        {
            if (string.Equals(normalized, NormalizeRussian(candidate, policy), StringComparison.Ordinal))
                return AnswerCheckResult.Correct;
        }

        return AnswerCheckResult.Wrong;
    }

    /// <summary>Выделение слов: точное множество TokenId; порядок выбора не важен.
    /// Пустой выбор — некорректный ввод, а не языковая ошибка.</summary>
    public static AnswerCheckResult CheckTokenSelection(IReadOnlyCollection<string> selectedTokenIds, IReadOnlySet<string> expectedTokenIds)
    {
        if (selectedTokenIds.Count == 0)
            return AnswerCheckResult.InvalidFormat;

        return selectedTokenIds.ToHashSet().SetEquals(expectedTokenIds)
            ? AnswerCheckResult.Correct
            : AnswerCheckResult.Wrong;
    }

    /// <summary>Границы для запятых: точное множество BoundaryId.
    /// Пустой набор неверен: в пунктуационных шаблонах этой версии целевая запятая всегда есть.</summary>
    public static AnswerCheckResult CheckBoundarySelection(IReadOnlyCollection<string> selectedBoundaryIds, IReadOnlySet<string> expectedBoundaryIds)
    {
        if (selectedBoundaryIds.Count == 0)
            return AnswerCheckResult.Wrong;

        return selectedBoundaryIds.ToHashSet().SetEquals(expectedBoundaryIds)
            ? AnswerCheckResult.Correct
            : AnswerCheckResult.Wrong;
    }

    /// <summary>Нормализация русского ответа: NFC, внешние пробелы, регистр, е/ё, знак ударения.</summary>
    public static string NormalizeRussian(string input, AnswerPolicy policy)
    {
        var text = input.Trim().Normalize(NormalizationForm.FormC);

        if (policy.IgnoreStressMark)
            text = text.Replace("\u0301", string.Empty);

        if (!policy.CaseSensitive)
            text = text.ToLowerInvariant();

        if (!policy.YoDistinct)
            text = text.Replace('ё', 'е').Replace('Ё', 'Е');

        return text;
    }

    /// <summary>Только русские буквы: похожие латинские буквы не проходят.</summary>
    public static bool IsCyrillicLetter(char c) =>
        c is >= 'а' and <= 'я' or >= 'А' and <= 'Я' or 'ё' or 'Ё';

    public static bool TryParseInteger(string? input, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input) || input.Length > MaxInputLength)
            return false;

        var text = input.Trim().Replace('−', '-');
        return int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDecimal(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input) || input.Length > MaxInputLength)
            return false;

        var text = input.Trim().Replace('−', '-').Replace(',', '.');

        // NumberStyles.Number без AllowThousands/AllowExponent: «1 234», «1e3», «2+3» не проходят.
        return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Сокращение дроби; знаменатель положительный.</summary>
    public static (int Numerator, int Denominator) ReduceFraction(int numerator, int denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("Знаменатель дроби не может быть нулевым");

        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        var gcd = Gcd(Math.Abs(numerator), denominator);
        return gcd <= 1 ? (numerator, denominator) : (numerator / gcd, denominator / gcd);
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }

        return a;
    }
}
