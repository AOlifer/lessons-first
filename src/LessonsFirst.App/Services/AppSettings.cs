using LessonsFirst.App.Tasks;

namespace LessonsFirst.App.Services;

public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = 1;

    public GradeBand GradeBand { get; set; } = GradeBand.Grades34;

    /// <summary>Предметный режим: математика, русский или чередование. Если настройки нет — «Смешанный».</summary>
    public SubjectMode SubjectMode { get; set; } = SubjectMode.Mixed;

    /// <summary>Сколько правильных ответов нужно для разблокировки.</summary>
    public int RequiredCorrect { get; set; } = 5;

    /// <summary>Показывать подсказки к задачам.</summary>
    public bool ShowHints { get; set; } = true;

    /// <summary>Запускаться вместе с Windows (HKCU\Software\...\Run).</summary>
    public bool AutostartEnabled { get; set; } = true;

    /// <summary>Показывать замок при запуске приложения (старт Windows).</summary>
    public bool LockOnStartup { get; set; } = true;

    /// <summary>Показывать замок после пробуждения из сна / разблокировки сеанса.</summary>
    public bool LockOnResume { get; set; } = true;

    /// <summary>PBKDF2-хэш PIN родителя (base64). null — PIN ещё не задан.</summary>
    public string? PinHash { get; set; }

    /// <summary>Соль для хэша PIN (base64).</summary>
    public string? PinSalt { get; set; }

    /// <summary>Когда последний раз проверяли обновления (UTC).</summary>
    public DateTime LastUpdateCheckUtc { get; set; } = DateTime.MinValue;
}
