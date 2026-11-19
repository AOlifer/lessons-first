using System.Text.RegularExpressions;
using LessonsFirst.App.Tasks;
using LessonsFirst.App.Tasks.MathTasks;
using Xunit;

namespace LessonsFirst.Tests;

/// <summary>
/// Независимая oracle-проверка: ответ пересчитывается из УСЛОВИЯ (разбор текста),
/// а не тем же методом, которым задача построена.
/// </summary>
public class MathEngineTests
{
    private static SchoolTask NextMath(GradeBand band, Random rng) => new MathEngine(rng, band).Next();

    [Fact]
    public void SameSeed_ReproducibleSequence()
    {
        var a = Generate(GradeBand.Grades56, seed: 777, count: 50);
        var b = Generate(GradeBand.Grades56, seed: 777, count: 50);

        for (var i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].TemplateId, b[i].TemplateId);
            Assert.Equal(a[i].Prompt, b[i].Prompt);
            Assert.Equal(a[i].ExpectedInteger, b[i].ExpectedInteger);
        }
    }

    [Theory]
    [InlineData(GradeBand.Grades12, 10)]
    [InlineData(GradeBand.Grades34, 12)]
    [InlineData(GradeBand.Grades56, 12)]
    public void FullCycle_EachTemplateExactlyOnce(GradeBand band, int templateCount)
    {
        var engine = new MathEngine(new Random(42), band);
        var ids = Enumerable.Range(0, templateCount).Select(_ => engine.Next().TemplateId).ToList();

        Assert.Equal(templateCount, ids.Distinct().Count());
    }

    [Fact]
    public void NoConsecutiveSameTemplate()
    {
        var tasks = Generate(GradeBand.Grades12, seed: 9, count: 500);
        for (var i = 1; i < tasks.Count; i++)
        {
            Assert.NotEqual(tasks[i - 1].TemplateId, tasks[i].TemplateId);
        }
    }

    [Theory]
    [InlineData(GradeBand.Grades12)]
    [InlineData(GradeBand.Grades34)]
    [InlineData(GradeBand.Grades56)]
    public void ThousandTasks_AllTemplatesCovered_AllValidated(GradeBand band)
    {
        var tasks = Generate(band, seed: 20261119, count: 1000);

        var expectedIds = band switch
        {
            GradeBand.Grades12 => Level12Templates.All.Select(t => t.Id),
            GradeBand.Grades34 => Level34Templates.All.Select(t => t.Id),
            _ => Level56Templates.All.Select(t => t.Id),
        };
        Assert.Subset(expectedIds.ToHashSet(), tasks.Select(t => t.TemplateId).ToHashSet());

        foreach (var task in tasks)
        {
            Validate(task);
        }
    }

    [Fact]
    public void A7_Exactly45Variants_ThenDeterministicExhaustion()
    {
        var template = Level12Templates.All.Single(t => t.Id == "A7");
        var rng = new Random(5);
        var used = new HashSet<string>();

        // Детерминированно выбираем все варианты по одному.
        for (var i = 0; i < 45; i++)
        {
            Assert.True(template.TryPickUnused(used, rng, out _, out var key), $"вариант {i + 1} из 45 не найден");
            used.Add(key);
        }

        Assert.Equal(45, used.Count);
        Assert.False(template.TryPickUnused(used, rng, out _, out _), "A7 должен исчерпаться ровно на 45 вариантах");
    }

    [Fact]
    public void A8_Exactly55Variants_ThenDeterministicExhaustion()
    {
        var template = Level12Templates.All.Single(t => t.Id == "A8");
        var rng = new Random(6);
        var used = new HashSet<string>();

        for (var i = 0; i < 55; i++)
        {
            Assert.True(template.TryPickUnused(used, rng, out _, out var key));
            used.Add(key);
        }

        Assert.Equal(55, used.Count);
        Assert.False(template.TryPickUnused(used, rng, out _, out _));
    }

    [Fact]
    public void Engine_ContinuesAfterTemplateExhaustion()
    {
        // 1000 задач уровня 1–2: A7 встречается ~100 раз при 45 вариантах —
        // движок обязан пережить исчерпание без исключений и повторов подряд.
        var tasks = Generate(GradeBand.Grades12, seed: 3, count: 1000);
        Assert.True(tasks.Count(t => t.TemplateId == "A7") > 45);
        for (var i = 1; i < tasks.Count; i++)
        {
            if (tasks[i - 1].TemplateId == "A7" && tasks[i].TemplateId == "A7")
            {
                Assert.NotEqual(tasks[i - 1].Prompt, tasks[i].Prompt);
            }
        }
    }

    [Fact]
    public void B3_AllValidPairsOf36()
    {
        // Для произведения 36 правильны 4×9, 9×4, 6×6 — и только они из 2…9.
        var expected = new[] { (4, 9), (9, 4), (6, 6) };
        foreach (var (a, b) in expected)
        {
            Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckFactorPair(
                a.ToString(), b.ToString(), (4, 9)));
        }

        var wrong = new[] { (2, 18), (3, 12), (6, 7), (36, 1) };
        foreach (var (a, b) in wrong)
        {
            Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckFactorPair(
                a.ToString(), b.ToString(), (4, 9)));
        }
    }

    private static List<SchoolTask> Generate(GradeBand band, int seed, int count)
    {
        var engine = new MathEngine(new Random(seed), band);
        return Enumerable.Range(0, count).Select(_ => engine.Next()).ToList();
    }

    // —— Независимая валидация: ответ считается разбором условия ——
    private static void Validate(SchoolTask task)
    {
        Assert.False(string.IsNullOrWhiteSpace(task.Prompt));
        Assert.False(string.IsNullOrWhiteSpace(task.Hint));
        Assert.NotEqual(Guid.Empty, task.TaskId);

        switch (task.TemplateId)
        {
            case "A1": case "A7": OracleBinary(task, @"Вычисли: (\d+) \+ (\d+)", (a, b) => a + b); break;
            case "A2": case "A8": OracleBinary(task, @"Вычисли: (\d+) − (\d+)", (a, b) => a - b); break;
            case "A3": case "B5": OracleBinary(task, @"Увеличь (\d+) на (\d+)", (a, b) => a + b); break;
            case "A4": case "B7": OracleBinary(task, @"Уменьши (\d+) на (\d+)", (a, b) => a - b); break;
            case "A5": OracleBinary(task, @"□ \+ (\d+) = (\d+)", (a, sum) => sum - a); break;
            case "A6": ValidateComparison(task); break;
            case "A9": OracleBinary(task, @"(\d+) [а-яё]+\. Ей подарили ещё (\d+)", (a, b) => a + b); break;
            case "A10": OracleBinary(task, @"Было (\d+) [а-яё]+\. (\d+) [а-яё]+ отдали", (a, b) => a - b); break;
            case "B1": OracleBinary(task, @"произведение чисел (\d+) и (\d+)", (a, b) => a * b); break;
            case "B3": ValidateB3(task); break;
            case "B8": OracleBinary(task, @"Один множитель — (\d+), другой — (\d+)", (a, b) => a * b); break;
            case "B2": OracleBinary(task, @"частное чисел (\d+) и (\d+)", (a, b) => a / b); break;
            case "B4": OracleBinary(task, @"Увеличь (\d+) в (\d+) раз", (a, k) => a * k); break;
            case "B6": OracleBinary(task, @"Уменьши (\d+) в (\d+) раз", (a, k) => a / k); break;
            case "B9": OracleBinary(task, @"Делимое — (\d+), делитель — (\d+)", (a, b) => a / b); break;
            case "B10": OracleBinary(task, @"разделить (\d+), чтобы получить (\d+)", (a, q) => a / q); break;
            case "B11": OracleBinary(task, @"большой упаковке (\d+) шоколад\w+, а в маленькой — в (\d+) раз", (big, k) => big / k); break;
            case "B12": OracleBinary(task, @"маленькой коробке (\d+) игруш\w+, а в большой — на (\d+) игруш\w+", (small, add) => small + add); break;
            case "C1": OracleBinary(task, @"Вычисли: (\d+) × (\d+)", (a, b) => a * b); break;
            case "C2": ValidateC2(task); break;
            case "C3": OracleTernary(task, @"Вычисли: (\d+) \+ (\d+) × (\d+)", (a, b, c) => a + b * c); break;
            case "C4": OracleTernary(task, @"Вычисли: \((\d+) \+ (\d+)\) × (\d+)", (a, b, c) => (a + b) * c); break;
            case "C5": ValidateC5(task); break;
            case "C6": ValidateC6(task); break;
            case "C7": ValidateC7(task); break;
            case "C8": ValidateC8(task); break;
            case "C9": ValidateC9(task); break;
            case "C10": ValidateC10(task); break;
            case "C11":
            {
                var match = Regex.Match(task.Prompt, @"Купили (\d+) набор\w+ по (\d+) [а-яё]+\. (\d+) [а-яё]+ отдали");
                Assert.True(match.Success, task.Prompt);
                Assert.Equal(
                    int.Parse(match.Groups[1].Value) * int.Parse(match.Groups[2].Value)
                        - int.Parse(match.Groups[3].Value),
                    task.ExpectedInteger);
                break;
            }
            case "C12": ValidateC12(task); break;
            default: Assert.Fail($"Неизвестный шаблон {task.TemplateId}"); break;
        }
    }

    private static void OracleBinary(SchoolTask task, string pattern, Func<int, int, int> compute)
    {
        var match = Regex.Match(task.Prompt, pattern);
        Assert.True(match.Success, $"Условие не разобрано [{task.TemplateId}]: {task.Prompt}");
        var expected = compute(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        Assert.Equal(AnswerForm.Integer, task.Form);
        Assert.Equal(expected, task.ExpectedInteger);
    }

    private static void OracleTernary(SchoolTask task, string pattern, Func<int, int, int, int> compute)
    {
        var match = Regex.Match(task.Prompt, pattern);
        Assert.True(match.Success, $"Условие не разобрано [{task.TemplateId}]: {task.Prompt}");
        var expected = compute(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[3].Value));
        Assert.Equal(expected, task.ExpectedInteger);
    }

    private static void ValidateB3(SchoolTask task)
    {
        Assert.Equal(AnswerForm.FactorPair, task.Form);
        var match = Regex.Match(task.Prompt, @"произведение которых равно (\d+)");
        Assert.True(match.Success, task.Prompt);
        var target = int.Parse(match.Groups[1].Value);

        var (a, b) = task.ExpectedFactors;
        Assert.InRange(a, 2, 9);
        Assert.InRange(b, 2, 9);
        Assert.Equal(target, a * b);
    }

    private static void ValidateComparison(SchoolTask task)
    {
        Assert.Equal(AnswerForm.Comparison, task.Form);
        var match = Regex.Match(task.Prompt, @"Выбери знак: (\d+) □ (\d+)");
        Assert.True(match.Success, task.Prompt);
        var a = int.Parse(match.Groups[1].Value);
        var b = int.Parse(match.Groups[2].Value);
        var expected = a < b ? ComparisonRelation.Less : a > b ? ComparisonRelation.Greater : ComparisonRelation.Equal;
        Assert.Equal(expected, task.ExpectedRelation);
    }

    private static void ValidateC2(SchoolTask task)
    {
        var match = Regex.Match(task.Prompt, @"Вычисли: (\d+) ÷ (\d+)");
        Assert.True(match.Success, task.Prompt);
        var a = int.Parse(match.Groups[1].Value);
        var d = int.Parse(match.Groups[2].Value);
        Assert.Equal(0, a % d); // остатка нет
        Assert.Equal(a / d, task.ExpectedInteger);
        Assert.InRange(d, 12, 25);
    }

    private static void ValidateC5(SchoolTask task)
    {
        var match = Regex.Match(task.Prompt, @"(\d+) × x \+ (\d+) = (\d+)");
        Assert.True(match.Success, task.Prompt);
        var a = int.Parse(match.Groups[1].Value);
        var b = int.Parse(match.Groups[2].Value);
        var c = int.Parse(match.Groups[3].Value);
        Assert.Equal(0, (c - b) % a);
        Assert.Equal((c - b) / a, task.ExpectedInteger);
    }

    private static void ValidateC6(SchoolTask task)
    {
        Assert.Equal(AnswerForm.Fraction, task.Form);
        var match = Regex.Match(task.Prompt, @"(\d+)/(\d+) ([+−]) (\d+)/(\d+)");
        Assert.True(match.Success, task.Prompt);
        var n1 = int.Parse(match.Groups[1].Value);
        var d1 = int.Parse(match.Groups[2].Value);
        var minus = match.Groups[3].Value == "−";
        var n2 = int.Parse(match.Groups[4].Value);
        var d2 = int.Parse(match.Groups[5].Value);

        Assert.InRange(d1, 2, 12);
        Assert.InRange(d2, 2, 12);
        Assert.True(n1 < d1 && n2 < d2); // правильные дроби
        var lcm = d1 / Gcd(d1, d2) * d2;
        Assert.True(lcm <= 60);
        if (minus)
        {
            Assert.True(n1 * d2 > n2 * d1, "при вычитании первая дробь должна быть больше");
        }

        var num = minus ? n1 * d2 - n2 * d1 : n1 * d2 + n2 * d1;
        var expected = AnswerValidator.ReduceFraction(num, d1 * d2);
        Assert.Equal(expected, task.ExpectedFraction);
        Assert.True(task.ExpectedFraction.Denominator > 0);
    }

    private static void ValidateC7(SchoolTask task)
    {
        var match = Regex.Match(task.Prompt, @"Найди (\d+)/(\d+) от числа (\d+)");
        Assert.True(match.Success, task.Prompt);
        var m = int.Parse(match.Groups[1].Value);
        var n = int.Parse(match.Groups[2].Value);
        var big = int.Parse(match.Groups[3].Value);
        Assert.True(m < n);
        Assert.Equal(0, big % n);
        Assert.Equal(big / n * m, task.ExpectedInteger);
    }

    private static void ValidateC8(SchoolTask task)
    {
        var match = Regex.Match(task.Prompt, @"Найди (\d+)% от числа (\d+)");
        Assert.True(match.Success, task.Prompt);
        var p = int.Parse(match.Groups[1].Value);
        var n = int.Parse(match.Groups[2].Value);
        Assert.Contains(p, new[] { 5, 10, 20, 25, 50, 75 });
        Assert.InRange(n, 20, 1000);
        Assert.Equal(0, n * p % 100); // целый результат
        Assert.Equal(n * p / 100, task.ExpectedInteger);
    }

    private static void ValidateC9(SchoolTask task)
    {
        Assert.Equal(AnswerForm.Decimal, task.Form);
        var match = Regex.Match(task.Prompt, @"Вычисли: (\d+,\d{1,2}) ([+−]) (\d+,\d{1,2})");
        Assert.True(match.Success, task.Prompt);
        var a = decimal.Parse(match.Groups[1].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
        var minus = match.Groups[2].Value == "−";
        var b = decimal.Parse(match.Groups[3].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
        var expected = minus ? a - b : a + b;
        Assert.Equal(expected, task.ExpectedDecimalValue);

        var hasFraction = (a % 1) != 0 || (b % 1) != 0;
        Assert.True(hasFraction, "хотя бы один операнд с дробной частью");
        if (!minus)
        {
            Assert.True(a + b <= 100);
        }
    }

    private static void ValidateC10(SchoolTask task)
    {
        Assert.Equal(AnswerForm.Decimal, task.Form);
        var match = Regex.Match(task.Prompt, @"Вычисли: (\d+,\d{1,2}) × (\d+)");
        Assert.True(match.Success, task.Prompt);
        var a = decimal.Parse(match.Groups[1].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
        var b = int.Parse(match.Groups[2].Value);
        Assert.True(a % 1 != 0);
        Assert.Equal(a * b, task.ExpectedDecimalValue);
    }

    private static void ValidateC12(SchoolTask task)
    {
        Assert.Equal(AnswerForm.Integer, task.Form);
        var match = Regex.Match(task.Prompt, @"Вычисли: (-?\d+) ([+−]) (\(?)(-?\d+)(\)?)");
        Assert.True(match.Success, task.Prompt);
        var a = int.Parse(match.Groups[1].Value);
        var minus = match.Groups[2].Value == "−";
        var b = int.Parse(match.Groups[4].Value);
        Assert.True(a < 0 || b < 0, "хотя бы один операнд отрицательный");
        if (minus && b < 0)
        {
            Assert.True(match.Groups[3].Value == "(" && match.Groups[5].Value == ")", "отрицательный второй операнд — в скобках");
        }

        var expected = minus ? a - b : a + b;
        Assert.Equal(expected, task.ExpectedInteger);
        Assert.InRange(expected, -100, 100);
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
