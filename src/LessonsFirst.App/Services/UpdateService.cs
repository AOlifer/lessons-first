using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace LessonsFirst.App.Services;

public sealed record UpdateInfo(Version Version, string Tag, string NotesUrl, string AssetName, string SetupAssetUrl);

/// <summary>
/// Самообновление из GitHub Releases. Репозиторий задаётся переменной проекта
/// RepoSlug в csproj. Скачивает инсталлятор LessonsFirst-Setup.exe новой версии
/// и запускает его (для установки в Program Files нужен UAC-запрос — это нормально),
/// после чего приложение завершается.
/// </summary>
public sealed class UpdateService : IDisposable
{
    /// <summary>Ассет ищется по маске: имя начинается с LessonsFirst-Setup и заканчивается на .exe
    /// (инсталлятор называется LessonsFirst-Setup-{версия}.exe).</summary>
    private const string SetupAssetPrefix = "LessonsFirst-Setup";

    private static readonly HttpClient Http = CreateClient();

    public string UpdatesDirectory { get; }

    public UpdateService(string? baseDirectory = null)
    {
        UpdatesDirectory = baseDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LessonsFirst", "updates");
    }

    public Version CurrentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 1, 0);

    /// <summary>Возвращает информацию о новой версии или null (нет релизов / уже актуально / ошибка сети).</summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        try
        {
            using var response = await Http.GetAsync($"https://api.github.com/repos/{AppLinks.RepoSlug}/releases/latest");
            if (!response.IsSuccessStatusCode)
                return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = doc.RootElement;

            if (!root.TryGetProperty("tag_name", out var tagElement) || tagElement.GetString() is not { } tag)
                return null;

            var versionText = tag.TrimStart('v', 'V');
            if (!Version.TryParse(versionText, out var version) || version <= CurrentVersion)
                return null;

            string? assetUrl = null;
            string? assetName = null;
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    if (name is not null
                        && name.StartsWith(SetupAssetPrefix, StringComparison.OrdinalIgnoreCase)
                        && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        assetName = name;
                        assetUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
            }

            if (assetUrl is null || assetName is null)
                return null;

            var notesUrl = root.TryGetProperty("html_url", out var html) ? html.GetString() : null;
            return new UpdateInfo(version, tag, notesUrl ?? AppLinks.ReleasesUrl, assetName, assetUrl);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Скачивает инсталлятор новой версии и запускает его. Возвращает true, если установка началась.</summary>
    public async Task<bool> DownloadAndApplyAsync(UpdateInfo info, Action<string>? log = null)
    {
        try
        {
            Directory.CreateDirectory(UpdatesDirectory);
            var setupPath = Path.Combine(UpdatesDirectory, info.AssetName);
            var tempPath = setupPath + ".part";

            log?.Invoke($"Скачиваю {info.Tag}…");
            using (var file = File.Create(tempPath))
            using (var download = await Http.GetStreamAsync(info.SetupAssetUrl))
            {
                await download.CopyToAsync(file);
            }
            File.Move(tempPath, setupPath, overwrite: true);

            log?.Invoke("Запускаю установщик…");
            Process.Start(new ProcessStartInfo(setupPath) { UseShellExecute = true });
            Application.Current.Shutdown();
            return true;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Обновление не удалось: {ex.Message} Скачайте вручную: {info.NotesUrl}");
            return false;
        }
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("LessonsFirst-updater");
        client.Timeout = TimeSpan.FromMinutes(10);
        return client;
    }

    public void Dispose() => Http.Dispose();
}
