using LessonsFirst.App.Tasks.MathTasks;
using LessonsFirst.App.Tasks.RussianTasks;

namespace LessonsFirst.App.Tasks;

/// <summary>
/// Выдаёт задания согласно предметному режиму родителя:
/// — «Математика» / «Русский язык» — только один предмет;
/// — «Смешанный» — перемешанными парами [Математика, Русский]: в каждой полной паре
///   по одному заданию каждого предмета, следующая пара перемешивается заново.
/// У каждого предмета — свой независимый перемешанный цикл TemplateId,
/// который продвигается только при создании следующего задания этого предмета.
/// Неверный ответ не вызывает Next(): текущее задание остаётся.
/// </summary>
public sealed class TaskProvider
{
    private const string MathSubject = "Математика";
    private const string RussianSubject = "Русский язык";

    private readonly Random _rng;
    private RussianEngine _russian = null!;
    private MathEngine _math = null!;
    private GradeBand _band;
    private SubjectMode _mode;
    private readonly List<bool> _pair = new();

    public TaskProvider(Random? random = null)
    {
        _rng = random ?? new Random();
        Configure(GradeBand.Grades34, SubjectMode.Mixed);
    }

    /// <summary>
    /// Переконфигурация на новую сессию: уровень и режим фиксируются здесь.
    /// Движки сохраняют циклы, если уровень и режим не менялись.
    /// </summary>
    public void Configure(GradeBand band, SubjectMode mode)
    {
        if (_band == band && _mode == mode && _math is not null)
        {
            return;
        }

        _band = band;
        _mode = mode;
        _russian = new RussianEngine(_rng, band);
        _math = new MathEngine(_rng, band);
        _pair.Clear();
    }

    public SchoolTask Next()
    {
        var wantMath = _mode switch
        {
            SubjectMode.Math => true,
            SubjectMode.Russian => false,
            _ => TakeMixedPair(),
        };

        return wantMath
            ? _math.Next() with { Difficulty = _band }
            : _russian.Next() with { Subject = RussianSubject, Difficulty = _band };
    }

    /// <summary>Очередная пара [Математика, Русский] в случайном порядке; следующая пара мешается заново.</summary>
    private bool TakeMixedPair()
    {
        if (_pair.Count == 0)
        {
            _pair.Add(true);
            _pair.Add(false);
            Shuffle(_pair);
        }

        var wantMath = _pair[0];
        _pair.RemoveAt(0);
        return wantMath;
    }

    private void Shuffle(List<bool> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
