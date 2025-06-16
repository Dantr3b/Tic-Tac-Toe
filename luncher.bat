@echo off
echo Lancement du serveur...
start cmd /k "dotnet run --project Server"

timeout /t 2 >nul

echo Lancement du Client 1...
start cmd /k "dotnet run --project Client"

timeout /t 1 >nul

echo Lancement du Client 2...
start cmd /k "dotnet run --project Client"