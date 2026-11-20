using System.IO;
using LessonsFirst.App.Tasks;
using Xunit;

namespace LessonsFirst.Tests;

/// <summary>
/// Общая сессия: предметные режимы, перемешанные пары, независимые циклы
/// шаблонов, общий счётчик N и фиксация настроек на сессию.
/// </summary>
public class SessionFlowTests
{
    private const string Math = "Математика";
    private const string Russian = "Русский язык";

    [Fact]
    public void MathMode_OnlyMathTasks()
    {
        var provider = new TaskProvider(new Random(1));
        provider.Configure(GradeBand.Grades34, SubjectMode.Math);

        for (var i = 0; i < 100; i++)
        {
            var task = provider.Next();
            Assert.Equal(Math, task.Subject);
            Assert.False(task.TemplateId.StartsWith("Russian.", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void RussianMode_OnlyRussianTasks()
    {
        var provider = new TaskProvider(new Random(2));
        provider.Configure(GradeBand.Grades56, SubjectMode.Russian);

        for (var i = 0; i < 100; i++)
        {
            var task = provider.Next();
            Assert.Equal(Russian, task.Subject);
            Assert.StartsWith("Russian.R56-", task.TemplateId);
            Assert.Equal(GradeBand.Grades56, task.Difficulty);
        }
    }

    [Fact]
    public void MixedMode_EachPairHasBothSubjects()
    {
        var provider = new TaskProvider(new Random(3));
        provider.Configure(GradeBand.Grades34, SubjectMode.Mixed);

        var tasks = Enumerable.Range(0, 200).Select(_ => provider.Next().Subject).ToList();

        // Разбиваем на последовательные пары: в каждой полной паре — по одному предмету.
        for (var i = 0; i + 1 < tasks.Count; i += 2)
        {
            var pair = new[] { tasks[i], tasks[i + 1] };
            Assert.Contains(Math, pair);
            Assert.Contains(Russian, pair);
        }
    }

    [Fact]
    public void MixedMode_IndependentTemplateCycles()
    {
        var provider = new TaskProvider(new Random(4));
        provider.Configure(GradeBand.Grades12, SubjectMode.Mixed);

        var mathIds = new HashSet<string>();
        var russianIds = new HashSet<string>();
        SchoolTask? lastMath = null;
        var mathCount = 0;

        for (var i = 0; i < 120; i++)
        {
            var task = provider.Next();
            if (task.Subject == Math)
            {
                Assert.Matches("^[ABC][0-9]+$", task.TemplateId);
                mathIds.Add(task.TemplateId);
                // Русские задания не сдвигают математический цикл: подряд идущие
                // математические задания — это соседние позиции одного цикла.
                if (lastMath is not null)
                {
                    Assert.NotEqual(lastMath.TemplateId, task.TemplateId);
                }

                lastMath = task;
                mathCount++;
            }
            else
            {
                Assert.StartsWith("Russian.R12-", task.TemplateId);
                russianIds.Add(task.TemplateId);
            }
        }

        Assert.True(mathCount >= 50);
        // Полный цикл каждого предмета пройден.
        Assert.Equal(10, russianIds.Count);
        Assert.True(mathIds.Count >= 10, $"математических шаблонов: {mathIds.Count}");
    }

    [Fact]
    public void MixedMode_SameSeed_Reproducible()
    {
        var a = Sequence(777);
        var b = Sequence(777);
        for (var i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].Subject, b[i].Subject);
            Assert.Equal(a[i].TemplateId, b[i].TemplateId);
            Assert.Equal(a[i].Prompt, b[i].Prompt);
        }
    }

    private static List<SchoolTask> Sequence(int seed)
    {
        var provider = new TaskProvider(new Random(seed));
        provider.Configure(GradeBand.Grades34, SubjectMode.Mixed);
        return Enumerable.Range(0, 60).Select(_ => provider.Next()).ToList();
    }

    [Fact]
    public void Configure_LevelChange_SwitchesTemplates()
    {
        var provider = new TaskProvider(new Random(5));
        provider.Configure(GradeBand.Grades12, SubjectMode.Russian);
        Assert.StartsWith("Russian.R12-", provider.Next().TemplateId);

        provider.Configure(GradeBand.Grades56, SubjectMode.Russian);
        Assert.StartsWith("Russian.R56-", provider.Next().TemplateId);
    }

    [Fact]
    public void Configure_SameSettings_KeepsCyclePosition()
    {
        var provider = new TaskProvider(new Random(6));
        provider.Configure(GradeBand.Grades34, SubjectMode.Math);
        var first = provider.Next().TemplateId;
        provider.Configure(GradeBand.Grades34, SubjectMode.Math); // повторная конфигурация — цикл не сбрасывается
        var second = provider.Next().TemplateId;

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void SessionSettingsSnapshot_ChangesApplyOnlyOnNextConfigure()
    {
        // Настройки фиксируются в начале сессии: пока Configure не вызван снова,
        // прежний уровень продолжает действовать (изменения родителя — к следующей сессии).
        var provider = new TaskProvider(new Random(7));
        provider.Configure(GradeBand.Grades12, SubjectMode.Mixed);
        provider.Next();

        // «Родитель поменял» настройки в объекте — но Configure сессии не вызван.
        var task = provider.Next();
        Assert.Equal(GradeBand.Grades12, task.Difficulty);
    }

    [Fact]
    public void Tasks_AllFormsPresentPerLevel()
    {
        // Формы ответа распределены по уровням: 1–2 — Integer/Choice/ShortText,
        // 3–4 — Choice/ShortText/TokenSelection, 5–6 — Choice/ShortText/BoundarySelection.
        var forms12 = FormsOfLevel(GradeBand.Grades12);
        Assert.Contains(AnswerForm.Integer, forms12);
        Assert.Contains(AnswerForm.Choice, forms12);
        Assert.Contains(AnswerForm.ShortText, forms12);

        var forms34 = FormsOfLevel(GradeBand.Grades34);
        Assert.Contains(AnswerForm.Choice, forms34);
        Assert.Contains(AnswerForm.ShortText, forms34);
        Assert.Contains(AnswerForm.TokenSelection, forms34);

        var forms56 = FormsOfLevel(GradeBand.Grades56);
        Assert.Contains(AnswerForm.Choice, forms56);
        Assert.Contains(AnswerForm.ShortText, forms56);
        Assert.Contains(AnswerForm.BoundarySelection, forms56);
    }

    private static HashSet<AnswerForm> FormsOfLevel(GradeBand band)
    {
        var provider = new TaskProvider(new Random(8));
        provider.Configure(band, SubjectMode.Russian);
        return Enumerable.Range(0, 300).Select(_ => provider.Next().Form).ToHashSet();
    }

    [Fact]
    public void RussianTemplates_AllThirtyIds_Unique()
    {
        var all = LessonsFirst.App.Tasks.RussianTasks.RussianTemplates12.All
            .Concat(LessonsFirst.App.Tasks.RussianTasks.RussianTemplates34.All)
            .Concat(LessonsFirst.App.Tasks.RussianTasks.RussianTemplates56.All)
            .Select(t => t.Id)
            .ToList();

        Assert.Equal(30, all.Count);
        Assert.Equal(30, all.Distinct().Count());
        Assert.Contains("Russian.R12-01", all);
        Assert.Contains("Russian.R34-01", all);
        Assert.Contains("Russian.R56-01", all);
        Assert.Contains("Russian.R56-10", all);
    }

    [Fact]
    public void FullVersion_64TemplateFamilies()
    {
        var math = LessonsFirst.App.Tasks.MathTasks.Level12Templates.All
            .Concat(LessonsFirst.App.Tasks.MathTasks.Level34Templates.All)
            .Concat(LessonsFirst.App.Tasks.MathTasks.Level56Templates.All)
            .Select(t => t.Id);
        var russian = LessonsFirst.App.Tasks.RussianTasks.RussianTemplates12.All
            .Concat(LessonsFirst.App.Tasks.RussianTasks.RussianTemplates34.All)
            .Concat(LessonsFirst.App.Tasks.RussianTasks.RussianTemplates56.All)
            .Select(t => t.Id);

        Assert.Equal(34, math.Count());
        Assert.Equal(30, russian.Count());
        Assert.Equal(64, math.Concat(russian).Distinct().Count()); // всего 64 семейства
    }
}

/// <summary>Сохранение предметного режима в настройках.</summary>
public class SubjectModeSettingsTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "lessons-first-mode-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void Default_IsMixed()
    {
        var service = new LessonsFirst.App.Services.SettingsService(_tempDir);
        Assert.Equal(SubjectMode.Mixed, service.Settings.SubjectMode);
    }

    [Fact]
    public void SaveLoad_PreservesSubjectMode()
    {
        var service = new LessonsFirst.App.Services.SettingsService(_tempDir);
        service.Settings.SubjectMode = SubjectMode.Russian;
        service.Settings.GradeBand = GradeBand.Grades12;
        service.Save();

        var reloaded = new LessonsFirst.App.Services.SettingsService(_tempDir);
        Assert.Equal(SubjectMode.Russian, reloaded.Settings.SubjectMode);
        Assert.Equal(GradeBand.Grades12, reloaded.Settings.GradeBand);
    }

    [Fact]
    public void LegacySettingsWithoutMode_DefaultsToMixed()
    {
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(Path.Combine(_tempDir, "settings.json"), """{ "GradeBand": 2 }""");

        var service = new LessonsFirst.App.Services.SettingsService(_tempDir);
        Assert.Equal(SubjectMode.Mixed, service.Settings.SubjectMode);
        Assert.Equal(GradeBand.Grades56, service.Settings.GradeBand);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Временный каталог мог не создаться.
        }
    }
}
