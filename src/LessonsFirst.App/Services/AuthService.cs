using System.Security.Cryptography;

namespace LessonsFirst.App.Services;

/// <summary>PIN родителя: PBKDF2 (SHA-256, 100 000 итераций) со случайной солью.</summary>
public sealed class AuthService
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    private readonly SettingsService _settings;

    public AuthService(SettingsService settings) => _settings = settings;

    public bool HasPin => !string.IsNullOrEmpty(_settings.Settings.PinHash)
        && !string.IsNullOrEmpty(_settings.Settings.PinSalt);

    /// <summary>Задаёт (или меняет) PIN. Минимум 4 символа.</summary>
    public void SetPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Trim().Length < 4)
            throw new ArgumentException("PIN должен содержать минимум 4 символа", nameof(pin));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        _settings.Settings.PinSalt = Convert.ToBase64String(salt);
        _settings.Settings.PinHash = Convert.ToBase64String(DeriveKey(pin.Trim(), salt));
        _settings.Save();
    }

    public bool VerifyPin(string? pin)
    {
        if (string.IsNullOrEmpty(pin) || !HasPin)
            return false;

        var salt = Convert.FromBase64String(_settings.Settings.PinSalt!);
        var expected = Convert.FromBase64String(_settings.Settings.PinHash!);
        var actual = DeriveKey(pin.Trim(), salt);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private static byte[] DeriveKey(string pin, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
}
