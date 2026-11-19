using Microsoft.Win32;

namespace LessonsFirst.App.Services;

/// <summary>Автозапуск через HKCU\Software\Microsoft\Windows\CurrentVersion\Run (без прав админа).</summary>
public static class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "LessonsFirst";

    public static bool IsEnabled(string exePath) => GetCommand() is { } cmd && cmd == Quote(exePath);

    public static void Enable(string exePath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        key.SetValue(ValueName, Quote(exePath));
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    /// <summary>Приводит запись автозапуска в соответствие с настройкой и текущим путём exe.</summary>
    public static void Sync(bool enabled, string exePath)
    {
        if (enabled)
            Enable(exePath);
        else
            Disable();
    }

    private static string? GetCommand()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        return key?.GetValue(ValueName) as string;
    }

    private static string Quote(string path) => $"\"{path}\"";
}
