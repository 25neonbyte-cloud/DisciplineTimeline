@echo off
setlocal

cd /d "%~dp0\.."

if not exist artifacts\win-x64 mkdir artifacts\win-x64

dotnet --version
if errorlevel 1 exit /b %errorlevel%

dotnet tool restore
if errorlevel 1 exit /b %errorlevel%

dotnet restore DisciplineTimeline.sln
if errorlevel 1 exit /b %errorlevel%

dotnet build DisciplineTimeline.sln -c Release --no-restore
if errorlevel 1 exit /b %errorlevel%

dotnet test DisciplineTimeline.sln -c Release --no-build
if errorlevel 1 exit /b %errorlevel%

dotnet publish src\DisciplineTimeline.App\DisciplineTimeline.App.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -o artifacts\win-x64

if errorlevel 1 exit /b %errorlevel%

echo.
echo Build e testes concluidos: artifacts\win-x64
endlocal
