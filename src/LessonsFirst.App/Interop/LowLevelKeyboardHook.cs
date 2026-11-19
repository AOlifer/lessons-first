using System.Runtime.InteropServices;

namespace LessonsFirst.App.Interop;

/// <summary>
/// Низкоуровневый хук клавиатуры (WH_KEYBOARD_LL). Пока замок активен, глушит
/// Win, Alt+Tab, Alt+F4, Ctrl+Esc / Ctrl+Shift+Esc — стандартный приём киоск-приложений.
/// Хук обязан стоять на потоке с циклом сообщений (UI-поток WPF).
/// </summary>
internal sealed class LowLevelKeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int HC_ACTION = 0;

    private const int VK_TAB = 0x09;
    private const int VK_MENU = 0x12;       // Alt
    private const int VK_CONTROL = 0x11;
    private const int VK_SHIFT = 0x10;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;
    private const int VK_F4 = 0x73;

    private readonly HookProc _proc;
    private IntPtr _hookId = IntPtr.Zero;

    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    public LowLevelKeyboardHook() => _proc = HookCallback;

    public void Install()
    {
        if (_hookId != IntPtr.Zero)
            return;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);
    }

    public void Uninstall()
    {
        if (_hookId == IntPtr.Zero)
            return;
        UnhookWindowsHookEx(_hookId);
        _hookId = IntPtr.Zero;
    }

    public void Dispose() => Uninstall();

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode == HC_ACTION)
        {
            // KBDLLHOOKSTRUCT: { int vkCode; int scanCode; int flags; int time; IntPtr extraInfo; }
            var vk = Marshal.ReadInt32(lParam);
            if (ShouldBlock(vk))
                return (IntPtr)1;
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static bool ShouldBlock(int vk) => vk switch
    {
        VK_LWIN or VK_RWIN => true,
        VK_TAB when IsPressed(VK_MENU) => true,              // Alt+Tab
        VK_F4 when IsPressed(VK_MENU) => true,               // Alt+F4
        VK_ESCAPE when IsPressed(VK_CONTROL) => true,        // Ctrl+Esc и Ctrl+Shift+Esc (диспетчер задач)
        _ => false,
    };

    private static bool IsPressed(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
