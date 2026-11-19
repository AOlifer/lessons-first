using System.IO;

namespace LessonsFirst.App.Services;

/// <summary>
/// Простейший лог в %APPDATA%\LessonsFirst\app.log — для диагностики «почему не запустилось».
/// Никогда не бросает исключений.
/// </summary>
public static class AppLog
{
    private static readonly object Gate = new();
    private static string _path = null!;

    public static void Init(string? baseDirectory = null)
    {
        var dir = baseDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LessonsFirst");
        _path = Path.Combine(dir, "app.log");
        Write($"=== Старт: {Environment.ProcessPath}, версия {Environment.Version} ===");
    }

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                if (_path is null)
                    return;
                File.AppendAllText(_path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Лог не должен ломать приложение.
        }
    }

    public static void Error(string where, Exception ex) =>
        Write($"[ОШИБКА] {where}: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
}
