namespace LessonsFirst.App;

/// <summary>
/// Прогресс сессии: N верных ответов для доступа. Задание, в котором была ошибка,
/// не зачитывается никогда — даже после верного ответа — и увеличивает цель на
/// одну задачу: сколько заданий не удалось решить с первого раза, столько
/// дополнительных и добавится. Один TaskId засчитывается ровно один раз;
/// повторная отправка и отложенные события старого задания не увеличивают счётчик.
/// </summary>
public sealed class SessionProgress
{
    private readonly HashSet<Guid> _countedTaskIds = new();
    private readonly HashSet<Guid> _failedTaskIds = new();

    public int InitialRequired { get; }

    /// <summary>Текущая цель: стартовое число задач + по одной за каждое проваленное с первого раза.</summary>
    public int Required => InitialRequired + _failedTaskIds.Count;

    public int Correct { get; private set; }

    public bool Completed => Correct >= Required;

    public SessionProgress(int initialRequired) => InitialRequired = initialRequired;

    /// <summary>
    /// Зарегистрировать ошибку в задании: оно больше не может быть зачтено, а цель
    /// вырастет на одну задачу. false — задание уже провалено ранее.
    /// </summary>
    public bool RegisterMistake(Guid taskId) => _failedTaskIds.Add(taskId);

    /// <summary>Засчитать верный ответ. false — уже зачтён, провален ранее или сессия завершена.</summary>
    public bool RegisterCorrect(Guid taskId)
    {
        if (Completed || _failedTaskIds.Contains(taskId) || !_countedTaskIds.Add(taskId))
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
        _failedTaskIds.Clear();
    }
}
