#define AppName "VolumeGuard"
#define AppVersion "1.0.3"
[Setup]
AppId={{A1484F55-FF4A-4674-8B58-01D6C5A8D311}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\VolumeGuard
DefaultGroupName=VolumeGuard
OutputDir=..\dist
OutputBaseFilename=VolumeGuard-Setup-{#AppVersion}
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
UninstallDisplayIcon={app}\VolumeGuard-{#AppVersion}.ico
SetupIconFile=..\src\VolumeGuard.Admin\Assets\VolumeGuard.ico
ShowLanguageDialog=yes
LanguageDetectionMethod=uilanguage
UsePreviousLanguage=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]
en.Configure=Configure VolumeGuard
es.Configure=Configurar VolumeGuard
en.StopFailed=Could not stop the existing service. See setup-error.log in the installation folder.
es.StopFailed=No se pudo detener el servicio existente. Consulta setup-error.log en la carpeta de instalación.
en.InstallFailed=Could not configure or start VolumeGuard. See setup-error.log in the installation folder.
es.InstallFailed=No se pudo configurar o iniciar VolumeGuard. Consulta setup-error.log en la carpeta de instalación.
en.UninstallFailed=Could not stop or remove the service. See setup-error.log in the installation folder.
es.UninstallFailed=No se pudo detener o eliminar el servicio. Consulta setup-error.log en la carpeta de instalación.
en.LanguageFailed=Could not save the application language.
es.LanguageFailed=No se pudo guardar el idioma de la aplicación.

[Dirs]
Name: "{commonappdata}\VolumeGuard"; Permissions: admins-full system-full

[Files]
Source: "..\src\VolumeGuard.Admin\Assets\VolumeGuard.ico"; DestDir: "{app}"; DestName: "VolumeGuard-{#AppVersion}.ico"; Flags: ignoreversion
Source: "Manage-Service.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "Manage-Service.ps1"; Flags: dontcopy
Source: "..\publish\Service\*"; DestDir: "{app}\Service"; Flags: recursesubdirs createallsubdirs ignoreversion
Source: "..\publish\Admin\*"; DestDir: "{app}\Admin"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{cm:Configure}"; Filename: "{app}\Admin\VolumeGuard.Admin.exe"; IconFilename: "{app}\VolumeGuard-{#AppVersion}.ico"; IconIndex: 0; AppUserModelID: "VolumeGuard.Admin"
Name: "{commondesktop}\VolumeGuard"; Filename: "{app}\Admin\VolumeGuard.Admin.exe"; IconFilename: "{app}\VolumeGuard-{#AppVersion}.ico"; IconIndex: 0; AppUserModelID: "VolumeGuard.Admin"

[InstallDelete]
Type: files; Name: "{group}\Configurar VolumeGuard.lnk"
Type: files; Name: "{group}\Configure VolumeGuard.lnk"

[Code]
function ManageService(ScriptPath, Action: String): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
    '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + ScriptPath +
    '" -Action ' + Action + ' -AppDirectory "' + ExpandConstant('{app}') + '" -Language ' + ActiveLanguage,
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if Result then Result := ResultCode = 0;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  ExtractTemporaryFile('Manage-Service.ps1');
  if not ManageService(ExpandConstant('{tmp}\Manage-Service.ps1'), 'Prepare') then
    Result := CustomMessage('StopFailed')
  else
    Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if not SaveStringToFile(ExpandConstant('{app}\Admin\language.txt'), ActiveLanguage, False) then
      RaiseException(CustomMessage('LanguageFailed'));
    if not ManageService(ExpandConstant('{app}\Manage-Service.ps1'), 'Install') then
      RaiseException(CustomMessage('InstallFailed'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    if not ManageService(ExpandConstant('{app}\Manage-Service.ps1'), 'Uninstall') then
      RaiseException(CustomMessage('UninstallFailed'));
end;

[UninstallDelete]
Type: files; Name: "{app}\Admin\language.txt"
