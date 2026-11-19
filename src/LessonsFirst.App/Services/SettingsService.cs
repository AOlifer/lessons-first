using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LessonsFirst.App.Services;

/// <summary>Загрузка/сохранение настроек в %APPDATA%\LessonsFirst\settings.json (атомарно).</summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public string SettingsDirectory { get; }
    public string SettingsPath { get; }
    public AppSettings Settings { get; private set; }

    public SettingsService(string? baseDirectory = null)
    {
        SettingsDirectory = baseDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LessonsFirst");
        SettingsPath = Path.Combine(SettingsDirectory, "settings.json");
        Settings = Load();
    }

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (settings is not null)
                {
                    Clamp(settings);
                    return settings;
                }
            }
        }
        catch
        {
            // Повреждённый файл настроек — откатываемся к значениям по умолчанию.
        }

        return new AppSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        var tempPath = SettingsPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(Settings, JsonOptions));
        File.Move(tempPath, SettingsPath, overwrite: true);
    }

    private static void Clamp(AppSettings s)
    {
        if (s.RequiredCorrect is < 1 or > 50)
            s.RequiredCorrect = 5;
    }
}
