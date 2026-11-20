namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Шаблон русского задания: по TemplateId и случайно выбранным допустимым
/// параметрам (лексема/форма/признак/конструкция/ветка) строит задание
/// с структурированным правильным ответом. Правильный ответ берётся из
/// словарных данных банка, а не вычисляется по окончанию.
/// </summary>
public interface IRussianTemplate
{
    /// <summary>Постоянный идентификатор: «Russian.R12-01»…«Russian.R56-10».</summary>
    string Id { get; }

    /// <summary>Строит новый экземпляр; key — канонический ключ варианта
    /// (предмет, шаблон, ветка, лексема/форма, признак, контекст), не TaskId и не оформление.</summary>
    SchoolTask Build(Random rng, out string key);

    /// <summary>Полный детерминированный перебор всех вариантов шаблона
    /// (используется для гарантированного завершения и отчёта покрытия банка).</summary>
    IEnumerable<(SchoolTask Task, string Key)> AllVariants();
}

/// <summary>Базовый помощник русских шаблонов.</summary>
public abstract class RussianTemplateBase : IRussianTemplate
{
    protected const string Subject = RussianLexicon.Subject;

    public abstract string Id { get; }

    public abstract SchoolTask Build(Random rng, out string key);

    public abstract IEnumerable<(SchoolTask Task, string Key)> AllVariants();

    protected static int Int(Random rng, int minInclusive, int maxInclusive) =>
        rng.Next(minInclusive, maxInclusive + 1);

    protected static T Pick<T>(Random rng, IReadOnlyList<T> list) => list[rng.Next(list.Count)];

    /// <summary>Политика для однобуквенных и фрагментных ответов: регистр не важен, е/ё различаются.</summary>
    protected static AnswerPolicy LetterPolicy { get; } = new() { CaseSensitive = false, YoDistinct = true };

    /// <summary>Политика для словоформ: регистр не важен, е/ё не различаются (допустимая вариативность печати).</summary>
    protected static AnswerPolicy WordFormPolicy { get; } = new() { CaseSensitive = false, YoDistinct = false, IgnoreStressMark = true };

    /// <summary>Политика регистровых упражнений (R12-08/09): регистр значим.</summary>
    protected static AnswerPolicy CaseSensitivePolicy { get; } = new() { CaseSensitive = true };

    protected static void Shuffle<T>(IList<T> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>Варианты SingleChoice с постоянными Id; порядок перемешивается.</summary>
    protected static List<ChoiceOption> ShuffledOptions(Random rng, params (string Id, string Text)[] options)
    {
        var list = options.Select(o => new ChoiceOption(o.Id, o.Text)).ToList();
        Shuffle(list, rng);
        return list;
    }
}

/// <summary>
/// Движок русских заданий уровня: перемешанный цикл TemplateId (каждый тип
/// один раз за цикл, затем набор перемешивается заново), дедупликация по
/// каноническим ключам с учётом веток, ограниченное число случайных попыток,
/// детерминированный перебор остатка и сброс истории только при фактическом
/// исчерпании всех вариантов шаблона (всех его веток).
/// </summary>
public sealed class RussianEngine
{
    private const int RandomAttempts = 60;

    private readonly Random _rng;
    private readonly GradeBand _band;
    private readonly IRussianTemplate[] _templates;
    private readonly Dictionary<string, HashSet<string>> _used = new();
    private readonly List<string> _cycle = new();
    private int _cycleIndex;
    private string? _lastTemplateId;

    public RussianEngine(Random rng, GradeBand band)
    {
        _rng = rng;
        _band = band;
        _templates = band switch
        {
            GradeBand.Grades12 => RussianTemplates12.All,
            GradeBand.Grades34 => RussianTemplates34.All,
            _ => RussianTemplates56.All,
        };
    }

    public IReadOnlyList<string> TemplateIds => _templates.Select(t => t.Id).ToList();

    /// <summary>Создаёт следующее русское задание (вызов = продвижение цикла).</summary>
    public SchoolTask Next()
    {
        var template = TakeTemplate();
        var used = History(template.Id);

        for (var attempt = 0; attempt < RandomAttempts; attempt++)
        {
            var task = template.Build(_rng, out var key);
            if (used.Add(key))
            {
                _lastTemplateId = template.Id;
                return task with { Difficulty = _band };
            }
        }

        // Случайные попытки исчерпаны — детерминированный поиск неиспользованного
        // варианта по всем веткам шаблона (лимит попыток не означает исчерпание).
        foreach (var (candidate, key) in template.AllVariants())
        {
            if (used.Add(key))
            {
                _lastTemplateId = template.Id;
                return candidate with { Difficulty = _band };
            }
        }

        // Фактическое исчерпание всех веток шаблона: сбрасываем только его историю.
        used.Clear();
        var fresh = template.Build(_rng, out var freshKey);
        used.Add(freshKey);
        _lastTemplateId = template.Id;
        return fresh with { Difficulty = _band };
    }

    private IRussianTemplate TakeTemplate()
    {
        if (_cycle.Count == 0 || _cycleIndex >= _cycle.Count)
        {
            _cycle.Clear();
            _cycle.AddRange(_templates.Select(t => t.Id));
            Shuffle(_cycle);
            _cycleIndex = 0;
        }

        var id = _cycle[_cycleIndex];
        // Одинаковые типы подряд запрещены: на стыке циклов меняем местами.
        if (_lastTemplateId == id && _cycle.Count > 1)
        {
            var swapWith = (_cycleIndex + 1) % _cycle.Count;
            (_cycle[_cycleIndex], _cycle[swapWith]) = (_cycle[swapWith], _cycle[_cycleIndex]);
            id = _cycle[_cycleIndex];
        }

        _cycleIndex++;
        return _templates.First(t => t.Id == id);
    }

    private HashSet<string> History(string templateId)
    {
        if (!_used.TryGetValue(templateId, out var history))
        {
            history = new HashSet<string>();
            _used[templateId] = history;
        }

        return history;
    }

    private void Shuffle(List<string> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
