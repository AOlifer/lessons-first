using LessonsFirst.App.Tasks;
using Xunit;

namespace LessonsFirst.Tests;

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
            Assert.Equal(AnswerForm.Choice, task.Form);
            Assert.NotNull(task.Options);
            Assert.Contains(task.CorrectOption, task.Options!);

            var distinct = task.Options!.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Assert.True(distinct.Count >= 2);
            Assert.Equal(distinct.Count, task.Options!.Count);

            foreach (var option in task.Options!)
            {
                if (option == task.CorrectOption)
                {
                    continue;
                }

                Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckChoice(option, task.CorrectOption));
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

            Assert.True(streak <= 2);
        }
    }

    [Fact]
    public void MathTasks_UseTemplatesOfConfiguredBand()
    {
        var provider = new TaskProvider(new Random(1));
        provider.Configure(GradeBand.Grades12);

        var mathSeen = new HashSet<string>();
        for (var i = 0; i < 400; i++)
        {
            var task = provider.Next();
            if (task.Subject == "Математика")
            {
                mathSeen.Add(task.TemplateId);
                Assert.Contains(task.TemplateId, new[]
                {
                    "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "A10",
                });
            }
        }

        Assert.True(mathSeen.Count >= 8, $"Покрыто шаблонов: {mathSeen.Count}");
    }
}
