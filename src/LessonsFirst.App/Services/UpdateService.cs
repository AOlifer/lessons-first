using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace LessonsFirst.App.Services;

public sealed record UpdateInfo(Version Version, string Tag, string NotesUrl, string AssetUrl);

/// <summary>
/// Самообновление из GitHub Releases. Репозиторий задаётся переменной проекта
/// RepoSlug в csproj, ассет — LessonsFirst.exe. Скачивает новую версию и
/// подменяет работающий exe (старый переименовывается в .old и удаляется
/// при следующем запуске).
/// </summary>
public sealed class UpdateService : IDisposable
{
    private static string Repo => AppLinks.RepoSlug;
    private const string AssetName = "LessonsFirst.exe";

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
            using var response = await Http.GetAsync($"https://api.github.com/repos/{Repo}/releases/latest");
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
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    if (asset.GetProperty("name").GetString() == AssetName)
                    {
                        assetUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
            }

            if (assetUrl is null)
                return null;

            var notesUrl = root.TryGetProperty("html_url", out var html) ? html.GetString() : null;
            return new UpdateInfo(version, tag, notesUrl ?? $"https://github.com/{Repo}/releases/latest", assetUrl);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Скачивает новую версию и подменяет текущий exe. Возвращает true при успехе.</summary>
    public async Task<bool> DownloadAndApplyAsync(UpdateInfo info, Action<string>? log = null)
    {
        try
        {
            Directory.CreateDirectory(UpdatesDirectory);
            var downloadedPath = Path.Combine(UpdatesDirectory, AssetName);
            var tempPath = downloadedPath + ".part";

            log?.Invoke($"Скачиваю {info.Tag}…");
            using (var file = File.Create(tempPath))
            using (var download = await Http.GetStreamAsync(info.AssetUrl))
            {
                await download.CopyToAsync(file);
            }
            File.Move(tempPath, downloadedPath, overwrite: true);

            ApplyUpdate(downloadedPath, log);
            return true;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Обновление не удалось: {ex.Message}");
            return false;
        }
    }

    /// <summary>Подменяет работающий exe: текущий → .old, новый → на его место, запуск новой версии.</summary>
    private static void ApplyUpdate(string newFilePath, Action<string>? log)
    {
        var exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Не удалось определить путь к исполняемому файлу");

        var oldPath = exePath + ".old";
        if (File.Exists(oldPath))
            File.Delete(oldPath);

        log?.Invoke("Заменяю исполняемый файл…");
        File.Move(exePath, oldPath);
        File.Copy(newFilePath, exePath, overwrite: true);

        Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
        Application.Current.Shutdown();
    }

    /// <summary>Удаляет оставшиеся от прошлого обновления .old-файлы (вызывать при старте).</summary>
    public static void CleanupOldFiles()
    {
        try
        {
            if (Environment.ProcessPath is not { } exePath)
                return;
            var old = exePath + ".old";
            if (File.Exists(old))
                File.Delete(old);
        }
        catch
        {
            // Не критично: файл подчистим при следующем запуске.
        }
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("LessonsFirst-updater");
        client.Timeout = TimeSpan.FromMinutes(5);
        return client;
    }

    public void Dispose() => Http.Dispose();
}
