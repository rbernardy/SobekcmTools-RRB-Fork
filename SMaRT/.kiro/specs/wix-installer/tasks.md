# Implementation Plan: WiX Installer for SMaRT Application

## Overview

This plan implements a WiX Toolset v3.x installer for the SMaRT application. The implementation creates a professional MSI installer that packages the application, dependencies, data files, and resources with proper upgrade logic and Windows installation best practices.

## Tasks

- [x] 1. Create WiX project structure and configuration
  - Create WixInstaller directory at solution root
  - Create WixInstaller.wixproj with MSBuild configuration
  - Add WiX extensions references (WixUIExtension, WixNetFxExtension)
  - Configure project references to SMaRT and dependency projects
  - Set output filename to UFDC_SMART_3.52.0.msi
  - _Requirements: 11.5_

- [x] 2. Implement Product.wxs with product metadata and upgrade logic
  - [x] 2.1 Define product metadata and directory structure
    - Create Product.wxs file
    - Define Product element with name, version, manufacturer
    - Generate and set stable UpgradeCode GUID
    - Configure Package element with platform="x86"
    - Define directory structure (ProgramFilesFolder → ManufacturerFolder → INSTALLFOLDER)
    - Define ProgramMenuFolder for Start Menu shortcuts
    - Set application icon reference
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 5.1, 5.2, 5.3, 8.2_

  - [x] 2.2 Implement upgrade detection and removal logic
    - Add Upgrade element with stable UpgradeCode
    - Configure UpgradeVersion to detect versions 1.0.0 to 3.52.0
    - Add RemoveExistingProducts to InstallExecuteSequence after InstallInitialize
    - _Requirements: 9.1, 9.2, 9.3, 9.5_

  - [x] 2.3 Add .NET Framework 4.8 prerequisite check
    - Add PropertyRef for NETFRAMEWORK48
    - Add Condition with error message for missing .NET Framework
    - _Requirements: 14.1, 14.2, 14.3, 14.4_

  - [x] 2.4 Configure UI and feature references
    - Add UIRef for WixUI_InstallDir
    - Set WIXUI_INSTALLDIR property to INSTALLFOLDER
    - Define MainFeature with title and description
    - Reference all ComponentGroup elements from Files.wxs
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_

- [x] 3. Checkpoint - Verify Product.wxs compiles
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement Files.wxs with component definitions
  - [x] 4.1 Create application files component group
    - Create Files.wxs file
    - Define ComponentGroup for ApplicationFiles
    - Add Component for SMaRT.exe using $(var.SMaRT.TargetPath)
    - Add Component for SMaRT.exe.config
    - _Requirements: 1.1, 1.2, 12.2, 12.3_

  - [x] 4.2 Create project dependencies component groups
    - Add ComponentGroup for Custom_Grid.dll
    - Add ComponentGroup for SobekCM libraries (5 DLLs)
    - Add ComponentGroup for EngineAgnosticLayerDbAccess.dll
    - Use project reference variables for file sources
    - _Requirements: 1.3, 1.4, 1.5_

  - [x] 4.3 Create third-party dependencies component groups
    - Add ComponentGroup for core third-party DLLs (GemBox, iTextSharp, ApplicationBlocks)
    - Add ComponentGroup for JIL libraries
    - Add ComponentGroup for Protobuf libraries
    - Add ComponentGroup for Recaptcha library
    - Add ComponentGroup for Saxon libraries (IKVM and saxon9he DLLs)
    - Add ComponentGroup for SolrNet libraries
    - Add ComponentGroup for Zoom.net libraries (managed and native DLLs)
    - Add ComponentGroup for AppFabric caching libraries
    - Use relative paths from $(var.ProjectDir) for all third-party DLLs
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10_

  - [x] 4.4 Create data files component group
    - Define Directory element for DataFolder
    - Add ComponentGroup for data files
    - Add Components for searchfields.xml, sobekcm.config, stopwords.xml
    - Add Component for Zoom.Net.Factory.config
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 12.1_

  - [x] 4.5 Create image resources component group
    - Define Directory element for ImagesFolder
    - Add ComponentGroup for all image files
    - Include SMaRT.ico and all other image resources
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 4.6 Create Start Menu shortcuts component
    - Add Component for application shortcut to SMaRT.exe
    - Add Component for uninstall shortcut
    - Set shortcut icons, names, and descriptions
    - Add RemoveFolder element for cleanup
    - Add registry key path for component
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 5. Checkpoint - Verify Files.wxs compiles and all files referenced
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Add WiX project to solution and configure build dependencies
  - Add WixInstaller.wixproj to SMaRT.sln
  - Configure project dependencies on SMaRT and all library projects
  - Set build order to ensure dependencies build before installer
  - _Requirements: 11.5_

- [x] 7. Build and validate the installer
  - [x] 7.1 Build the WiX project in Release configuration
    - Build solution to generate all dependencies
    - Build WixInstaller project to create MSI
    - Verify UFDC_SMART_3.52.0.msi is created
    - _Requirements: 11.5_

  - [x] 7.2 Validate installer structure and metadata
    - Verify MSI file size is reasonable
    - Check that product name, version, and manufacturer are correct
    - Verify all required files are packaged in MSI
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

- [x] 8. Final checkpoint - Installer ready for testing
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- WiX Toolset v3.x must be installed on the build machine
- All project dependencies must build successfully before the installer
- The UpgradeCode GUID must remain constant across all future versions
- ProductCode is auto-generated (Id="*") for each build
- Component GUIDs are auto-generated (Guid="*") for simplicity
- The installer targets 64-bit Windows but uses x86 package platform for .NET Framework compatibility
- Installation requires administrator privileges (UAC prompt)
- Users can test installation with: msiexec /i UFDC_SMART_3.52.0.msi /l*v install.log
