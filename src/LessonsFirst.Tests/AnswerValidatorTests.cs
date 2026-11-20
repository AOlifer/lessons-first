using LessonsFirst.App;
using LessonsFirst.App.Tasks;
using Xunit;

namespace LessonsFirst.Tests;

public class AnswerValidatorTests
{
    // —— Integer ——
    [Theory]
    [InlineData("15", 15, AnswerCheckResult.Correct)]
    [InlineData(" 15 ", 15, AnswerCheckResult.Correct)]
    [InlineData("015", 15, AnswerCheckResult.Correct)]
    [InlineData("15", 16, AnswerCheckResult.Wrong)]
    [InlineData("−5", -5, AnswerCheckResult.Correct)] // математический минус
    [InlineData("-5", -5, AnswerCheckResult.Correct)]
    [InlineData("", 5, AnswerCheckResult.InvalidFormat)]
    [InlineData("abc", 5, AnswerCheckResult.InvalidFormat)]
    [InlineData("2+3", 5, AnswerCheckResult.InvalidFormat)]
    [InlineData("9999999999999999999999", 5, AnswerCheckResult.InvalidFormat)] // переполнение — безопасно
    public void Integer_Answers(string input, int expected, AnswerCheckResult result) =>
        Assert.Equal(result, AnswerValidator.CheckInteger(input, expected));

    // —— Decimal ——
    [Theory]
    [InlineData("7,2", AnswerCheckResult.Correct)]
    [InlineData("7.2", AnswerCheckResult.Correct)]
    [InlineData("7,20", AnswerCheckResult.Correct)]
    [InlineData("07,2", AnswerCheckResult.Correct)]
    [InlineData("8", AnswerCheckResult.Wrong)]
    [InlineData("1 234", AnswerCheckResult.InvalidFormat)]
    [InlineData("1e3", AnswerCheckResult.InvalidFormat)]
    [InlineData("2+3", AnswerCheckResult.InvalidFormat)]
    public void Decimal_SevenPointTwo(string input, AnswerCheckResult result) =>
        Assert.Equal(result, AnswerValidator.CheckDecimal(input, 7.2m));

