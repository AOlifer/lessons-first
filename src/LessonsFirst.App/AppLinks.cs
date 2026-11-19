using System.Reflection;

namespace LessonsFirst.App;

/// <summary>
/// Внешние ссылки приложения. Значения задаются переменными проекта
/// (csproj: RepoSlug, DonateUrl), попадают в сборку как AssemblyMetadata
/// и читаются здесь отражением.
/// </summary>
public static class AppLinks
{
    private static readonly Dictionary<string, string> Metadata =
        Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .GroupBy(a => a.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

    private static string Get(string key) =>
        Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : "";

    public static string RepoSlug => Get("RepoSlug");

    public static string DonateUrl => Get("DonateUrl");

    public static string RepositoryUrl => $"https://github.com/{RepoSlug}";

    public static string IssuesUrl => $"https://github.com/{RepoSlug}/issues";

    public static string ReleasesUrl => $"https://github.com/{RepoSlug}/releases";
}
