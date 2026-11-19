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
ArchitecturesInstallIn64BitMode=x64compatible
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
; Задача 1 «LessonsFirstLogon»: запуск при входе ЛЮБОГО пользователя —
; групповой принципал «Пользователи» даёт интерактивный запуск в сеансе вошедшего.
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""$a = New-ScheduledTaskAction -Execute '{app}\LessonsFirst.exe' -Argument '--watchdog'; $t = New-ScheduledTaskTrigger -AtLogOn; $p = New-ScheduledTaskPrincipal -GroupId 'S-1-5-32-545' -RunLevel Limited; Register-ScheduledTask -TaskName 'LessonsFirstLogon' -Action $a -Trigger $t -Principal $p -Force"""; \
    Flags: runhidden; Tasks: watchdog; StatusMsg: "Настройка автозапуска при входе…"
; Задача 2 «LessonsFirstWatch»: проверка каждые 5 минут — принципал именно УСТАНАВЛИВАЮЩИЙ
; пользователь с Interactive-входом, иначе планировщик стартует процесс на скрытом
; фоновом рабочем столе (приложение «висит невидимым» и держит блокировку запуска).
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""$a = New-ScheduledTaskAction -Execute '{app}\LessonsFirst.exe' -Argument '--watchdog'; $t = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 5) -RepetitionDuration (New-TimeSpan -Days 3650); $p = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited; Register-ScheduledTask -TaskName 'LessonsFirstWatch' -Action $a -Trigger $t -Principal $p -Force"""; \
    Flags: runhidden; Tasks: watchdog; StatusMsg: "Настройка слежения (перезапуск, если закрыли)…"
Filename: "{app}\LessonsFirst.exe"; Description: "{cm:LaunchProgram,Сначала уроки!}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Приложение может работать — останавливаем перед удалением, снимаем задачи планировщика.
Filename: "powershell.exe"; Parameters: "-NoProfile -Command ""Get-Process LessonsFirst -ErrorAction SilentlyContinue | Stop-Process -Force"""; Flags: runhidden; RunOnceId: "StopApp"
Filename: "schtasks.exe"; Parameters: "/Delete /TN LessonsFirstLogon /F"; Flags: runhidden; RunOnceId: "DelTask1"
Filename: "schtasks.exe"; Parameters: "/Delete /TN LessonsFirstWatch /F"; Flags: runhidden; RunOnceId: "DelTask2"
