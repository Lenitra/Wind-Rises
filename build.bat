@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Wind-Rises - Build Script
echo ========================================
echo.

REM Configuration
set PROJECT_PATH=%~dp0
set PROJECT_PATH=%PROJECT_PATH:~0,-1%
set BUILD_DIR=%PROJECT_PATH%\Builds\Windows
set GAME_NAME=Wind
set ARCHIVE_NAME=Wind-Windows
set UNITY_VERSION=6000.2.12f1

REM Detection de Unity
set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe
if not exist "%UNITY_PATH%" (
    echo ERREUR: Unity %UNITY_VERSION% introuvable
    echo Chemin: %UNITY_PATH%
    echo Veuillez mettre a jour UNITY_VERSION dans ce script.
    pause
    exit /b 1
)

echo Unity trouve: %UNITY_PATH%
echo Chemin projet: %PROJECT_PATH%
echo.

REM Nettoyage du dossier de build precedent
if exist "%BUILD_DIR%" (
    echo Nettoyage du build precedent...
    rmdir /s /q "%BUILD_DIR%"
    echo Dossier nettoye.
)
echo.

REM Creation du dossier Logs si necessaire
if not exist "%PROJECT_PATH%\Logs" (
    mkdir "%PROJECT_PATH%\Logs"
)

echo Demarrage du build Unity...
echo Cela peut prendre plusieurs minutes...
echo.

REM Execution du build Unity
"%UNITY_PATH%" -quit -batchmode -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.PerformBuild -logFile "%PROJECT_PATH%\Logs\build.log"

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERREUR: Le build a echoue! Consultez: Logs\build.log
    pause
    exit /b 1
)

REM Verification que le build existe
if not exist "%BUILD_DIR%\%GAME_NAME%.exe" (
    echo.
    echo ERREUR: Executable du build introuvable!
    echo Attendu: %BUILD_DIR%\%GAME_NAME%.exe
    echo Consultez: Logs\build.log
    pause
    exit /b 1
)

echo.
echo Build termine avec succes!
echo Emplacement: %BUILD_DIR%
echo.

REM Creation de l'archive avec timestamp
echo Creation de l'archive...

REM Generer timestamp (format: YYYYMMDD-HHMMSS)
for /f "tokens=1-6 delims=/:. " %%a in ("%date% %time%") do (
    set TIMESTAMP=%%c%%b%%a-%%d%%e%%f
)
set TIMESTAMP=%TIMESTAMP: =0%
set ARCHIVE_PATH=%PROJECT_PATH%\Builds\%ARCHIVE_NAME%-%TIMESTAMP%.zip

REM Utiliser PowerShell pour compresser
powershell -Command "Compress-Archive -Path '%BUILD_DIR%' -DestinationPath '%ARCHIVE_PATH%' -Force"

if %ERRORLEVEL% neq 0 (
    echo ATTENTION: Echec de la creation de l'archive!
    goto :skip_archive
)

echo Archive creee: %ARCHIVE_PATH%

REM Afficher la taille de l'archive
for %%A in ("%ARCHIVE_PATH%") do set ARCHIVE_SIZE=%%~zA
set /a ARCHIVE_SIZE_MB=%ARCHIVE_SIZE% / 1048576
echo Taille archive: %ARCHIVE_SIZE_MB% MB
goto :done_archive

:skip_archive
echo Creation d'archive ignoree.

:done_archive
echo.
echo ========================================
echo Build termine!
echo ========================================
echo.
echo Dossier build: %BUILD_DIR%
if exist "%ARCHIVE_PATH%" (
    echo Archive: %ARCHIVE_PATH%
)
echo.

REM Ouvrir le dossier du build
explorer /select,"%BUILD_DIR%\%GAME_NAME%.exe"

pause
