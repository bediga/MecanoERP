; ============================================================
;  MecanoERP — Script d'installation Inno Setup 6
;  Copyright © 2026 GISEBS
; ============================================================

#define AppName      "MecanoERP"
#define AppVersion   "1.0.0"
#define AppPublisher "GISEBS"
#define AppURL       "https://www.gisebs.com"
#define AppExeName   "MecanoERP.exe"
#define AppIcon      "..\src\MecanoERP.WPF\Assets\mecanoerp.ico"
#define PublishDir   "..\publish"

[Setup]
; Identifiant unique — NE PAS changer entre les versions
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
LicenseFile=license.txt
OutputDir=output
OutputBaseFilename=MecanoERP_Setup_v{#AppVersion}
SetupIconFile={#AppIcon}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardResizable=no
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExeName}
MinVersion=10.0
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon";  Description: "{cm:CreateDesktopIcon}";  GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunch"; Description: "Épingler à la barre des tâches"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Application publiée (auto-contenu)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";         Filename: "{app}\{#AppExeName}";  WorkingDir: "{app}"
Name: "{group}\Désinstaller {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";   Filename: "{app}\{#AppExeName}";  WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; Ajout dans "Programmes installés"
Root: HKLM; Subkey: "SOFTWARE\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: createvalueifdoesntexist
Root: HKLM; Subkey: "SOFTWARE\{#AppPublisher}\{#AppName}"; ValueType: string; ValueName: "Version";     ValueData: "{#AppVersion}"; Flags: createvalueifdoesntexist

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
// Vérification que .NET Desktop Runtime 8 est installé
function IsDotNetInstalled(): Boolean;
var
  Key: string;
begin
  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  Result := RegKeyExists(HKLM, Key) or RegKeyExists(HKCU, Key);
end;

procedure InitializeWizard();
begin
  if not IsDotNetInstalled() then
    MsgBox('.NET Desktop Runtime 8 n''est pas détecté. Veuillez l''installer depuis https://dotnet.microsoft.com/download avant de lancer MecanoERP.', mbInformation, MB_OK);
end;
