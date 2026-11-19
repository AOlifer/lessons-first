using System.IO;
using LessonsFirst.App.Services;
using Xunit;

namespace LessonsFirst.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "lessons-first-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void NoFile_DefaultsReturned()
    {
        var service = new SettingsService(_tempDir);

        Assert.Equal(5, service.Settings.RequiredCorrect);
        Assert.True(service.Settings.AutostartEnabled);
        Assert.Null(service.Settings.PinHash);
    }

    [Fact]
    public void SaveLoad_PreservesSettings()
    {
        var service = new SettingsService(_tempDir);
        service.Settings.GradeBand = LessonsFirst.App.Tasks.GradeBand.Grades56;
        service.Settings.RequiredCorrect = 10;
        service.Settings.AutostartEnabled = false;
        service.Save();

        var reloaded = new SettingsService(_tempDir);
        Assert.Equal(LessonsFirst.App.Tasks.GradeBand.Grades56, reloaded.Settings.GradeBand);
        Assert.Equal(10, reloaded.Settings.RequiredCorrect);
        Assert.False(reloaded.Settings.AutostartEnabled);
    }

    [Fact]
    public void CorruptedFile_DefaultsReturned()
    {
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(Path.Combine(_tempDir, "settings.json"), "{{{ не json");

        var service = new SettingsService(_tempDir);
        Assert.Equal(5, service.Settings.RequiredCorrect);
    }

    [Fact]
    public void OutOfRangeRequiredCorrect_Clamped()
    {
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(Path.Combine(_tempDir, "settings.json"),
            """{ "RequiredCorrect": 9999 }""");

        var service = new SettingsService(_tempDir);
        Assert.Equal(5, service.Settings.RequiredCorrect);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Временный каталог мог не создаться.
        }
    }
}

public class AuthServiceTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "lessons-first-auth-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void SetPin_ThenVerify_AcceptsCorrectRejectsWrong()
    {
        var settings = new SettingsService(_tempDir);
        var auth = new AuthService(settings);

        Assert.False(auth.HasPin);

        auth.SetPin("1234родитель");
        Assert.True(auth.HasPin);
        Assert.True(auth.VerifyPin("1234родитель"));
        Assert.False(auth.VerifyPin("1234"));
        Assert.False(auth.VerifyPin(null));
        Assert.False(auth.VerifyPin(""));
    }

    [Fact]
    public void SetPin_TrimsSpaces()
    {
        var settings = new SettingsService(_tempDir);
        var auth = new AuthService(settings);

        auth.SetPin(" 9876 ");
        Assert.True(auth.VerifyPin("9876"));
        Assert.False(auth.VerifyPin("9877"));
    }

    [Fact]
    public void SetPin_TooShort_Throws()
    {
        var settings = new SettingsService(_tempDir);
        var auth = new AuthService(settings);

        Assert.Throws<ArgumentException>(() => auth.SetPin("123"));
        Assert.Throws<ArgumentException>(() => auth.SetPin("   "));
    }

    [Fact]
    public void Hash_IsNotPlaintextPin()
    {
        var settings = new SettingsService(_tempDir);
        var auth = new AuthService(settings);

        auth.SetPin("секретныйпин");
        Assert.DoesNotContain("секретныйпин", settings.Settings.PinHash);
        Assert.DoesNotContain("секретныйпин", settings.Settings.PinSalt);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // ok
        }
    }
}
