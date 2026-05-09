@echo off
setlocal enabledelayedexpansion
REM ============================================================================
REM Build script for UFDC SMaRT Installer
REM This script builds the SMaRT solution and compiles the Inno Setup installer
REM ============================================================================

echo ============================================================================
echo Building UFDC SMaRT Installer
echo ============================================================================
echo.

REM Set paths
set "SOLUTION_PATH=..\SMaRT.sln"
set "ISS_SCRIPT=SMaRT.iss"
set "OUTPUT_DIR=Output"
set "DIST_DIR=..\dist"

REM Check if MSBuild is available
where msbuild >nul 2>&1
if errorlevel 1 (
    echo ERROR: MSBuild not found in PATH
    echo Please run this script from a Visual Studio Developer Command Prompt
    echo or add MSBuild to your PATH
    exit /b 1
)

REM Check if Inno Setup compiler is available
set "ISCC_PATH=C:\Users\rbernardy\AppData\Local\Programs\Inno Setup 6\ISCC.exe"
if not exist "!ISCC_PATH!" (
    echo ERROR: Inno Setup compiler not found at: !ISCC_PATH!
    echo Please install Inno Setup 6 or update the ISCC_PATH variable
    exit /b 1
)

echo Step 1: Building SMaRT solution (Release configuration)...
echo ----------------------------------------------------------------------------
msbuild "!SOLUTION_PATH!" /p:Configuration=Release "/p:Platform=Any CPU" /v:minimal
if errorlevel 1 (
    echo ERROR: Solution build failed
    exit /b 1
)
echo Solution build completed successfully
echo.

echo Step 2: Compiling Inno Setup installer...
echo ----------------------------------------------------------------------------
"!ISCC_PATH!" "!ISS_SCRIPT!"
if errorlevel 1 (
    echo ERROR: Inno Setup compilation failed
    exit /b 1
)
echo Installer compilation completed successfully
echo.

echo Step 3: Copying installer to distribution folder...
echo ----------------------------------------------------------------------------
if not exist "!DIST_DIR!" mkdir "!DIST_DIR!"
copy "!OUTPUT_DIR!\UFDC_SMART_Setup_3.52.8.exe" "!DIST_DIR!\" >nul
if errorlevel 1 (
    echo WARNING: Failed to copy installer to distribution folder
) else (
    echo Installer copied to: !DIST_DIR!\UFDC_SMART_Setup_3.52.8.exe
)
echo.

echo ============================================================================
echo Build completed successfully!
echo Installer location: !OUTPUT_DIR!\UFDC_SMART_Setup_3.52.8.exe
echo ============================================================================

endlocal
