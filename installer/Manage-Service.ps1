param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Prepare', 'Install', 'Uninstall')]
    [string]$Action,
    [Parameter(Mandatory = $true)]
    [string]$AppDirectory,
    [ValidateSet('es', 'en')]
    [string]$Language = 'es'
)

$ErrorActionPreference = 'Stop'
$configDirectory = Join-Path $env:ProgramData 'VolumeGuard'

function Stop-Guard {
    $service = Get-Service -Name VolumeGuard -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -ne 'Stopped') {
            Stop-Service -InputObject $service -ErrorAction Stop
            $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
        }
        $service.Dispose()
    }
}

try {
    if ($Action -eq 'Prepare') {
        Stop-Guard
        exit 0
    }
    if ($Action -eq 'Uninstall') {
        Stop-Guard
        if (Get-Service -Name VolumeGuard -ErrorAction SilentlyContinue) {
            & "$env:SystemRoot\System32\sc.exe" delete VolumeGuard
            if ($LASTEXITCODE -ne 0) { throw "No se pudo eliminar el servicio: $LASTEXITCODE" }
            $deadline = [DateTime]::UtcNow.AddSeconds(30)
            while (Get-Service -Name VolumeGuard -ErrorAction SilentlyContinue) {
                if ([DateTime]::UtcNow -ge $deadline) { throw 'El servicio sigue pendiente de eliminación. Cierra Servicios e inténtalo de nuevo.' }
                Start-Sleep -Milliseconds 250
            }
        }
        exit 0
    }

    # Refuse redirected configuration paths before writing with elevated permissions.
    if (Test-Path -LiteralPath $configDirectory) {
        $items = @(Get-Item -LiteralPath $configDirectory) + @(Get-ChildItem -LiteralPath $configDirectory -Force -Recurse)
        foreach ($item in $items) {
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "La configuración contiene un enlace o punto de redirección: $($item.FullName)"
            }
        }
    }
    [IO.Directory]::CreateDirectory($configDirectory) | Out-Null
    $acl = New-Object Security.AccessControl.DirectorySecurity
    $acl.SetAccessRuleProtection($true, $false)
    $admins = New-Object Security.Principal.SecurityIdentifier('S-1-5-32-544')
    $system = New-Object Security.Principal.SecurityIdentifier('S-1-5-18')
    $acl.SetOwner($admins)
    foreach ($identity in @($admins, $system)) {
        $rule = New-Object Security.AccessControl.FileSystemAccessRule($identity, 'FullControl', 'ContainerInherit, ObjectInherit', 'None', 'Allow')
        $acl.AddAccessRule($rule)
    }
    Set-Acl -LiteralPath $configDirectory -AclObject $acl
    # Existing children must not retain explicit permissions from an earlier installation.
    foreach ($item in Get-ChildItem -LiteralPath $configDirectory -Force -Recurse) {
        if ($item.PSIsContainer) { $childAcl = New-Object Security.AccessControl.DirectorySecurity }
        else { $childAcl = New-Object Security.AccessControl.FileSecurity }
        $childAcl.SetOwner($admins)
        $childAcl.SetAccessRuleProtection($false, $false)
        Set-Acl -LiteralPath $item.FullName -AclObject $childAcl
    }
    $settings = Join-Path $configDirectory 'settings.json'
    if (-not (Test-Path -LiteralPath $settings)) {
        [IO.File]::WriteAllText($settings, '{"MaxVolumePercent":40}')
    }

    $executable = Join-Path $AppDirectory 'Service\VolumeGuard.Service.exe'
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw "No se encuentra $executable" }
    $binaryPath = '"' + $executable + '"'
    $existing = Get-CimInstance Win32_Service -Filter "Name='VolumeGuard'"
    if ($existing) {
        $result = Invoke-CimMethod -InputObject $existing -MethodName Change -Arguments @{
            PathName = $binaryPath; StartMode = 'Automatic'; StartName = 'LocalSystem'
        }
        if ($result.ReturnValue -ne 0) { throw "No se pudo actualizar el servicio: $($result.ReturnValue)" }
    }
    else {
        New-Service -Name VolumeGuard -BinaryPathName $binaryPath -StartupType Automatic | Out-Null
    }
    $displayName = if ($Language -eq 'es') { 'VolumeGuard - Limitador de volumen' } else { 'VolumeGuard - Volume limiter' }
    $description = if ($Language -eq 'es') { 'Mantiene el volumen de salida por debajo del límite administrativo' } else { 'Keeps output volume below the administrator limit' }
    Set-Service -Name VolumeGuard -DisplayName $displayName
    & "$env:SystemRoot\System32\sc.exe" description VolumeGuard $description
    if ($LASTEXITCODE -ne 0) { throw "No se pudo configurar la descripción: $LASTEXITCODE" }
    & "$env:SystemRoot\System32\sc.exe" failure VolumeGuard reset= 86400 actions= restart/5000/restart/5000/restart/5000
    if ($LASTEXITCODE -ne 0) { throw "No se pudo configurar la recuperación: $LASTEXITCODE" }
    Remove-Item -LiteralPath (Join-Path $configDirectory 'status.json') -Force -ErrorAction SilentlyContinue
    Start-Service -Name VolumeGuard
    $service = Get-Service -Name VolumeGuard
    $service.WaitForStatus('Running', [TimeSpan]::FromSeconds(30))
    $service.Dispose()
    exit 0
}
catch {
    # The installer checks the exit code and directs the administrator to this log.
    $message = "{0:O} {1}" -f [DateTime]::UtcNow, $_.Exception.Message
    try { [IO.File]::AppendAllText((Join-Path $AppDirectory 'setup-error.log'), $message + [Environment]::NewLine) } catch { }
    Write-Error $message -ErrorAction Continue
    exit 1
}