    [Fact]
    public void Decimal_WholeAnswerAccepted() =>
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckDecimal("5", 5.00m));

    [Fact]
    public void Decimal_ExactArithmetic_NoDoubleRounding()
    {
        // 0,1 + 0,2 = 0,3 — точно через decimal, без double.
        var sum = 0.1m + 0.2m;
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckDecimal("0,3", sum));
    }

    // —— Fraction ——
    [Theory]
    [InlineData("1", "2", AnswerCheckResult.Correct)]
    [InlineData("2", "4", AnswerCheckResult.Correct)]
    [InlineData("3", "6", AnswerCheckResult.Correct)]
    [InlineData("50", "100", AnswerCheckResult.Correct)]
    [InlineData("1", "3", AnswerCheckResult.Wrong)]
    [InlineData("-1", "-2", AnswerCheckResult.Correct)] // эквивалентная запись
    [InlineData("1", "0", AnswerCheckResult.InvalidFormat)] // знаменатель 0
    [InlineData("a", "2", AnswerCheckResult.InvalidFormat)]
    [InlineData("", "2", AnswerCheckResult.InvalidFormat)]
    public void Fraction_HalfVariants(string num, string den, AnswerCheckResult result) =>
        Assert.Equal(result, AnswerValidator.CheckFraction(num, den, (1, 2)));

    [Fact]
    public void Fraction_WholeNumberWithDenominatorOne() =>
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckFraction("5", "1", (5, 1)));

    // —— FactorPair ——
    [Theory]
    [InlineData("3", "7", AnswerCheckResult.Correct)]
    [InlineData("7", "3", AnswerCheckResult.Correct)] // порядок не важен
    [InlineData("1", "21", AnswerCheckResult.Wrong)] // вне диапазона 2–9
    [InlineData("21", "1", AnswerCheckResult.Wrong)]
    [InlineData("4", "8", AnswerCheckResult.Wrong)]   // произведение не то
    [InlineData("x", "3", AnswerCheckResult.InvalidFormat)]
    public void FactorPair_21(string a, string b, AnswerCheckResult result) =>
        Assert.Equal(result, AnswerValidator.CheckFactorPair(a, b, (3, 7)));

    // —— Comparison ——
    [Theory]
    [InlineData(ComparisonRelation.Less, AnswerCheckResult.Correct)]
    [InlineData(ComparisonRelation.Equal, AnswerCheckResult.Wrong)]
    [InlineData(ComparisonRelation.Greater, AnswerCheckResult.Wrong)]
    public void Comparison_Relations(ComparisonRelation given, AnswerCheckResult result) =>
        Assert.Equal(result, AnswerValidator.CheckComparison(given, ComparisonRelation.Less));

    // —— Choice (по идентификатору варианта) ——
    [Fact]
    public void Choice_ById()
    {
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckChoice("o1", "o1"));
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckChoice("o2", "o1"));
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckChoice(null, "o1"));
    }

    // —— ShortText ——
    private static readonly AnswerPolicy LetterPolicy = new() { CaseSensitive = false };
    private static readonly AnswerPolicy WordPolicy = new() { CaseSensitive = false, YoDistinct = false, IgnoreStressMark = true };

    [Fact]
    public void ShortText_Letter_CaseInsensitive()
    {
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText("И", new[] { "и" }, LetterPolicy, 1));
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText(" и ", new[] { "и" }, LetterPolicy, 1));
    }

    [Fact]
    public void ShortText_WrongLetter_IsWrong()
    {
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText("а", new[] { "и" }, LetterPolicy, 1));
        // й и и не объединяются
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText("й", new[] { "и" }, LetterPolicy, 1));
    }

    [Fact]
    public void ShortText_LatinLookalikes_Rejected()
    {
        // латинские i, c, e, o, a, y не принимаются вместо русских
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckShortText("i", new[] { "и" }, LetterPolicy, 1));
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckShortText("c", new[] { "с" }, LetterPolicy, 1));
        // слово с латинскими k, o вместо к, о
        var latinKon = string.Concat('k', 'o', 'н', 'ь');
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckShortText(latinKon, new[] { "конь" }, LetterPolicy, 4));
    }

    [Fact]
    public void ShortText_YoDistinct_WhenPolicyRequires()
    {
        var policy = new AnswerPolicy { CaseSensitive = false, YoDistinct = true };
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckShortText("е", new[] { "ё" }, policy, 1));
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckShortText("ё", new[] { "ё" }, policy, 1));
    }

    [Fact]
    public void ShortText_YoVariantsAccepted_ForWordForms()
    {
        // конём и конем — обе формы нормативны в заданиях на словоформу
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckShortText("конем", new[] { "конём", "конем" }, WordPolicy, 24));
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckShortText("КОНЁМ", new[] { "конём", "конем" }, WordPolicy, 24));
    }

    [Fact]
    public void ShortText_NfcDecomposedEqualsComposed()
    {
        // NFC: й в разложенной форме (и + U+0306? нет: й = и + combining breve U+0306)
        var decomposed = "и\u0306"; // й как и + U+0306
        var composed = "й";
        Assert.NotEqual(decomposed, composed); // до нормализации разные строки
        var policy = new AnswerPolicy { CaseSensitive = false };
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText(decomposed, new[] { "и" }, policy, 1)); // й ≠ и даже после NFC
    }

    [Fact]
    public void ShortText_StressMarkIgnoredWhenAllowed()
    {
        var policy = new AnswerPolicy { CaseSensitive = false, IgnoreStressMark = true };
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText("ко\u0301нь", new[] { "конь" }, policy, 4));
        // знак ударения — не буква и не увеличивает длину смысла
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText("конь", new[] { "кон" }, policy, 4));
    }

    [Fact]
    public void ShortText_EmptyOrTooLong_IsInvalidFormat()
    {
        Assert.Equal(AnswerCheckResult.InvalidFormat, AnswerValidator.CheckShortText("", new[] { "и" }, LetterPolicy, 1));
        Assert.Equal(AnswerCheckResult.InvalidFormat, AnswerValidator.CheckShortText("   ", new[] { "и" }, LetterPolicy, 1));
        Assert.Equal(AnswerCheckResult.InvalidFormat, AnswerValidator.CheckShortText("нннн", new[] { "нн" }, LetterPolicy, 2));
        Assert.Equal(AnswerCheckResult.InvalidFormat, AnswerValidator.CheckShortText("оченьдлинноеслово", new[] { "нн" }, LetterPolicy, 2));
    }

    // —— TokenSelection ——
    [Fact]
    public void TokenSelection_ExactSetMatch()
    {
        var expected = new HashSet<string> { "t1", "t3" };
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckTokenSelection(new[] { "t3", "t1" }, expected));
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckTokenSelection(new[] { "t1" }, expected));
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckTokenSelection(new[] { "t1", "t3", "t5" }, expected));
        Assert.Equal(AnswerCheckResult.InvalidFormat, AnswerValidator.CheckTokenSelection(Array.Empty<string>(), expected));
    }

    // —— BoundarySelection ——
    [Fact]
    public void BoundarySelection_ExactSetMatch_EmptyIsWrong()
    {
        var expected = new HashSet<string> { "b0", "b1" };
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckBoundarySelection(new[] { "b1", "b0" }, expected));
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckBoundarySelection(new[] { "b0" }, expected));
        // пустой набор неверен: целевая запятая всегда есть
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckBoundarySelection(Array.Empty<string>(), expected));
    }

    [Fact]
    public void ReduceFraction_SimplifiesAndKeepsPositiveDenominator()
    {
        Assert.Equal((1, 2), AnswerValidator.ReduceFraction(3, 6));
        Assert.Equal((1, 2), AnswerValidator.ReduceFraction(-3, -6));
        Assert.Equal((-1, 2), AnswerValidator.ReduceFraction(-3, 6));
    }
}

