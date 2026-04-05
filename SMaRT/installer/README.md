# UFDC SMaRT Installer

This directory contains the Inno Setup installer project for UFDC SMaRT (SobekCM Management Tool).

## Overview

The installer packages the SMaRT Windows Forms application with all dependencies, third-party libraries, data files, and resources into a single executable installer for easy deployment on Windows systems.

## Features

- Single executable installer with LZMA2 compression
- Automatic detection and upgrade of previous versions
- .NET Framework 4.8 prerequisite checking
- Start Menu and optional desktop shortcuts
- Silent installation support for automated deployment
- Clean uninstallation with removal of all files and shortcuts
- Support for Windows 10 and later (64-bit)

## System Requirements

### For End Users

- Windows 10 or later (64-bit)
- .NET Framework 4.8 or later
- Administrator privileges for installation
- Approximately 100 MB of disk space

### For Building the Installer

- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Inno Setup 6.0 or later
- Windows 10 or later

## Installation

### Standard Installation

1. Download `UFDC_SMART_Setup_3.52.0.exe`
2. Double-click the installer to launch
3. Follow the installation wizard:
   - Accept the license agreement
   - Choose installation directory (default: `C:\Program Files (x86)\University of Florida\SMaRT`)
   - Choose Start Menu folder (default: `UFDC SMaRT`)
   - Select whether to create a desktop shortcut (checked by default)
4. Click Install to begin installation
5. Optionally launch SMaRT after installation completes

### Silent Installation

For automated deployment or system administrators:

```batch
REM Silent installation with default options
UFDC_SMART_Setup_3.52.0.exe /SILENT

REM Very silent installation (no UI at all)
UFDC_SMART_Setup_3.52.0.exe /VERYSILENT

REM Silent installation with custom directory
UFDC_SMART_Setup_3.52.0.exe /SILENT /DIR="C:\Custom\Path"

REM Silent installation without Start Menu folder
UFDC_SMART_Setup_3.52.0.exe /SILENT /NOICONS

REM Silent installation without desktop shortcut
UFDC_SMART_Setup_3.52.0.exe /SILENT /TASKS="!desktopicon"

REM Silent installation with custom Start Menu folder
UFDC_SMART_Setup_3.52.0.exe /SILENT /GROUP="Custom Folder"

REM Silent installation with logging
UFDC_SMART_Setup_3.52.0.exe /SILENT /LOG="C:\Logs\smart_install.log"
```

### Command-Line Parameters

- `/SILENT` - Silent mode with progress window
- `/VERYSILENT` - Very silent mode (no UI)
- `/DIR="path"` - Specify installation directory
- `/GROUP="name"` - Specify Start Menu folder name
- `/NOICONS` - Don't create Start Menu folder
- `/TASKS="taskname"` - Specify tasks to perform (use `!taskname` to skip)
- `/LOG="path"` - Create installation log file
- `/NORESTART` - Don't restart even if necessary
- `/SUPPRESSMSGBOXES` - Suppress message boxes

## Upgrading from Previous Versions

The installer automatically detects previous versions of SMaRT and handles the upgrade process:

1. Launch the new installer
2. The installer detects the existing installation
3. The old version is automatically removed
4. The new version is installed to the same location
5. Shortcuts are updated

User data and configuration files are preserved during the upgrade.

## Uninstallation

### Using Programs and Features

1. Open Windows Settings > Apps > Apps & features
2. Find "UFDC SMaRT" in the list
3. Click Uninstall
4. Confirm the uninstallation

### Using Start Menu

1. Open Start Menu
2. Navigate to UFDC SMaRT folder
3. Click "Uninstall UFDC SMaRT"
4. Confirm the uninstallation

### Silent Uninstallation

```batch
REM Find the uninstaller in the installation directory
"C:\Program Files (x86)\University of Florida\SMaRT\unins000.exe" /SILENT
```

## Troubleshooting

### Installation Fails with ".NET Framework 4.8 Required" Error

**Problem:** The installer displays an error message about missing .NET Framework 4.8.

**Solution:**
1. Download .NET Framework 4.8 from: https://dotnet.microsoft.com/download/dotnet-framework/net48
2. Install .NET Framework 4.8
3. Restart your computer
4. Run the SMaRT installer again

### Installation Fails with "Access Denied" Error

**Problem:** The installer cannot write files to the installation directory.

**Solution:**
1. Right-click the installer executable
2. Select "Run as administrator"
3. Click Yes on the UAC prompt
4. Proceed with installation

### Installer Won't Launch or Shows Corruption Error

**Problem:** The installer file is corrupted or incomplete.

**Solution:**
1. Delete the downloaded installer file
2. Download the installer again from the official source
3. Verify the file size matches the expected size
4. Run the installer

### Application Won't Launch After Installation

**Problem:** SMaRT.exe doesn't start or shows an error.

**Solution:**
1. Verify .NET Framework 4.8 is installed
2. Check Windows Event Viewer for error details
3. Try running SMaRT.exe as administrator
4. Reinstall the application
5. Contact support with error details

### Upgrade Fails or Old Version Remains

**Problem:** After installing a new version, the old version is still present.

**Solution:**
1. Manually uninstall the old version via Programs and Features
2. Delete the installation directory if it still exists
3. Run the new installer again

### Silent Installation Doesn't Work

**Problem:** Silent installation parameters are ignored or installation shows UI.

**Solution:**
1. Verify command-line syntax is correct
2. Use `/VERYSILENT` instead of `/SILENT` for completely silent installation
3. Check that you have administrator privileges
4. Review the installation log file for errors

## File Locations

After installation, files are located at:

- **Application files:** `C:\Program Files (x86)\University of Florida\SMaRT\`
- **Start Menu shortcuts:** `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\UFDC SMaRT\`
- **Desktop shortcut:** `C:\Users\[Username]\Desktop\UFDC SMaRT.lnk` (if selected)
- **Uninstaller:** `C:\Program Files (x86)\University of Florida\SMaRT\unins000.exe`

## Support

For issues, questions, or support:

- Website: http://ufdc.ufl.edu
- Email: [support contact]
- Documentation: [documentation URL]

## Version History

### Version 3.52.0
- Initial Inno Setup installer release
- Replaces previous WiX-based installer
- Improved compression and smaller installer size
- Enhanced upgrade detection and handling
- Added silent installation support
