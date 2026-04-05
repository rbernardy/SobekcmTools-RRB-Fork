# Build script for WiX Installer
# This script builds the SMaRT application and creates the MSI installer

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path (Split-Path -Parent $scriptPath) "SMaRT.sln"
$wixProjectPath = Join-Path $scriptPath "WixInstaller.wixproj"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SMaRT WiX Installer Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if WiX Toolset is installed
Write-Host "Checking for WiX Toolset..." -ForegroundColor Yellow
$wixPath = "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin"
if (-not (Test-Path $wixPath)) {
    $wixPath = "${env:ProgramFiles}\WiX Toolset v3.11\bin"
}
if (-not (Test-Path $wixPath)) {
    Write-Host "ERROR: WiX Toolset v3.11 not found!" -ForegroundColor Red
    Write-Host "Please install WiX Toolset from: http://wixtoolset.org/releases/" -ForegroundColor Red
    exit 1
}
Write-Host "WiX Toolset found at: $wixPath" -ForegroundColor Green
Write-Host ""

# Check if MSBuild is available
Write-Host "Checking for MSBuild..." -ForegroundColor Yellow
$msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
if (-not $msbuildPath) {
    Write-Host "ERROR: MSBuild not found!" -ForegroundColor Red
    Write-Host "Please install Visual Studio or MSBuild Tools" -ForegroundColor Red
    exit 1
}
Write-Host "MSBuild found at: $msbuildPath" -ForegroundColor Green
Write-Host ""

# Build the solution first (unless skipped)
if (-not $SkipBuild) {
    Write-Host "Building SMaRT solution ($Configuration)..." -ForegroundColor Yellow
    $buildArgs = @(
        $solutionPath,
        "/p:Configuration=$Configuration",
        "/p:Platform=`"Any CPU`"",
        "/t:Rebuild",
        "/m",
        "/v:minimal"
    )
    if ($Verbose) {
        $buildArgs[5] = "/v:detailed"
    }
    
    & $msbuildPath $buildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Solution build failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "Solution build completed successfully!" -ForegroundColor Green
    Write-Host ""
}

# Build the WiX installer
Write-Host "Building WiX Installer ($Configuration)..." -ForegroundColor Yellow
$wixBuildArgs = @(
    $wixProjectPath,
    "/p:Configuration=$Configuration",
    "/p:Platform=x86",
    "/t:Rebuild",
    "/v:minimal"
)
if ($Verbose) {
    $wixBuildArgs[4] = "/v:detailed"
}

& $msbuildPath $wixBuildArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: WiX Installer build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

$msiPath = Join-Path $scriptPath "bin\$Configuration\UFDC_SMART_3.52.0.msi"
if (Test-Path $msiPath) {
    $msiInfo = Get-Item $msiPath
    Write-Host "MSI Location: $msiPath" -ForegroundColor Cyan
    Write-Host "MSI Size: $([math]::Round($msiInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "Created: $($msiInfo.CreationTime)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To install with logging:" -ForegroundColor Yellow
    Write-Host "  msiexec /i `"$msiPath`" /l*v install.log" -ForegroundColor White
    Write-Host ""
    Write-Host "To install silently:" -ForegroundColor Yellow
    Write-Host "  msiexec /i `"$msiPath`" /qn" -ForegroundColor White
} else {
    Write-Host "WARNING: MSI file not found at expected location!" -ForegroundColor Yellow
}
