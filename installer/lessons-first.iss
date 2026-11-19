; Инсталлятор «Сначала уроки!» (Inno Setup 6).
; Сборка: iscc /DVersion=x.y.z installer\lessons-first.iss
; Ожидает опубликованный single-file exe в ..\publish\LessonsFirst.exe

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
DefaultDirName={localappdata}\LessonsFirst
DefaultGroupName=Сначала уроки!
UninstallDisplayName=Сначала уроки!
UninstallDisplayIcon={app}\LessonsFirst.exe
OutputDir=..\installer-output
OutputBaseFilename=LessonsFirst-Setup
SetupIconFile=..\lessons-first-brand\icons\lessons-first.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
DisableProgramGroupPage=yes

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "Запускать при входе в Windows"; GroupDescription: "Автозапуск:"; Flags: checkedonce

[Files]
Source: "..\publish\LessonsFirst.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Сначала уроки!"; Filename: "{app}\LessonsFirst.exe"
Name: "{group}\{cm:UninstallProgram,Сначала уроки!}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Сначала уроки!"; Filename: "{app}\LessonsFirst.exe"; Tasks: desktopicon

[Registry]
; Тот же ключ, что пишет само приложение (HKCU Run); снимается при деинсталляции
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "LessonsFirst"; ValueData: """{app}\LessonsFirst.exe"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\LessonsFirst.exe"; Description: "{cm:LaunchProgram,Сначала уроки!}"; Flags: nowait postinstall skipifsilent
