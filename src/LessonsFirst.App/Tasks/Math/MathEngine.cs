namespace LessonsFirst.App.Tasks.MathTasks;

/// <summary>
/// Движок математических заданий уровня.
/// — перемешанный цикл TemplateId: каждый тип встречается один раз за цикл, затем набор перемешивается заново;
/// — дедупликация по каноническим числовым параметрам (в ключе — ветка операции/отношения);
/// — ограниченное число случайных попыток; при исчерпании — детерминированный поиск
///   оставшегося варианта, и только при фактическом исчерпании — сброс истории шаблона;
/// — одинаковые задания подряд исключены.
/// </summary>
public sealed class MathEngine
{
    private const int RandomAttempts = 50;

    private readonly Random _rng;
    private readonly IMathTemplate[] _templates;
    private readonly Dictionary<string, HashSet<string>> _used = new();
    private readonly List<string> _cycle = new();
    private int _cycleIndex;
    private string? _lastTemplateId;

    public MathEngine(Random rng, GradeBand band)
    {
        _rng = rng;
        _templates = band switch
        {
            GradeBand.Grades12 => Level12Templates.All,
            GradeBand.Grades34 => Level34Templates.All,
            _ => Level56Templates.All,
        };
    }

    public IReadOnlyList<string> TemplateIds => _templates.Select(t => t.Id).ToList();

    /// <summary>Создаёт следующее математическое задание (вызов = выбор задания, цикл продвигается).</summary>
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
                return task;
            }
        }

        // Случайные попытки исчерпаны: детерминированный поиск незанятого варианта.
        if (template.TryPickUnused(used, _rng, out var fallback, out var fallbackKey))
        {
            used.Add(fallbackKey);
            _lastTemplateId = template.Id;
            return fallback;
        }

        // Фактическое исчерпание вариантов шаблона (или перебор не поддержан): сбрасываем только его историю.
        used.Clear();
        var fresh = template.Build(_rng, out var freshKey);
        used.Add(freshKey);
        _lastTemplateId = template.Id;
        return fresh;
    }

    private IMathTemplate TakeTemplate()
    {
        if (_cycle.Count == 0 || _cycleIndex >= _cycle.Count)
        {
            _cycle.Clear();
            _cycle.AddRange(_templates.Select(t => t.Id));
            Shuffle(_cycle);
            _cycleIndex = 0;
        }

        var id = _cycle[_cycleIndex];
        // Одинаковые задания подряд запрещены: на стыке циклов пропускаем повтор того же типа.
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