public class SessionProgressTests
{
    [Fact]
    public void Correct_CountsOncePerTaskId()
    {
        var progress = new SessionProgress(3);
        var taskId = Guid.NewGuid();

        Assert.True(progress.RegisterCorrect(taskId));
        Assert.False(progress.RegisterCorrect(taskId)); // двойной Enter / повтор
        Assert.Equal(1, progress.Correct);
    }

    [Fact]
    public void Access_OnlyAfterRequiredCount()
    {
        var progress = new SessionProgress(3);
        Assert.False(progress.Completed);

        Assert.True(progress.RegisterCorrect(Guid.NewGuid()));
        Assert.False(progress.Completed);
        Assert.True(progress.RegisterCorrect(Guid.NewGuid()));
        Assert.False(progress.Completed);
        Assert.True(progress.RegisterCorrect(Guid.NewGuid()));
        Assert.True(progress.Completed);
    }

    [Fact]
    public void NoProgress_AfterCompletion()
    {
        var progress = new SessionProgress(1);
        progress.RegisterCorrect(Guid.NewGuid());

        Assert.False(progress.RegisterCorrect(Guid.NewGuid()));
        Assert.Equal(1, progress.Correct);
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var progress = new SessionProgress(2);
        progress.RegisterCorrect(Guid.NewGuid());
        progress.Reset();

        Assert.Equal(0, progress.Correct);
        Assert.Equal(0, progress.Mistakes);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Mistake_DoesNotChangeProgressOrGoal()
    {
        var progress = new SessionProgress(3);
        progress.RegisterMistake();
        progress.RegisterMistake();
        progress.RegisterMistake();

        Assert.Equal(3, progress.Required);
        Assert.Equal(0, progress.Correct);
        Assert.Equal(3, progress.Mistakes);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Task_CountedAfterMistakeOnIt()
    {
        // Тот же вопрос остаётся: верный ответ после ошибки засчитывается.
        var progress = new SessionProgress(2);
        var taskId = Guid.NewGuid();
        progress.RegisterMistake();

        Assert.True(progress.RegisterCorrect(taskId));
        Assert.Equal(1, progress.Correct);
        Assert.Equal(2, progress.Required);
    }

    [Fact]
    public void Completion_AtExactlyN()
    {
        var progress = new SessionProgress(3);
        progress.RegisterMistake();
        progress.RegisterCorrect(Guid.NewGuid());
        progress.RegisterCorrect(Guid.NewGuid());

        Assert.False(progress.Completed);

        progress.RegisterCorrect(Guid.NewGuid());
        Assert.True(progress.Completed);
        Assert.Equal(3, progress.Correct);
        Assert.Equal(3, progress.Required);
        Assert.Equal(1, progress.Mistakes);
    }
}
