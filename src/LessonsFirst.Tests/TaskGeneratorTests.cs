using LessonsFirst.App.Tasks;
using Xunit;

namespace LessonsFirst.Tests;

public class MathGeneratorTests
{
    public static TheoryData<GradeBand> AllBands() => new()
    {
        GradeBand.Grades12,
        GradeBand.Grades34,
        GradeBand.Grades56,
    };

    [Theory]
    [MemberData(nameof(AllBands))]
    public void GeneratedTasks_AreSelfConsistent(GradeBand band)
    {
        var generator = new MathGenerator(band);
        var rng = new Random(20261119);

        for (var i = 0; i < 300; i++)
        {
            var task = generator.Generate(rng);

            Assert.Equal("Математика", task.Subject);
            Assert.True(task.IsTextInput);
            Assert.Null(task.Options);
            Assert.False(string.IsNullOrWhiteSpace(task.Prompt));
            Assert.True(task.CheckAnswer(task.CorrectAnswer),
                $"Задача не проходит по своему же ответу: «{task.Prompt}» → {task.CorrectAnswer}");
        }
    }

    [Theory]
    [MemberData(nameof(AllBands))]
    public void GeneratedTasks_AnswerIsNotEmpty(GradeBand band)
    {
        var generator = new MathGenerator(band);
        var rng = new Random(42);

        for (var i = 0; i < 100; i++)
        {
            var task = generator.Generate(rng);
            Assert.False(string.IsNullOrWhiteSpace(task.CorrectAnswer));
        }
    }
}

public class RussianGeneratorTests
{
    [Theory]
    [InlineData(GradeBand.Grades12)]
    [InlineData(GradeBand.Grades34)]
    [InlineData(GradeBand.Grades56)]
    public void GeneratedTasks_HaveValidOptions(GradeBand band)
    {
        var generator = new RussianGenerator(band);
        var rng = new Random(7);

        for (var i = 0; i < 300; i++)
        {
            var task = generator.Generate(rng);

            Assert.Equal("Русский язык", task.Subject);
            Assert.False(task.IsTextInput);
            Assert.NotNull(task.Options);
            Assert.Contains(task.CorrectAnswer, task.Options!);

            var distinct = task.Options!.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Assert.True(distinct.Count >= 2, $"Меньше двух вариантов: {task.Prompt}");
            Assert.Equal(distinct.Count, task.Options!.Count); // нет дубликатов

            foreach (var option in task.Options!)
            {
                if (option == task.CorrectAnswer)
                    continue;
                Assert.False(task.CheckAnswer(option), $"Дистрактор принят как верный: {task.Prompt}");
            }
        }
    }
}

public class TaskProviderTests
{
    [Fact]
    public void Tasks_NeverRepeatSubjectThreeTimesInRow()
    {
        var provider = new TaskProvider(new Random(99));

        var streak = 1;
        string? last = null;
        for (var i = 0; i < 200; i++)
        {
            var task = provider.Next();
            if (task.Subject == last)
            {
                streak++;
            }
            else
            {
                streak = 1;
                last = task.Subject;
            }

            Assert.True(streak <= 2, $"Три задачи «{task.Subject}» подряд");
        }
    }

    [Fact]
    public void Configure_SwitchesBand()
    {
        var provider = new TaskProvider(new Random(1));
        provider.Configure(GradeBand.Grades56);

        var sawFraction = false;
        for (var i = 0; i < 200 && !sawFraction; i++)
        {
            var task = provider.Next();
            sawFraction |= task.Subject == "Математика" && task.IsFraction;
        }

        Assert.True(sawFraction, "После Configure(Grades56) должны встречаться задачи с дробями");
    }
}
