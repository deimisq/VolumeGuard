param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'Assets'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
$source = Join-Path $PSScriptRoot '../../src/VolumeGuard.Admin/Assets/VolumeGuard.ico'
$bytes = [IO.File]::ReadAllBytes($source)
$count = [BitConverter]::ToUInt16($bytes,4)
$best = 0
$bestSize = 0
for ($i = 0; $i -lt $count; $i++) {
    $entryOffset = 6 + 16 * $i
    $size = if ($bytes[$entryOffset] -eq 0) { 256 } else { [int]$bytes[$entryOffset] }
    if ($size -gt $bestSize) { $bestSize = $size; $best = $entryOffset }
}
# The source ICO contains PNG-compressed frames; read the largest frame directly.
$length = [BitConverter]::ToUInt32($bytes,$best + 8)
$offset = [BitConverter]::ToUInt32($bytes,$best + 12)
$memory = New-Object IO.MemoryStream(,$bytes[$offset..($offset + $length - 1)])
$image = [Drawing.Image]::FromStream($memory)
try {
    foreach ($entry in @(@('StoreLogo',50), @('Square44x44Logo',44), @('Square150x150Logo',150), @('ListingLogo300',300))) {
        $size = [int]$entry[1]
        $bitmap = New-Object Drawing.Bitmap($size,$size)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([Drawing.Color]::Transparent)
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.DrawImage($image, 0, 0, $size, $size)
            $bitmap.Save((Join-Path $OutputDirectory ($entry[0] + '.png')), [Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
} finally { $image.Dispose(); $memory.Dispose() }
