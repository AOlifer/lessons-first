using System.Diagnostics;
using System.IO;
using Xunit;

namespace LessonsFirst.Tests;

/// <summary>
/// Ручная визуальная проверка WPF: запускает приложение с аргументом
/// --visual-review — оно рендерит экран замка со всеми русскими формами
/// ответов на трёх уровнях в PNG и завершается. Скриншоты просматриваются
/// вручную (см. отчёт). Запуск: LF_VISUAL=1 dotnet test --filter VisualReview
/// В CI без переменной тест завершается мгновенно.
/// </summary>
public class VisualReviewTests
{
    [Fact]
    public void AppVisualReview_ProducesScreenshots()
    {
        if (Environment.GetEnvironmentVariable("LF_VISUAL") != "1")
        {
            return;
        }

        var exe = FindAppExecutable();
        Assert.True(exe is not null, "LessonsFirst.exe не найден рядом с тестами");

        var outDir = Path.Combine(Path.GetTempPath(), "lf-visual-" + Guid.NewGuid().ToString("N")[..8]);
        var psi = new ProcessStartInfo(exe!, $"--visual-review={outDir}") { UseShellExecute = false };
        using var process = Process.Start(psi)!;
        Assert.True(process.WaitForExit(120_000), "приложение не завершило визуальную проверку за 2 минуты");

        Assert.Equal(0, process.ExitCode);
        var files = Directory.GetFiles(outDir, "*.png");
        Assert.True(files.Length >= 12, $"скриншотов {files.Length}");
        Assert.All(files, f => Assert.True(new FileInfo(f).Length > 10_000, $"пустой скриншот {f}"));
        Console.WriteLine($"Скриншоты ({files.Length}): {outDir}");
    }

    private static string? FindAppExecutable()
    {
        var baseDir = AppContext.BaseDirectory;
        var local = Path.Combine(baseDir, "LessonsFirst.exe");
        if (File.Exists(local))
        {
            return local;
        }

        // Поиск вверх доpublish-каталога репозитория.
        for (var dir = new DirectoryInfo(baseDir); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "publish", "LessonsFirst.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
