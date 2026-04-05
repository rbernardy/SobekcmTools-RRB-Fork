# ============================================================================
# Build script for UFDC SMaRT Installer (PowerShell)
# This script builds the SMaRT solution and compiles the Inno Setup installer
# ============================================================================

param(
    [string]$Version = "3.52.0",
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU"
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "Building UFDC SMaRT Installer v$Version" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# Set paths
$SolutionPath = Join-Path $PSScriptRoot "..\SMaRT.sln"
$IssScript = Join-Path $PSScriptRoot "SMaRT.iss"
$OutputDir = Join-Path $PSScriptRoot "Output"
$DistDir = Join-Path $PSScriptRoot "..\dist"
$IsccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

# Function to find MSBuild
function Find-MSBuild {
    # Try to find MSBuild in common locations
    $msbuildPaths = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    )
    
    foreach ($path in $msbuildPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    # Try to find in PATH
    $msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($msbuild) {
        return $msbuild.Source
    }
    
    return $null
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

$MSBuildPath = Find-MSBuild
if (-not $MSBuildPath) {
    Write-Host "ERROR: MSBuild not found" -ForegroundColor Red
    Write-Host "Please install Visual Studio or run from a Developer Command Prompt" -ForegroundColor Red
    exit 1
}
Write-Host "  MSBuild found: $MSBuildPath" -ForegroundColor Green

if (-not (Test-Path $IsccPath)) {
    Write-Host "ERROR: Inno Setup compiler not found at: $IsccPath" -ForegroundColor Red
    Write-Host "Please install Inno Setup 6 from https://jrsoftware.org/isinfo.php" -ForegroundColor Red
    exit 1
}
Write-Host "  Inno Setup found: $IsccPath" -ForegroundColor Green

if (-not (Test-Path $SolutionPath)) {
    Write-Host "ERROR: Solution file not found: $SolutionPath" -ForegroundColor Red
    exit 1
}
Write-Host "  Solution found: $SolutionPath" -ForegroundColor Green

Write-Host ""

# Step 1: Build solution
Write-Host "Step 1: Building SMaRT solution ($Configuration configuration)..." -ForegroundColor Yellow
Write-Host "----------------------------------------------------------------------------" -ForegroundColor Gray

try {
    & $MSBuildPath $SolutionPath /p:Configuration=$Configuration /p:Platform=$Platform /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild failed with exit code $LASTEXITCODE"
    }
    Write-Host "Solution build completed successfully" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "ERROR: Solution build failed - $_" -ForegroundColor Red
    exit 1
}

# Step 2: Compile Inno Setup installer
Write-Host "Step 2: Compiling Inno Setup installer..." -ForegroundColor Yellow
Write-Host "----------------------------------------------------------------------------" -ForegroundColor Gray

try {
    & $IsccPath $IssScript
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed with exit code $LASTEXITCODE"
    }
    Write-Host "Installer compilation completed successfully" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "ERROR: Inno Setup compilation failed - $_" -ForegroundColor Red
    exit 1
}

# Step 3: Copy to distribution folder
Write-Host "Step 3: Copying installer to distribution folder..." -ForegroundColor Yellow
Write-Host "----------------------------------------------------------------------------" -ForegroundColor Gray

$InstallerFile = Join-Path $OutputDir "UFDC_SMART_Setup_$Version.exe"
if (-not (Test-Path $InstallerFile)) {
    Write-Host "WARNING: Installer file not found: $InstallerFile" -ForegroundColor Yellow
}
else {
    if (-not (Test-Path $DistDir)) {
        New-Item -ItemType Directory -Path $DistDir | Out-Null
    }
    
    $DestFile = Join-Path $DistDir "UFDC_SMART_Setup_$Version.exe"
    Copy-Item $InstallerFile $DestFile -Force
    
    $FileSize = (Get-Item $DestFile).Length / 1MB
    Write-Host "Installer copied to: $DestFile" -ForegroundColor Green
    Write-Host "Installer size: $([math]::Round($FileSize, 2)) MB" -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Installer location: $InstallerFile" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
