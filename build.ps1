param([string]$InnoSetupCompiler = $env:ISCC)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Instala .NET 8 SDK desde https://dotnet.microsoft.com/download/dotnet/8.0' }
$installedSdks = @(dotnet --list-sdks)
if ($LASTEXITCODE -ne 0 -or -not ($installedSdks | Where-Object { $_ -match '^([89]|[1-9][0-9]+)\.' })) {
    throw 'No se encontró un SDK de .NET 8 o posterior. Tener solo el runtime no permite compilar.'
}
$compiler = @($InnoSetupCompiler, "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:ProgramFiles\Inno Setup 6\ISCC.exe") | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) } | Select-Object -First 1
if (-not $compiler) { throw 'Instala Inno Setup 6 desde https://jrsoftware.org/isinfo.php' }
dotnet publish src/VolumeGuard.Service/VolumeGuard.Service.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/Service
if ($LASTEXITCODE -ne 0) { throw 'Error al compilar servicio' }
dotnet publish src/VolumeGuard.Admin/VolumeGuard.Admin.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/Admin
if ($LASTEXITCODE -ne 0) { throw 'Error al compilar panel' }
& $compiler installer/VolumeGuard.iss
if ($LASTEXITCODE -ne 0) { throw 'Error al generar instalador' }
Write-Host 'Instalador listo en dist\VolumeGuard-Setup-1.0.2.exe'
