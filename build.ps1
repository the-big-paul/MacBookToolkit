# build.ps1 - compile MacBookToolkit.exe with the built-in .NET Framework compiler.
# No external tools needed: csc.exe ships with Windows (.NET Framework 4).
#
# Usage:   powershell -ExecutionPolicy Bypass -File build.ps1

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$src  = Join-Path $root 'MacBookToolkit.cs'
$ico  = Join-Path $root 'app.ico'
$out  = Join-Path $root 'MacBookToolkit.exe'

if (-not (Test-Path $src)) { throw "Source not found: $src" }

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path $csc)) { throw 'csc.exe not found. .NET Framework 4.x is required.' }

$args = @(
    '/nologo',
    '/target:winexe',
    ('/win32icon:' + $ico),
    ('/out:' + $out),
    '/reference:System.dll',
    '/reference:System.Windows.Forms.dll',
    '/reference:System.Drawing.dll',
    '/reference:System.Management.dll',
    $src
)

& $csc $args
if ($LASTEXITCODE -ne 0) { throw "csc failed with exit code $LASTEXITCODE" }

Write-Host ('OK: ' + $out) -ForegroundColor Green
