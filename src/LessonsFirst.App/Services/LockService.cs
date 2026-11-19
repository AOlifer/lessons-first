using System.Windows;
using Microsoft.Win32;

namespace LessonsFirst.App.Services;

/// <summary>
/// Следит за системными событиями: пробуждение из сна / выход из блокировки сеанса —
/// и по ним требует показать замок.
/// </summary>
internal sealed class LockService : IDisposable
{
    private readonly SettingsService _settings;

    public LockService(SettingsService settings) => _settings = settings;

    public event Action? LockRequested;

    public void Start()
    {
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume && _settings.Settings.LockOnResume)
            Raise();
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        // Ребёнок разблокировал сеанс (Win+L) — снова требуем уроки.
        if (e.Reason == SessionSwitchReason.SessionUnlock && _settings.Settings.LockOnResume)
            Raise();
    }

    private void Raise() => Application.Current?.Dispatcher.BeginInvoke(() => LockRequested?.Invoke());

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.SessionSwitch -= OnSessionSwitch;
    }
}
