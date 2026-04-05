# Implementation Plan: Inno Setup Installer for SMaRT

## Overview

This implementation plan creates an Inno Setup-based installer for the SMaRT application. The installer will package the .NET Framework 4.8 Windows Forms application with all dependencies, third-party libraries, data files, and resources into a single executable installer. The implementation follows the design document's architecture and includes all required sections: [Setup], [Files], [Icons], [Tasks], [Run], and [Code].

## Tasks

- [x] 1. Create installer project structure
  - Create `installer` directory at project root
  - Create subdirectories for output and temporary files
  - _Requirements: 18.1, 18.4_

- [x] 2. Create license agreement file
  - Create `installer/license.txt` with appropriate license text
  - Include copyright information for University of Florida
  - _Requirements: 20.3, 11.6_

- [x] 3. Implement Inno Setup script - [Setup] section
  - [x] 3.1 Create `installer/SMaRT.iss` with [Setup] section
    - Generate unique AppId GUID for upgrade detection
    - Set AppName to "UFDC SMaRT"
    - Set AppVersion to "3.52.0"
    - Set AppPublisher to "University of Florida Digital Collections"
    - Set AppPublisherURL to appropriate URL
    - Configure DefaultDirName to "{autopf}\University of Florida\SMaRT"
    - Configure DefaultGroupName to "UFDC SMaRT"
    - Set OutputBaseFilename to "UFDC_SMART_Setup_3.52.0"
    - Configure Compression=lzma2/max and SolidCompression=yes
    - Set ArchitecturesInstallIn64BitMode=x64
    - Set PrivilegesRequired=admin
    - Set LicenseFile=license.txt
    - Set UninstallDisplayIcon={app}\SMaRT.exe
    - Add section comments explaining key directives
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 9.2, 9.3, 5.1, 5.2, 6.1, 15.1, 15.2, 8.2, 13.1, 20.5, 18.2, 18.3_

- [x] 4. Implement Inno Setup script - [Files] section
  - [x] 4.1 Add main application files
    - Add SMaRT.exe from SMaRT\bin\Release
    - Add SMaRT.exe.config from SMaRT\bin\Release
    - Use ignoreversion flag for all files
    - _Requirements: 1.1, 1.2, 12.2_
  
  - [x] 4.2 Add project dependency DLLs
    - Add Custom_Grid.dll from Custom_Grid\bin\Release
    - Add SobekCM_Core.dll from SobekCM_Core\bin\Release
    - Add SobekCM_Engine_Library.dll from SobekCM_Engine_Library\bin\Release
    - Add SobekCM_Resource_Database.dll from SobekCM_Resource_Database\bin\Release
    - Add SobekCM_Resource_Object.dll from SobekCM_Resource_Object\bin\Release
    - Add SobekCM_Tools.dll from SobekCM_Tools\bin\Release
    - Add EngineAgnosticLayerDbAccess.dll from EngineAgnosticLayerDbAccess\bin\Release
    - _Requirements: 1.3, 1.4, 1.5_
  
  - [x] 4.3 Add third-party library DLLs
    - Add GemBox.Spreadsheet.dll from DLLs
    - Add itextsharp.dll from DLLs
    - Add Microsoft.ApplicationBlocks.Data.dll from DLLs
    - Add Jil.dll and Sigil.dll from DLLs\JIL
    - Add protobuf-net.dll from DLLs\Protobuf-net
    - Add Recaptcha.dll from DLLs\ReCaptcha
    - Add IKVM.*.dll, saxon9he.dll, saxon9he-api.dll from DLLs\Saxon
    - Add SolrNet.dll and Microsoft.Practices.ServiceLocation.dll from DLLs\SolrNet
    - Add Zoom.Net.dll, Zoom.Net.YazSharp.dll, yaz.dll, libxml2.dll, libxslt.dll, iconv.dll, zlib1.dll from DLLs\Zoom.net
    - Add Microsoft.ApplicationServer.Caching.Client.dll and Microsoft.ApplicationServer.Caching.Core.dll from DLLs\AppFabric
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10_
  
  - [x] 4.4 Add configuration files
    - Add Zoom.Net.Factory.config from DLLs\Zoom.net
    - _Requirements: 12.1, 12.3_
  
  - [x] 4.5 Add data files
    - Add Data\* with recursesubdirs flag to preserve folder structure
    - Include searchfields.xml, sobekcm.config, stopwords.xml
    - _Requirements: 3.1, 3.2, 3.3, 3.4_
  
  - [x] 4.6 Add image resources
    - Add Images\* with recursesubdirs flag to preserve folder structure
    - Include SMaRT.ico and all other image files
    - _Requirements: 4.1, 4.2, 4.3_

