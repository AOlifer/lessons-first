; Инсталлятор «Сначала уроки!» (Inno Setup 6).
; Сборка: ISCC /DVersion=x.y.z installer\lessons-first.iss
; Ожидает многофайловый publish-вывод в ..\publish\ (без PublishSingleFile —
; сжатые single-file сборки нестабильны при запуске с некоторых дисков/политик).

#ifndef Version
#define Version "0.0.0"
#endif

[Setup]
AppId={{8B4A0F1C-62D3-4A9E-B7C5-9E2D1F3A4B07}
AppName=Сначала уроки!
AppVersion={#Version}
AppPublisher=AOlifer
AppPublisherURL=https://github.com/AOlifer/lessons-first
AppSupportURL=https://github.com/AOlifer/lessons-first/issues
AppUpdatesURL=https://github.com/AOlifer/lessons-first/releases
DefaultDirName={autopf}\LessonsFirst
DefaultGroupName=Сначала уроки!
UninstallDisplayName=Сначала уроки!
UninstallDisplayIcon={app}\LessonsFirst.exe
OutputDir=..\installer-output
OutputBaseFilename=LessonsFirst-Setup
SetupIconFile=..\lessons-first-brand\icons\lessons-first.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
DisableProgramGroupPage=yes

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "watchdog"; Description: "Автозапуск при входе в Windows и слежение (перезапуск, если приложение закрыли)"; GroupDescription: "Автозапуск:"; Flags: checkedonce

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Сначала уроки!"; Filename: "{app}\LessonsFirst.exe"
Name: "{group}\{cm:UninstallProgram,Сначала уроки!}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Сначала уроки!"; Filename: "{app}\LessonsFirst.exe"; Tasks: desktopicon

[Run]
; Задача планировщика: запуск при входе любого пользователя + проверка каждые 5 минут.
; Приложение стартует от обычного пользователя (без прав администратора),
; повторные запуски отсеиваются мьютексом внутри приложения.
; Примечание: GroupId и LogonType нельзя указывать вместе (неоднозначный набор параметров).
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""$a = New-ScheduledTaskAction -Execute '{app}\LessonsFirst.exe'; $t1 = New-ScheduledTaskTrigger -AtLogOn; $t2 = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 5) -RepetitionDuration (New-TimeSpan -Days 3650); $p = New-ScheduledTaskPrincipal -GroupId 'S-1-5-32-545' -RunLevel Limited; Register-ScheduledTask -TaskName 'LessonsFirstStart' -Action $a -Trigger $t1,$t2 -Principal $p -Force"""; \
    Flags: runhidden; Tasks: watchdog; StatusMsg: "Настройка автозапуска и слежения…"
Filename: "{app}\LessonsFirst.exe"; Description: "{cm:LaunchProgram,Сначала уроки!}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Приложение может работать — останавливаем перед удалением, снимаем задачу планировщика.
Filename: "powershell.exe"; Parameters: "-NoProfile -Command ""Get-Process LessonsFirst -ErrorAction SilentlyContinue | Stop-Process -Force"""; Flags: runhidden; RunOnceId: "StopApp"
Filename: "schtasks.exe"; Parameters: "/Delete /TN LessonsFirstStart /F"; Flags: runhidden; RunOnceId: "DelTask"
