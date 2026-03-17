@echo off
REM Money Manager SCSS Development Helper
REM =========================================

echo.
echo Money Manager SCSS Development Helper
echo =====================================
echo.
echo Select an option:
echo.
echo 1. Watch SCSS files (auto-compile on change)
echo 2. Build SCSS once
echo 3. Install dependencies
echo 4. Watch SCSS + Start .NET project
echo 5. Exit
echo.

set /p choice="Enter choice (1-5): "

if "%choice%"=="1" goto watch_scss
if "%choice%"=="2" goto build_scss
if "%choice%"=="3" goto install_deps
if "%choice%"=="4" goto watch_and_run
if "%choice%"=="5" goto end

echo Invalid choice. Try again.
timeout /t 2
cls
goto start

:watch_scss
cls
echo.
echo Watching SCSS files for changes...
echo Press Ctrl+C to stop.
echo.
call npm run watch:css
goto end

:build_scss
cls
echo.
echo Building SCSS files...
echo.
call npm run build:css
echo.
echo Build complete!
timeout /t 3
goto end

:install_deps
cls
echo.
echo Installing npm dependencies...
echo.
call npm install
echo.
echo Installation complete!
timeout /t 3
goto end

:watch_and_run
cls
echo.
echo Starting SCSS watch + .NET development server...
echo.
echo Note: You may need to have two terminals open for this to work properly.
echo Terminal 1: SCSS Watch (this one)
echo Terminal 2: dotnet run
echo.
call npm run watch:css
goto end

:end
exit /b
