# Съёмка скриншота самого большого видимого окна процесса:
# capture.ps1 -ProcessName LessonsFirst -Out "файл.png"
param(
    [Parameter(Mandatory = $true)][string]$ProcessName,
    [Parameter(Mandatory = $true)][string]$Out,
    [string]$TitleContains = ""
)

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class WinEnum {
    public delegate bool EnumProc(IntPtr h, IntPtr l);
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumProc cb, IntPtr l);
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
    public static extern int GetText(IntPtr h, StringBuilder sb, int max);
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr h);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
}
"@

$proc = Get-Process -Name $ProcessName -ErrorAction Stop | Select-Object -First 1
$targetPid = [uint32]$proc.Id

$deadline = [DateTime]::Now.AddSeconds(15)
$best = @{ Handle = [IntPtr]::Zero; Area = 0 }
while ([DateTime]::Now -lt $deadline -and $best.Handle -eq [IntPtr]::Zero) {
    $callback = {
        param($h, $l)
        $windowPid = [uint32]0
        [WinEnum]::GetWindowThreadProcessId($h, [ref]$windowPid) | Out-Null
        if ($windowPid -eq $script:targetPid -and [WinEnum]::IsWindowVisible($h)) {
            $sb = New-Object System.Text.StringBuilder 256
            [WinEnum]::GetText($h, $sb, 256) | Out-Null
            $title = $sb.ToString()
            if ($script:TitleContains -eq "" -or $title.Contains($script:TitleContains)) {
                $r = New-Object WinEnum+RECT
                [WinEnum]::GetWindowRect($h, [ref]$r) | Out-Null
                $area = ($r.Right - $r.Left) * ($r.Bottom - $r.Top)
                if ($area -gt $script:best.Area) {
                    $script:best.Handle = $h
                    $script:best.Area = $area
                }
            }
        }
        return $true
    }
    [WinEnum]::EnumWindows($callback, [IntPtr]::Zero) | Out-Null
    if ($best.Handle -eq [IntPtr]::Zero) { Start-Sleep -Milliseconds 500 }
}

if ($best.Handle -eq [IntPtr]::Zero) { throw "Видимое окно процесса $ProcessName не найдено" }

Start-Sleep -Milliseconds 800   # дать отрисоваться
$rect = New-Object WinEnum+RECT
[WinEnum]::GetWindowRect($best.Handle, [ref]$rect) | Out-Null

$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top
$bmp = New-Object System.Drawing.Bitmap($w, $h)
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$gfx.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
$gfx.Dispose()
New-Item -ItemType Directory -Force -Path (Split-Path $Out) | Out-Null
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "OK: $Out ($w x $h)"
