# run-tests.ps1 - compile the sources into a library and run the logic test suite.
# Pure logic tests: no WMI, no hooks, no changes to the user's settings.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out  = Join-Path $PSScriptRoot 'bin'

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path $csc)) { throw 'csc.exe not found. .NET Framework 4.x is required.' }

New-Item -ItemType Directory -Force -Path $out | Out-Null
$refs = @('/reference:System.dll','/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll','/reference:System.Management.dll')

& $csc /nologo /target:library ('/out:' + (Join-Path $out 'MacBookToolkitLib.dll')) $refs (Join-Path $root 'MacBookToolkit.cs') (Join-Path $root 'ThemeUi.cs') (Join-Path $root 'Strings.cs') (Join-Path $root 'AssemblyInfo.cs')
if ($LASTEXITCODE -ne 0) { throw 'library build failed' }

& $csc /nologo /target:exe ('/out:' + (Join-Path $out 'Tests.exe')) (@(('/reference:' + (Join-Path $out 'MacBookToolkitLib.dll')), '/reference:System.dll', '/reference:System.Windows.Forms.dll', '/reference:System.Drawing.dll') + @((Join-Path $PSScriptRoot 'Tests.cs')))
if ($LASTEXITCODE -ne 0) { throw 'tests build failed' }

& (Join-Path $out 'Tests.exe')
if ($LASTEXITCODE -ne 0) { throw ("tests failed with exit code " + $LASTEXITCODE) }
Write-Host 'ALL TESTS PASSED' -ForegroundColor Green
