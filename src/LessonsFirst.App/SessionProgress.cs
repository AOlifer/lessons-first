namespace LessonsFirst.App;

/// <summary>
/// Прогресс сессии: N верных ответов для доступа (настройка родителя).
/// Задание, в котором была ошибка, не зачитывается никогда — даже после
/// верного ответа — и добавляет одну задачу к цели: сколько заданий не удалось
/// решить с первого раза, столько дополнительных и появится. Задание без
/// ошибок засчитывается ровно один раз (один TaskId — один зачёт; повторный
/// Enter, двойной клик и отложенные события не считаются).
/// </summary>
public sealed class SessionProgress
{
    private readonly HashSet<Guid> _countedTaskIds = new();
    private readonly HashSet<Guid> _failedTaskIds = new();

    public int InitialRequired { get; }

    /// <summary>Текущая цель: стартовое число задач + по одной за каждое проваленное с первого раза.</summary>
    public int Required => InitialRequired + _failedTaskIds.Count;

    public int Correct { get; private set; }

    /// <summary>Число неверных ответов за сессию (для статистики).</summary>
    public int Mistakes { get; private set; }

    public bool Completed => Correct >= Required;

    public SessionProgress(int initialRequired) => InitialRequired = initialRequired;

    /// <summary>
    /// Зафиксировать неверный ответ: задание больше не может быть зачтено, а цель
    /// вырастет на одну задачу. false — задание уже было провалено ранее.
    /// </summary>
    public bool RegisterMistake(Guid taskId)
    {
        Mistakes++;
        return _failedTaskIds.Add(taskId);
    }

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
        Mistakes = 0;
        _countedTaskIds.Clear();
        _failedTaskIds.Clear();
    }
}
