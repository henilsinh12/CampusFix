@echo off
cd /d "%~dp0"
echo CampusFix - restoring packages...
dotnet restore
if errorlevel 1 goto error
echo.
echo Starting CampusFix...
dotnet run
if errorlevel 1 goto error
goto end
:error
echo.
echo CampusFix did not start. Copy the error shown above and send it to ChatGPT.
pause
:end
