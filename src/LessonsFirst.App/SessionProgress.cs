namespace LessonsFirst.App;

/// <summary>
/// Прогресс сессии: N верных ответов для доступа (настройка родителя).
/// Неверный ответ оставляет то же задание и не меняет прогресс; верный —
/// увеличивает счётчик ровно один раз (один TaskId засчитывается однократно;
/// повторный Enter, двойной клик и отложенные события не считаются).
/// Цель N фиксирована и не растёт из-за ошибок.
/// </summary>
public sealed class SessionProgress
{
    private readonly HashSet<Guid> _countedTaskIds = new();

    public int InitialRequired { get; }

    public int Required => InitialRequired;

    public int Correct { get; private set; }

    /// <summary>Число неверных ответов за сессию (для статистики).</summary>
    public int Mistakes { get; private set; }

    public bool Completed => Correct >= Required;

    public SessionProgress(int initialRequired) => InitialRequired = initialRequired;

    /// <summary>Зафиксировать неверный ответ: прогресс не меняется, задание остаётся.</summary>
    public void RegisterMistake() => Mistakes++;

    /// <summary>Засчитать верный ответ. false — уже зачтён или сессия завершена.</summary>
    public bool RegisterCorrect(Guid taskId)
    {
        if (Completed || !_countedTaskIds.Add(taskId))
        {
            return false;
        }

        Correct++;
        return true;
    }

    public void Reset()
    {
        Correct = 0;
        Mistakes = 0;
        _countedTaskIds.Clear();
    }
}
