# Build Instructions for WiX Installer

## Prerequisites

Before building the installer, ensure you have:

1. **WiX Toolset v3.11 or newer** installed
   - Download from: http://wixtoolset.org/releases/
   - Install the WiX Toolset build tools
   - Optionally install the Visual Studio extension for IDE integration

2. **Visual Studio 2012 or newer** (or MSBuild Tools)
   - Required to build the C# projects

3. **.NET Framework 4.8 SDK** installed
   - Required for building the SMaRT application

## Building the Installer

### Option 1: Using PowerShell Script (Recommended)

```powershell
cd WixInstaller
.\Build-Installer.ps1 -Configuration Release
```

This script will:
- Check for WiX Toolset installation
- Build all C# projects in the solution
- Build the WiX installer project
- Display the MSI location and size

### Option 2: Using Visual Studio

1. Open `SMaRT.sln` in Visual Studio
2. Set the build configuration to **Release**
3. Right-click the solution and select **Rebuild Solution**
4. Right-click the **WixInstaller** project and select **Build**
5. The MSI will be created at: `WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi`

### Option 3: Using MSBuild Command Line

```cmd
REM Build the solution first
msbuild SMaRT.sln /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild

REM Build the WiX installer
msbuild WixInstaller\WixInstaller.wixproj /p:Configuration=Release /p:Platform=x86
```

## Verifying the Build

After a successful build, verify:

1. **MSI file exists**: `WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi`
2. **MSI file size**: Should be approximately 10-20 MB (depending on dependencies)
3. **No build warnings**: Check the build output for any WiX warnings

### Using Orca to Inspect the MSI

Orca is a database table editor for MSI files (part of Windows SDK):

```cmd
REM Open the MSI in Orca
orca WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi
```

Verify in Orca:
- **Property table**: Check ProductName, ProductVersion, Manufacturer, UpgradeCode
- **File table**: Verify all expected files are listed
- **Component table**: Check component GUIDs
- **Directory table**: Verify directory structure

## Testing the Installer

### Test 1: Fresh Installation

```cmd
REM Install with verbose logging
msiexec /i WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi /l*v install.log
```

Verify:
- [ ] Installation wizard appears
- [ ] License agreement displays
- [ ] Directory selection works
- [ ] Installation completes without errors
- [ ] Files installed to: `C:\Program Files (x86)\University of Florida\SMaRT\`
- [ ] Start Menu shortcuts created: `Start Menu\Programs\UFDC SMaRT\`
- [ ] Application appears in Programs and Features
- [ ] Application launches successfully from Start Menu

### Test 2: Prerequisites Check

On a system without .NET Framework 4.8:
- [ ] Installer displays error message about missing .NET Framework 4.8
- [ ] Installation does not proceed

### Test 3: Upgrade Installation

1. Install version 3.51.0 (if available)
2. Run the 3.52.0 installer
3. Verify:
   - [ ] Old version is detected and removed
   - [ ] New version installs successfully
   - [ ] No duplicate entries in Programs and Features
   - [ ] Application runs with new version

### Test 4: Uninstallation

```cmd
REM Uninstall via command line
msiexec /x WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi /l*v uninstall.log
```

Or uninstall via Programs and Features.

Verify:
- [ ] All files removed from installation directory
- [ ] Installation directory removed (if empty)
- [ ] Start Menu shortcuts removed
- [ ] No entry in Programs and Features
- [ ] No orphaned registry entries

### Test 5: Silent Installation

```cmd
REM Silent install
msiexec /i WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi /qn /l*v silent_install.log

REM Verify installation
dir "C:\Program Files (x86)\University of Florida\SMaRT"

REM Silent uninstall
msiexec /x WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi /qn /l*v silent_uninstall.log
```

## Troubleshooting

### Build Errors

**Error: "The WiX Toolset v3.11 build tools must be installed"**
- Solution: Install WiX Toolset from http://wixtoolset.org/releases/

**Error: "Could not find file 'SMaRT.exe'"**
- Solution: Build the SMaRT project first before building the installer
- Run: `msbuild SMaRT.sln /p:Configuration=Release /t:Rebuild`

**Error: "Unresolved reference to symbol 'WixUI:WixUI_InstallDir'"**
- Solution: Ensure WixUIExtension is properly referenced in the project
- Check that WiX Toolset is installed correctly

**Error: "The system cannot find the file specified" for DLL files**
- Solution: Verify all third-party DLLs exist in the DLLs folder
- Check that project references are correct

### Installation Errors

**Error: "This installation package could not be opened"**
- Solution: The MSI file may be corrupted. Rebuild the installer.

**Error: "Another version of this product is already installed"**
- Solution: Uninstall the existing version first, or ensure upgrade logic is working

**Error: "The installer has insufficient privileges"**
- Solution: Run the installer as administrator (UAC prompt should appear automatically)

## Validation Checklist

Before releasing the installer:

- [ ] MSI builds without errors or warnings
- [ ] All required files are packaged (verify with Orca or dark.exe)
- [ ] Fresh installation works on clean Windows 11 system
- [ ] Upgrade from previous version works
- [ ] Uninstallation removes all files and registry entries
- [ ] Application launches and functions correctly after installation
- [ ] Prerequisites check works (test without .NET Framework 4.8)
- [ ] Silent installation works
- [ ] MSI file size is reasonable
- [ ] Product metadata is correct (name, version, manufacturer)
- [ ] UpgradeCode matches previous versions (if upgrading)

## Advanced: Extracting MSI Contents

To extract and inspect MSI contents without installing:

```cmd
REM Extract to a directory
msiexec /a WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi /qb TARGETDIR=C:\ExtractedMSI

REM Or use dark.exe (WiX decompiler)
dark.exe WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi -x C:\ExtractedMSI
```

## Continuous Integration

For automated builds in CI/CD:

```yaml
# Example for Azure Pipelines
steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: 'SMaRT.sln'

- task: VSBuild@1
  inputs:
    solution: 'SMaRT.sln'
    platform: 'Any CPU'
    configuration: 'Release'

- task: VSBuild@1
  inputs:
    solution: 'WixInstaller/WixInstaller.wixproj'
    platform: 'x86'
    configuration: 'Release'

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: 'WixInstaller/bin/Release'
    ArtifactName: 'installer'
```

## Support

For issues or questions:
- Check the build logs for detailed error messages
- Review the WiX documentation: http://wixtoolset.org/documentation/
- Check Windows Event Viewer for installation errors
- Enable verbose MSI logging: `/l*v install.log`
