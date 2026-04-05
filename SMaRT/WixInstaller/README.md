# WiX Installer for SMaRT Application

This directory contains the WiX Toolset installer project for the SMaRT (SobekCM Management Tool) application.

## Prerequisites

- WiX Toolset v3.11 or newer must be installed
- Download from: http://wixtoolset.org/releases/
- Visual Studio WiX extension (optional, for IDE integration)

## Building the Installer

### From Visual Studio

1. Open SMaRT.sln
2. Set build configuration to Release
3. Build the entire solution (this builds all dependencies)
4. Build the WixInstaller project
5. The MSI will be created at: `WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi`

### From Command Line

```powershell
# Build all projects in Release configuration
msbuild SMaRT.sln /p:Configuration=Release /p:Platform="Any CPU"

# Build WiX installer
msbuild WixInstaller\WixInstaller.wixproj /p:Configuration=Release /p:Platform=x86

# Output: WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi
```

## Project Structure

- `Product.wxs` - Main product definition, upgrade logic, and feature configuration
- `Files.wxs` - File and component definitions for all application files
- `WixInstaller.wixproj` - MSBuild project file with references and configuration

## Version Management

When creating a new release:

1. Update version in `Product.wxs`: `Version="X.Y.Z"`
2. Update version in `SMaRT\app.config`: `<add name="Version" value="X.Y.Z"/>`
3. Update output filename in `WixInstaller.wixproj`: `<OutputName>UFDC_SMART_X.Y.Z</OutputName>`
4. **DO NOT** change the UpgradeCode GUID - it must remain constant across all versions
5. ProductCode is auto-generated (Id="*") - no action needed

## Testing the Installer

### Installation with Logging

```cmd
msiexec /i UFDC_SMART_3.52.0.msi /l*v install.log
```

### Silent Installation

```cmd
msiexec /i UFDC_SMART_3.52.0.msi /qn
```

### Uninstallation

```cmd
msiexec /x UFDC_SMART_3.52.0.msi /qn
```

## Troubleshooting

### Build Errors

- **"WiX Toolset not found"**: Install WiX Toolset v3.11 or newer
- **"File not found"**: Ensure all dependency projects build successfully first
- **"Component GUID conflict"**: Clean and rebuild the solution

### Installation Errors

- **".NET Framework 4.8 required"**: Install .NET Framework 4.8 from Microsoft
- **"Access denied"**: Run installer as administrator (UAC prompt should appear)
- **"Another version is installed"**: Uninstall the old version first if upgrade fails

## Important GUIDs

- **UpgradeCode**: `{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}` (defined in Product.wxs)
  - This GUID must NEVER change across versions
  - It enables Windows Installer to detect and upgrade previous installations

- **ProductCode**: Auto-generated for each build (Id="*")
  - Ensures each version is uniquely identified by Windows Installer

## Maintenance Notes

- All file paths use MSBuild variables for portability
- Component GUIDs are auto-generated (Guid="*")
- The installer uses the "early" removal strategy (RemoveExistingProducts after InstallInitialize)
- Platform is set to x86 for .NET Framework 4.8 compatibility on 64-bit Windows
