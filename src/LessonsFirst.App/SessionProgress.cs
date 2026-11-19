namespace LessonsFirst.App;

/// <summary>
/// Прогресс сессии: N верных ответов для доступа. Один TaskId засчитывается
/// ровно один раз; повторная отправка и отложенные события старого задания
/// не увеличивают счётчик.
/// </summary>
public sealed class SessionProgress
{
    private readonly HashSet<Guid> _countedTaskIds = new();

    public int Required { get; }

    public int Correct { get; private set; }

    public bool Completed => Correct >= Required;

    public SessionProgress(int required) => Required = required;

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
        _countedTaskIds.Clear();
    }
}
