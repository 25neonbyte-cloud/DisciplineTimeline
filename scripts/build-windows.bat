@echo off
setlocal

cd /d "%~dp0\.."

if not exist artifacts\win-x64 mkdir artifacts\win-x64

dotnet restore DisciplineTimeline.sln
if errorlevel 1 exit /b %errorlevel%

dotnet build DisciplineTimeline.sln -c Release --no-restore
if errorlevel 1 exit /b %errorlevel%

dotnet publish src\DisciplineTimeline\DisciplineTimeline.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=false ^
  -o artifacts\win-x64

if errorlevel 1 exit /b %errorlevel%

echo.
echo Build concluido: artifacts\win-x64
endlocal
