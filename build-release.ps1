param(
    [string]$Version = "5.4.1"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Building MicroApp v$Version for GitHub " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. MSBuild Path
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msbuild)) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($vsPath) {
            $msbuild = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
        }
    }
}

if (-not (Test-Path $msbuild)) {
    throw "MSBuild.exe not found."
}

# 2. Makensis Path
$makensis = "C:\Program Files (x86)\NSIS\makensis.exe"
if (-not (Test-Path $makensis)) {
    $cmd = Get-Command makensis -ErrorAction SilentlyContinue
    if ($cmd) { $makensis = $cmd.Source }
    else { throw "makensis.exe not found." }
}

# Step 1: Release Build
Write-Host "`n[1/4] Compiling Release Build..." -ForegroundColor Yellow
& $msbuild MicroApp.csproj /p:Configuration=Release /p:SkipCodeSigning=true /v:m
if ($LASTEXITCODE -ne 0) { throw "MSBuild compilation failed." }

# Step 2: Bundle VC++ runtime DLLs
Write-Host "`n[2/4] Bundling VC++ Runtime DLLs and AI Model..." -ForegroundColor Yellow
foreach ($f in 'msvcp140.dll', 'msvcp140_1.dll', 'vcruntime140.dll', 'vcruntime140_1.dll') {
    Copy-Item "$env:WINDIR\System32\$f" "bin\Release\" -Force
}

$modelDir = "bin\Release\Models"
$modelFile = "$modelDir\isnet-general-use.onnx"
if (-not (Test-Path $modelFile)) {
    Write-Host "Downloading AI Model (isnet-general-use.onnx)..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force $modelDir | Out-Null
    Invoke-WebRequest -Uri "https://github.com/Mahi-BD/MicroApp/releases/download/models-1/isnet-general-use.onnx" -OutFile $modelFile
}

$distDir = "dist-v$Version"
New-Item -ItemType Directory -Force $distDir | Out-Null

# Step 3: NSIS Installers
Write-Host "`n[3/4] Building NSIS Installers (.exe)..." -ForegroundColor Yellow
Push-Location "Setup\nsis"
try {
    Write-Host "-> Building All-users installer..."
    & $makensis "/DVERSION=$Version" "/DSRC=..\..\bin\Release" MicroApp.nsi
    if ($LASTEXITCODE -ne 0) { throw "All-users installer creation failed." }

    Write-Host "-> Building Per-user installer..."
    & $makensis "/DPERUSER" "/DVERSION=$Version" "/DSRC=..\..\bin\Release" MicroApp.nsi
    if ($LASTEXITCODE -ne 0) { throw "Per-user installer creation failed." }

    Copy-Item "MicroApp-$Version-setup.exe" "..\..\$distDir\" -Force
    Copy-Item "MicroApp-$Version-peruser-setup.exe" "..\..\$distDir\" -Force
}
finally {
    Pop-Location
}

# Step 4: Portable ZIP
Write-Host "`n[4/4] Packaging Portable ZIP..." -ForegroundColor Yellow
$staging = "bin\PortableStaging"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Force "$staging\Models" | Out-Null

$releaseFiles = @(
    "MicroApp.exe", "MicroApp.exe.config",
    "AutoItX3.Assembly.dll", "AutoItX3.dll", "AutoItX3_x64.dll", "AutoIt_License.html",
    "Gma.System.MouseKeyHook.dll", "System.Buffers.dll", "System.Memory.dll",
    "System.Numerics.Vectors.dll", "System.Runtime.CompilerServices.Unsafe.dll",
    "Microsoft.ML.OnnxRuntime.dll", "onnxruntime.dll", "onnxruntime_providers_shared.dll",
    "msvcp140.dll", "msvcp140_1.dll", "vcruntime140.dll", "vcruntime140_1.dll"
)

foreach ($f in $releaseFiles) {
    Copy-Item "bin\Release\$f" "$staging\" -Force
}
Copy-Item $modelFile "$staging\Models\" -Force

$docFiles = @("LICENSE", "NOTICE.md", "README.md", "HELP.md", "CHANGELOG.md")
foreach ($d in $docFiles) {
    if (Test-Path $d) { Copy-Item $d "$staging\" -Force }
}

$zipTarget = "$distDir\MicroApp-$Version-win-x64.zip"
if (Test-Path $zipTarget) { Remove-Item $zipTarget -Force }
Compress-Archive -Path "$staging\*" -DestinationPath $zipTarget -CompressionLevel Optimal
Remove-Item $staging -Recurse -Force

# Create / Copy RELEASE description
$relDoc = @"
# MicroApp $Version — Typing Settings & Unified Always on Top Shortcut

Improvements to Typing settings and unified Always on Top shortcut management.

## Added

* **Unified Always on Top shortcut toggle:** "Always on top" is now integrated into the unified Shortcuts settings window with its own On/Off toggle switch and reset button.

## Changed

* **Typing Setting:** Renamed "Key Setting" to "Typing Setting" across the tray menu, settings dialogs, and documentation to more clearly describe its purpose.

Everything new in 5.4.0 (Screen Color Picker with Loupe & Inspector, Productivity Shortcuts Cheat Sheet Hub) is included.

## Installing

Three downloads: `MicroApp-$Version-setup.exe` (all users, Program Files), `MicroApp-$Version-peruser-setup.exe` (just you, no admin), or the portable `MicroApp-$Version-win-x64.zip`. Each is about 165 MB because the offline background-removal model is included. Details and silent switches are in [SETUP.md](https://github.com/Mahi-BD/MicroApp/blob/main/SETUP.md). Installing over an older version keeps your settings and notes.
"@

Set-Content -Path "$distDir\RELEASE.md" -Value $relDoc -Encoding UTF8
Set-Content -Path "RELEASE_v$Version.md" -Value $relDoc -Encoding UTF8

Write-Host "`n========================================" -ForegroundColor Green
Write-Host " Release build completed successfully! " -ForegroundColor Green
Write-Host " Output Directory: $distDir" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Get-ChildItem $distDir | Select-Object Name, Length