- [x] 5. Implement Inno Setup script - [Icons] section
  - Create Start Menu shortcut to SMaRT.exe with SMaRT.ico icon
  - Create Start Menu uninstall shortcut
  - Create desktop shortcut with Tasks: desktopicon condition
  - Set shortcut names to "UFDC SMaRT"
  - _Requirements: 6.2, 6.3, 6.4, 6.5, 17.2, 17.3, 17.4_

- [x] 6. Implement Inno Setup script - [Tasks] section
  - Define "desktopicon" task with description "Create a &desktop shortcut"
  - Set GroupDescription to "Additional icons:"
  - Set Flags: checked to make it selected by default
  - _Requirements: 17.1, 17.5_

- [x] 7. Implement Inno Setup script - [Run] section
  - Add entry to launch SMaRT.exe after installation
  - Set Flags: nowait postinstall skipifsilent unchecked
  - Set Description: "Launch UFDC SMaRT"
  - _Requirements: 10.7_

- [x] 8. Implement Inno Setup script - [Code] section
  - [x] 8.1 Implement .NET Framework 4.8 prerequisite check
    - Write InitializeSetup() function in Pascal
    - Check for .NET Framework 4.8 using IsDotNetInstalled(net48, 0)
    - Display error message with download link if not installed
    - Return False to prevent installation if prerequisite not met
    - _Requirements: 14.1, 14.2, 14.3, 14.4_

- [x] 9. Checkpoint - Verify Inno Setup script completeness
  - Ensure all tests pass, ask the user if questions arise.

- [x] 10. Create build automation script
  - [x] 10.1 Create batch script for Windows build automation
    - Create `installer/build.bat` script
    - Add MSBuild command to build SMaRT solution in Release configuration
    - Add ISCC.exe command to compile Inno Setup script
    - Add command to copy output installer to dist folder
    - Include error handling and status messages
    - _Requirements: 18.4_
  
  - [x] 10.2 Create PowerShell script alternative
    - Create `installer/build.ps1` script
    - Implement same build steps as batch script
    - Add better error handling and logging
    - Support version parameter for automated builds
    - _Requirements: 18.4_

- [x] 11. Create documentation
  - [x] 11.1 Create README.md for installer
    - Document installer features and requirements
    - Document command-line parameters for silent installation
    - Document upgrade process
    - Include troubleshooting section
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5_
  
  - [x] 11.2 Create BUILD_INSTRUCTIONS.md
    - Document build prerequisites (Visual Studio, Inno Setup)
    - Document build process steps
    - Document version management process
    - Document testing procedures
    - Include CI/CD integration notes
    - _Requirements: 18.4_

- [x] 12. Final checkpoint - Complete implementation verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- The Inno Setup script uses relative paths to support building from different locations
- AppId GUID must remain constant across all versions for upgrade detection
- LZMA2 compression with solid compression produces the smallest installer size
- Administrator privileges are required for installation to Program Files
- Silent installation is supported via /SILENT and /VERYSILENT command-line parameters
- The installer supports x64 Windows systems (Windows 10 and later)
- All file paths in [Files] section use relative paths from the installer directory
- The design document specifies no property-based testing is needed for this infrastructure code
