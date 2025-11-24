# Wind-Rises Build Script (PowerShell)
param(
    [string]$UnityVersion = "6000.2.12f1",
    [switch]$Clean,
    [switch]$NoArchive,
    [switch]$OpenFolder
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Wind-Rises - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectPath = $PSScriptRoot
$BuildDir = Join-Path $ProjectPath "Builds\Windows"
$GameName = "Wind"
$ArchiveName = "Wind-Windows"

# Détection de Unity
$UnityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (-not (Test-Path $UnityPath)) {
    Write-Host "ERROR: Unity $UnityVersion not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Please install the correct Unity version or update the -UnityVersion parameter." -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity found: $UnityPath" -ForegroundColor Green
Write-Host "Project path: $ProjectPath"
Write-Host ""

# Nettoyage du dossier de build précédent
if ($Clean -and (Test-Path $BuildDir)) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $BuildDir -Recurse -Force
}

# Création du dossier Logs si nécessaire
$LogsDir = Join-Path $ProjectPath "Logs"
if (-not (Test-Path $LogsDir)) {
    New-Item -ItemType Directory -Path $LogsDir | Out-Null
}

Write-Host "Starting Unity build process..." -ForegroundColor Cyan
Write-Host "This may take several minutes..." -ForegroundColor Yellow
Write-Host ""

# Exécution du build Unity
$LogFile = Join-Path $ProjectPath "Logs\build.log"
$UnityArgs = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$ProjectPath`"",
    "-executeMethod", "BuildScript.PerformBuild",
    "-logFile", "`"$LogFile`""
)

$ProcessInfo = New-Object System.Diagnostics.ProcessStartInfo
$ProcessInfo.FileName = $UnityPath
$ProcessInfo.Arguments = $UnityArgs -join " "
$ProcessInfo.UseShellExecute = $false
$ProcessInfo.RedirectStandardOutput = $true
$ProcessInfo.RedirectStandardError = $true

$Process = New-Object System.Diagnostics.Process
$Process.StartInfo = $ProcessInfo
$Process.Start() | Out-Null

# Affichage de la progression
$Spinner = @('|', '/', '-', '\')
$SpinnerIndex = 0
while (-not $Process.HasExited) {
    Write-Host "`r  Building... $($Spinner[$SpinnerIndex])" -NoNewline -ForegroundColor Yellow
    $SpinnerIndex = ($SpinnerIndex + 1) % 4
    Start-Sleep -Milliseconds 200
}

$Process.WaitForExit()
Write-Host "`r                    `r" -NoNewline

if ($Process.ExitCode -ne 0) {
    Write-Host "ERROR: Build failed! Check the log file at: Logs\build.log" -ForegroundColor Red
    Write-Host ""
    Write-Host "Last 20 lines of log:" -ForegroundColor Yellow
    if (Test-Path $LogFile) {
        Get-Content $LogFile -Tail 20 | Write-Host
    }
    exit 1
}

# Vérification que le build existe
$ExePath = Join-Path $BuildDir "$GameName.exe"
if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: Build executable not found!" -ForegroundColor Red
    Write-Host "Expected: $ExePath" -ForegroundColor Yellow
    Write-Host "Check the log file at: Logs\build.log" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Build location: $BuildDir" -ForegroundColor Cyan
Write-Host ""

# Calcul de la taille du build
$BuildSize = (Get-ChildItem -Path $BuildDir -Recurse | Measure-Object -Property Length -Sum).Sum
$BuildSizeMB = [math]::Round($BuildSize / 1MB, 2)
Write-Host "Build size: $BuildSizeMB MB" -ForegroundColor Cyan

# Création de l'archive
if (-not $NoArchive) {
    Write-Host ""
    Write-Host "Creating archive..." -ForegroundColor Cyan

    $Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $ArchivePath = Join-Path $ProjectPath "Builds\$ArchiveName-$Timestamp.zip"

    try {
        Compress-Archive -Path $BuildDir -DestinationPath $ArchivePath -Force

        $ArchiveSize = (Get-Item $ArchivePath).Length
        $ArchiveSizeMB = [math]::Round($ArchiveSize / 1MB, 2)

        Write-Host "Archive created: $ArchivePath" -ForegroundColor Green
        Write-Host "Archive size: $ArchiveSizeMB MB" -ForegroundColor Cyan
    }
    catch {
        Write-Host "WARNING: Archive creation failed: $_" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build process completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Build directory: $BuildDir" -ForegroundColor White
if (-not $NoArchive -and (Test-Path $ArchivePath)) {
    Write-Host "Archive: $ArchivePath" -ForegroundColor White
}
Write-Host ""

# Ouvrir le dossier si demandé
if ($OpenFolder) {
    Start-Process explorer.exe -ArgumentList "/select,`"$ExePath`""
}
