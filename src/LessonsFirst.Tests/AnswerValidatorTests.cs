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

    // —— Choice ——
    [Fact]
    public void Choice_CaseInsensitive() =>
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckChoice("О", "о"));

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
        Assert.False(progress.Completed);
    }

    [Fact]
    public void Mistake_AddsOneTaskToRequired()
    {
        var progress = new SessionProgress(3);
        Assert.Equal(3, progress.Required);

        Assert.True(progress.RegisterMistake(Guid.NewGuid()));
        Assert.Equal(4, progress.Required);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void FailedTask_NeverCounted_EvenAfterCorrect()
    {
        var progress = new SessionProgress(2);
        var taskId = Guid.NewGuid();
        progress.RegisterMistake(taskId);

        Assert.False(progress.RegisterCorrect(taskId));
        Assert.Equal(0, progress.Correct);
        Assert.Equal(3, progress.Required);
        Assert.False(progress.Completed);
    }

    [Fact]
    public void RepeatedMistakesOnSameTask_AddOnlyOne()
    {
        var progress = new SessionProgress(2);
        var taskId = Guid.NewGuid();

        Assert.True(progress.RegisterMistake(taskId));
        Assert.False(progress.RegisterMistake(taskId));

        Assert.Equal(3, progress.Required);
    }

    [Fact]
    public void Completion_NeedsExtraTaskPerEachFailed()
    {
        var progress = new SessionProgress(2);
        progress.RegisterMistake(Guid.NewGuid()); // цель стала 3
        progress.RegisterCorrect(Guid.NewGuid());
        progress.RegisterCorrect(Guid.NewGuid());

        Assert.False(progress.Completed); // 2 из 3: провал добавил задачу

        progress.RegisterCorrect(Guid.NewGuid());
        Assert.True(progress.Completed);
        Assert.Equal(3, progress.Correct);
        Assert.Equal(3, progress.Required);
    }

    [Fact]
    public void Reset_ClearsFailuresToo()
    {
        var progress = new SessionProgress(2);
        var taskId = Guid.NewGuid();
        progress.RegisterMistake(taskId);
        progress.Reset();

        Assert.Equal(2, progress.Required);
        Assert.True(progress.RegisterCorrect(taskId)); // после сброса задание снова зачитывается
    }
}
