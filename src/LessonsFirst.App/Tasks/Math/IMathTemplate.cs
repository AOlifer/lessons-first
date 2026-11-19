namespace LessonsFirst.App.Tasks.MathTasks;

/// <summary>
/// Шаблон математического задания: по TemplateId и случайным допустимым
/// параметрам строит структурированное задание (условие + точный ответ).
/// Ответ вычисляется из параметров, без разбора русского текста.
/// </summary>
public interface IMathTemplate
{
    /// <summary>Постоянный идентификатор шаблона, например «A7».</summary>
    string Id { get; }

    /// <summary>Строит новый экземпляр задания; canonicalKey — канонические числовые параметры + ветка.</summary>
    SchoolTask Build(Random rng, out string canonicalKey);

    /// <summary>
    /// Детерминированный поиск неиспользованного варианта (для малых пространств).
    /// Возвращает false, если вариантов не осталось (фактическое исчерпание)
    /// или перебор не поддерживается.
    /// </summary>
    bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string canonicalKey)
    {
        task = null!;
        canonicalKey = "";
        return false;
    }
}

public abstract class MathTemplateBase : IMathTemplate
{
    public abstract string Id { get; }

    public abstract SchoolTask Build(Random rng, out string canonicalKey);

    public virtual bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string canonicalKey)
    {
        task = null!;
        canonicalKey = "";
        return false;
    }

    protected static int Int(Random rng, int minInclusive, int maxInclusive) =>
        rng.Next(minInclusive, maxInclusive + 1);

    /// <summary>Помощник перебора: возвращает первую сборку с неиспользованным ключом.</summary>
    protected static bool FirstUnused(
        IEnumerable<(SchoolTask Task, string Key)> variants,
        HashSet<string> used,
        out SchoolTask task,
        out string canonicalKey)
    {
        foreach (var (candidate, key) in variants)
        {
            if (!used.Contains(key))
            {
                task = candidate;
                canonicalKey = key;
                return true;
            }
        }

        task = null!;
        canonicalKey = "";
        return false;
    }
}
