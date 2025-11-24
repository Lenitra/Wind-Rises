@echo off
echo ========================================
echo  Wind Rises - Build Docker Image
echo ========================================
echo.

REM Vérifier si Docker est en cours d'exécution
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERREUR] Docker n'est pas en cours d'execution.
    echo Veuillez demarrer Docker Desktop et reessayer.
    pause
    exit /b 1
)

echo [INFO] Docker est actif.
echo.

REM Vérifier si le fichier ZIP existe
if not exist "WindRisesWindows.zip" (
    echo [AVERTISSEMENT] Le fichier WindRisesWindows.zip n'existe pas dans ce dossier.
    echo Le build continuera, mais le telechargement ne fonctionnera pas sur le site.
    echo.
)

echo [ETAPE 1/3] Construction de l'image Docker...
docker build -t wind-rises-web .
if errorlevel 1 (
    echo [ERREUR] Echec de la construction de l'image.
    pause
    exit /b 1
)

echo.
echo [ETAPE 2/3] Export de l'image en fichier TAR...
docker save -o wind-rises-web.tar wind-rises-web
if errorlevel 1 (
    echo [ERREUR] Echec de l'export de l'image.
    pause
    exit /b 1
)

echo.
echo [ETAPE 3/3] Verification de la taille du fichier...
for %%A in (wind-rises-web.tar) do (
    set size=%%~zA
    set /a sizeMB=%%~zA/1024/1024
)
echo Taille du fichier TAR: %sizeMB% MB

echo.
echo ========================================
echo  BUILD TERMINE AVEC SUCCES !
echo ========================================
echo.
echo Le fichier wind-rises-web.tar est pret a etre transfere.
echo.
echo Pour tester localement :
echo   docker run -d -p 5555:5555 --name wind-rises wind-rises-web
echo.
echo Pour deployer sur une autre machine :
echo   1. Copiez wind-rises-web.tar sur la machine cible
echo   2. Executez le script deploy.sh
echo.
pause
