namespace LessonsFirst.App.Tasks;

/// <summary>
/// Выдаёт задачи, чередуя математику и русский язык (не больше двух подряд одного предмета).
/// </summary>
public sealed class TaskProvider
{
    private const string MathSubject = "Математика";
    private const string RussianSubject = "Русский язык";

    private readonly Random _rng;
    private MathGenerator _math = null!;
    private RussianGenerator _russian = null!;
    private GradeBand _band;
    private string? _lastSubject;
    private int _lastSubjectStreak;

    public TaskProvider(Random? random = null)
    {
        _rng = random ?? new Random();
        Configure(GradeBand.Grades34);
    }

    /// <summary>Пересоздаёт генераторы под новый класс (вызывается при смене настроек).</summary>
    public void Configure(GradeBand band)
    {
        if (_band == band && _math is not null)
            return;

        _band = band;
        _math = new MathGenerator(band);
        _russian = new RussianGenerator(band);
    }

    public SchoolTask Next()
    {
        var wantMath = _rng.Next(2) == 0;

        var subject = wantMath ? MathSubject : RussianSubject;
        if (subject == _lastSubject && _lastSubjectStreak >= 2)
        {
            wantMath = !wantMath;
            subject = wantMath ? MathSubject : RussianSubject;
        }

        if (subject == _lastSubject)
            _lastSubjectStreak++;
        else
            _lastSubjectStreak = 1;

        _lastSubject = subject;
        return wantMath ? _math.Generate(_rng) : _russian.Generate(_rng);
    }
}
