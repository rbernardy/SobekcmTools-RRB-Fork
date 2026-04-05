# Build Instructions for UFDC SMaRT Installer

This document provides detailed instructions for building the Inno Setup installer for UFDC SMaRT.

## Prerequisites

### Required Software

1. **Visual Studio 2019 or later**
   - Download from: https://visualstudio.microsoft.com/
   - Required workload: .NET desktop development
   - Required component: .NET Framework 4.8 SDK

2. **Inno Setup 6.0 or later**
   - Download from: https://jrsoftware.org/isinfo.php
   - Install to default location: `C:\Program Files (x86)\Inno Setup 6\`
   - Inno Setup compiler (ISCC.exe) must be accessible

3. **Windows 10 or later**
   - 64-bit operating system required
   - Administrator privileges for building and testing

### Optional Software

- **Git** - For version control
- **Code signing certificate** - For signing the installer (production releases)

## Build Process

### Method 1: Using Build Scripts (Recommended)

#### Using Batch Script

1. Open Command Prompt or PowerShell
2. Navigate to the installer directory:
   ```batch
   cd path\to\project\installer
   ```
3. Run the build script:
   ```batch
   build.bat
   ```
4. The script will:
   - Build the SMaRT solution in Release configuration
   - Compile the Inno Setup script
   - Copy the installer to the `dist` folder
5. Find the installer at: `installer\Output\UFDC_SMART_Setup_3.52.0.exe`

#### Using PowerShell Script

1. Open PowerShell
2. Navigate to the installer directory:
   ```powershell
   cd path\to\project\installer
   ```
3. Run the build script:
   ```powershell
   .\build.ps1
   ```
4. Optional: Specify version parameter:
   ```powershell
   .\build.ps1 -Version "3.52.0"
   ```
5. Find the installer at: `installer\Output\UFDC_SMART_Setup_3.52.0.exe`

### Method 2: Manual Build

#### Step 1: Build SMaRT Solution

1. Open Visual Studio
2. Open `SMaRT.sln`
3. Select Release configuration
4. Build > Build Solution (or press Ctrl+Shift+B)
5. Verify build succeeds without errors
6. Verify output files exist in:
   - `SMaRT\bin\Release\SMaRT.exe`
   - `Custom_Grid\bin\Release\Custom_Grid.dll`
   - `SobekCM_*\bin\Release\*.dll`
   - `EngineAgnosticLayerDbAccess\bin\Release\EngineAgnosticLayerDbAccess.dll`

#### Step 2: Compile Inno Setup Script

1. Open Inno Setup Compiler
2. File > Open > Select `installer\SMaRT.iss`
3. Build > Compile (or press Ctrl+F9)
4. Wait for compilation to complete
5. Verify no errors in the compilation log
6. Find the installer at: `installer\Output\UFDC_SMART_Setup_3.52.0.exe`

### Method 3: Command Line Build

```batch
REM Build solution
msbuild SMaRT.sln /p:Configuration=Release /p:Platform="Any CPU"

