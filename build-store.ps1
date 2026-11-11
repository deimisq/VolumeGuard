param(
    [Parameter(Mandatory=$true)][string]$IdentityFile,
    [ValidateSet('msix','appx')][string]$Format = 'msix',
    [string]$MakeAppxPath,
    [switch]$Development
)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$identity = Get-Content -LiteralPath $IdentityFile -Raw | ConvertFrom-Json
foreach ($field in @('Name','Publisher','PublisherDisplayName','Version')) {
    if (-not $identity.$field -or $identity.$field -match 'REPLACE_') { throw "Completa $field con la identidad de Partner Center." }
}
if ($identity.Name -notmatch '^[A-Za-z0-9][A-Za-z0-9.-]{2,49}$') { throw 'Identity Name inválido.' }
if (-not $Development -and ($identity.Name -match 'Development' -or $identity.Publisher -match 'VolumeGuardDevelopment')) {
    throw 'La identidad de desarrollo exige -Development y no se puede generar como envío Store.'
}
if ($identity.Publisher -notmatch '^CN=') { throw 'Publisher debe ser el valor completo de Partner Center, empezando por CN=.' }
$version = [version]$identity.Version
if ($version.Revision -ne 0 -or $version.Major -lt 1 -or (@($version.Major,$version.Minor,$version.Build) | Where-Object { $_ -gt 65535 -or $_ -lt 0 })) {
    throw 'Usa una versión a.b.c.0 con major >= 1 y componentes entre 0 y 65535.'
}
if (-not $MakeAppxPath) {
    $sdkRoot = "${env:ProgramFiles(x86)}/Windows Kits/10/bin"
    $MakeAppxPath = Get-ChildItem -LiteralPath $sdkRoot -Filter makeappx.exe -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.Directory.Name -eq 'x64' } | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $MakeAppxPath) {
    $MakeAppxPath = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'artifacts/store/tools/microsoft.windows.sdk.buildtools') -Filter makeappx.exe -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.Directory.Name -eq 'x64' } | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $MakeAppxPath -or -not (Test-Path -LiteralPath $MakeAppxPath)) { throw 'Instala Windows SDK (MakeAppx x64) o indica -MakeAppxPath.' }
# A unique staging directory prevents obsolete executables entering later releases.
$stage = Join-Path $PSScriptRoot ('artifacts/store/stage-' + [guid]::NewGuid().ToString('N'))
$app = Join-Path $stage 'App'
dotnet publish src/VolumeGuard.Store/VolumeGuard.Store.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:Version=$($identity.Version) -o $app
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación Store.' }
& (Join-Path $PSScriptRoot 'packaging/store/New-Assets.ps1') -OutputDirectory (Join-Path $stage 'Assets')
[xml]$manifest = Get-Content packaging/store/AppxManifest.template.xml -Raw
$ns = New-Object Xml.XmlNamespaceManager($manifest.NameTable)
$ns.AddNamespace('p','http://schemas.microsoft.com/appx/manifest/foundation/windows10')
$node = $manifest.SelectSingleNode('/p:Package/p:Identity',$ns)
$node.SetAttribute('Name', $identity.Name)
$node.SetAttribute('Publisher', $identity.Publisher)
$node.SetAttribute('Version', $identity.Version)
$manifest.SelectSingleNode('/p:Package/p:Properties/p:PublisherDisplayName',$ns).InnerText = $identity.PublisherDisplayName
$manifest.Save((Join-Path $stage 'AppxManifest.xml'))
$output = Join-Path $PSScriptRoot ('dist/store/' + $(if ($Development) { 'development' } else { 'submission' }))
[IO.Directory]::CreateDirectory($output) | Out-Null
$package = Join-Path $output ("VolumeGuard_$($identity.Version)_x64.$Format")
& $MakeAppxPath pack /d $stage /p $package /o
if ($LASTEXITCODE -ne 0) { throw 'MakeAppx rechazó el paquete.' }
Get-FileHash -LiteralPath $package -Algorithm SHA256 | Format-List
Write-Output "Paquete sin firmar: $package"
Write-Output 'Store firma al publicar. Para instalación local firma una COPIA con certificado de prueba confiable; consulta docs/store/README.es.md.'