REM Compile installer
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\SMaRT.iss
```

## Version Management

### Updating Version Numbers

When releasing a new version, update version numbers in these files:

1. **SMaRT/AssemblyInfo.cs**
   ```csharp
   [assembly: AssemblyVersion("3.52.0.0")]
   [assembly: AssemblyFileVersion("3.52.0.0")]
   ```

2. **SMaRT/app.config**
   ```xml
   <add name="Version" value="3.52.0"/>
   ```

3. **installer/SMaRT.iss**
   ```ini
   AppVersion=3.52.0
   OutputBaseFilename=UFDC_SMART_Setup_3.52.0
   VersionInfoVersion=3.52.0
   ```

4. **Build scripts** (if version is hardcoded)
   - `installer/build.bat` - Update filename in copy command
   - `installer/build.ps1` - Update default version parameter

### Version Numbering Convention

SMaRT uses semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR** - Incompatible API changes or major feature releases
- **MINOR** - New features, backward compatible
- **PATCH** - Bug fixes, backward compatible

Example: `3.52.0`
- Major version: 3
- Minor version: 52
- Patch version: 0

## Testing the Installer

### Pre-Release Testing Checklist

Before releasing the installer, perform these tests:

#### 1. Fresh Installation Test

1. Use a clean Windows 10/11 VM with .NET Framework 4.8
2. Run the installer
3. Verify:
   - [ ] Installation wizard displays correctly
   - [ ] License agreement page appears
   - [ ] Default installation path is correct
   - [ ] Start Menu folder is created
   - [ ] Desktop shortcut is created (if selected)
   - [ ] Application launches successfully
   - [ ] All features work correctly

#### 2. Upgrade Test

1. Install previous version (e.g., 3.51.0)
2. Run new installer (e.g., 3.52.0)
3. Verify:
   - [ ] Upgrade is detected
   - [ ] Old version is removed
   - [ ] New version is installed
   - [ ] Application launches successfully
   - [ ] User data is preserved

#### 3. Silent Installation Test

1. Run: `UFDC_SMART_Setup_3.52.0.exe /VERYSILENT /LOG="install.log"`
2. Verify:
   - [ ] Installation completes without UI
   - [ ] Files are installed correctly
   - [ ] Application launches successfully
   - [ ] Log file contains no errors

#### 4. Uninstallation Test

1. Install the application
2. Uninstall via Programs and Features
3. Verify:
   - [ ] All files are removed
   - [ ] Installation directory is removed
   - [ ] Start Menu folder is removed
   - [ ] Desktop shortcut is removed
   - [ ] No registry entries remain

#### 5. Prerequisites Test

1. Use a VM without .NET Framework 4.8
2. Run the installer
3. Verify:
   - [ ] Error message displays
   - [ ] Download link is provided
   - [ ] Installation does not proceed

#### 6. Permissions Test

1. Install the application
2. Launch as standard user (non-admin)
3. Verify:
   - [ ] Application launches successfully
   - [ ] All features work correctly

## Troubleshooting Build Issues

### MSBuild Not Found

**Problem:** Build script reports "MSBuild not found"

**Solution:**
- Run from Visual Studio Developer Command Prompt, or
- Add MSBuild to PATH:
  ```
  C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin
  ```

### Inno Setup Compiler Not Found

**Problem:** Build script reports "Inno Setup compiler not found"

**Solution:**
- Install Inno Setup 6 from https://jrsoftware.org/isinfo.php
- Update `ISCC_PATH` in build scripts if installed to non-default location

### Build Fails with Missing Dependencies

**Problem:** Solution build fails with missing assembly references

**Solution:**
- Verify all NuGet packages are restored
- Check that all DLL files exist in the `DLLs` folder
- Verify project references are correct

### Inno Setup Compilation Fails

**Problem:** ISCC.exe reports errors during compilation

**Solution:**
- Open `SMaRT.iss` in Inno Setup IDE to see detailed errors
- Verify all source files exist at specified paths
- Check that relative paths are correct
- Verify syntax of Inno Setup script

### Installer Size Too Large

**Problem:** Installer exceeds expected size (>50 MB)

**Solution:**
- Verify compression settings: `Compression=lzma2/max`
- Verify solid compression is enabled: `SolidCompression=yes`
- Check for unnecessary files in [Files] section
- Remove debug symbols and PDB files

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build Installer

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Setup NuGet
      uses: NuGet/setup-nuget@v1
    
    - name: Restore NuGet packages
      run: nuget restore SMaRT.sln
    
    - name: Build solution
      run: msbuild SMaRT.sln /p:Configuration=Release /p:Platform="Any CPU"
    
    - name: Install Inno Setup
      run: |
        choco install innosetup -y
    
    - name: Build installer
      run: |
        & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\SMaRT.iss
    
    - name: Upload installer
      uses: actions/upload-artifact@v2
      with:
        name: installer
        path: installer\Output\*.exe
```

### Azure DevOps Example

```yaml
trigger:
  tags:
    include:
    - v*

pool:
  vmImage: 'windows-latest'

steps:
- task: NuGetCommand@2
  inputs:
    command: 'restore'
    restoreSolution: 'SMaRT.sln'

- task: VSBuild@1
  inputs:
    solution: 'SMaRT.sln'
    platform: 'Any CPU'
    configuration: 'Release'

- powershell: |
    choco install innosetup -y
  displayName: 'Install Inno Setup'

- powershell: |
    & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\SMaRT.iss
  displayName: 'Build Installer'

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: 'installer\Output'
    artifactName: 'installer'
```

## Code Signing (Optional)

For production releases, sign the installer with a code signing certificate:

### Step 1: Obtain Certificate

1. Purchase code signing certificate from trusted CA
2. Install certificate to Windows certificate store
3. Note the certificate subject name

### Step 2: Configure Inno Setup

Add to `[Setup]` section in `SMaRT.iss`:

```ini
SignTool=signtool sign /n "Your Company Name" /t http://timestamp.digicert.com /fd sha256 /v $f
```

Or use PFX file:

```ini
SignTool=signtool sign /f "path\to\cert.pfx" /p "password" /t http://timestamp.digicert.com /fd sha256 /v $f
```

### Step 3: Verify Signature

After building, verify the signature:

```powershell
Get-AuthenticodeSignature installer\Output\UFDC_SMART_Setup_3.52.0.exe
```

## Distribution

### Recommended Distribution Methods

1. **Direct Download**
   - Host installer on web server
   - Provide download link to users
   - Include SHA256 checksum for verification

2. **Network Share**
   - Copy installer to network share
   - Users can run directly from share or copy locally

3. **Software Deployment Tools**
   - Use SCCM, Intune, or other deployment tools
   - Deploy using silent installation parameters

### Checksum Generation

Generate SHA256 checksum for verification:

```powershell
Get-FileHash installer\Output\UFDC_SMART_Setup_3.52.0.exe -Algorithm SHA256
```

Include checksum in release notes for users to verify download integrity.

## Support

For build issues or questions:

- Check this documentation first
- Review Inno Setup documentation: https://jrsoftware.org/ishelp/
- Contact development team
- Submit issue to project repository

## Additional Resources

- Inno Setup Documentation: https://jrsoftware.org/ishelp/
- Inno Setup Examples: https://jrsoftware.org/ishelp/index.php?topic=samples
- MSBuild Reference: https://docs.microsoft.com/en-us/visualstudio/msbuild/
- Code Signing Guide: https://docs.microsoft.com/en-us/windows/win32/seccrypto/cryptography-tools
