# Requirements Document

## Introduction

This document specifies the requirements for creating an Inno Setup installer for the SMaRT (SobekCM Management Tool) application as an alternative to the existing WiX installer. SMaRT is a Windows Forms application targeting .NET Framework 4.8 that manages digital library content. The Inno Setup installer provides a simpler script-based approach with easier customization, smaller installer size, and similar functionality to the WiX installer.

## Glossary

- **SMaRT**: SobekCM Management Tool - the main Windows Forms application
- **Installer**: The Inno Setup-based executable installer package
- **Target_System**: The Windows computer where SMaRT will be installed
- **Program_Files**: The standard Windows Program Files directory
- **Start_Menu**: The Windows Start Menu
- **Application_Files**: All executables, DLLs, data files, and resources required by SMaRT
- **Dependencies**: Referenced project outputs and third-party libraries required by SMaRT
- **Upgrade**: Installation over an existing version of SMaRT
- **Inno_Script**: The .iss script file that defines the installer configuration

## Requirements

### Requirement 1: Package Main Application

**User Story:** As a user, I want the installer to include the SMaRT executable and its direct dependencies, so that the application can run after installation.

#### Acceptance Criteria

1. THE Installer SHALL include SMaRT.exe from the SMaRT project build output
2. THE Installer SHALL include SMaRT.exe.config from the SMaRT project build output
3. THE Installer SHALL include Custom_Grid.dll from the Custom_Grid project build output
4. THE Installer SHALL include all SobekCM library DLLs (SobekCM_Core.dll, SobekCM_Engine_Library.dll, SobekCM_Resource_Database.dll, SobekCM_Resource_Object.dll, SobekCM_Tools.dll) from their respective project build outputs
5. THE Installer SHALL include EngineAgnosticLayerDbAccess.dll from the EngineAgnosticLayerDbAccess project build output

### Requirement 2: Package Third-Party Dependencies

**User Story:** As a user, I want the installer to include all third-party libraries, so that SMaRT has all required dependencies to function.

#### Acceptance Criteria

1. THE Installer SHALL include GemBox.Spreadsheet.dll from the DLLs folder
2. THE Installer SHALL include itextsharp.dll from the DLLs folder
3. THE Installer SHALL include Microsoft.ApplicationBlocks.Data.dll from the DLLs folder
4. THE Installer SHALL include all JIL library files (Jil.dll, Sigil.dll) from the DLLs/JIL folder
5. THE Installer SHALL include protobuf-net.dll from the DLLs/Protobuf-net folder
6. THE Installer SHALL include Recaptcha.dll from the DLLs/ReCaptcha folder
7. THE Installer SHALL include all Saxon library files (IKVM.*.dll, saxon9he.dll, saxon9he-api.dll) from the DLLs/Saxon folder
8. THE Installer SHALL include all SolrNet library files (SolrNet.dll, Microsoft.Practices.ServiceLocation.dll) from the DLLs/SolrNet folder
9. THE Installer SHALL include all Zoom.net library files (Zoom.Net.dll, Zoom.Net.YazSharp.dll, yaz.dll, libxml2.dll, libxslt.dll, iconv.dll, zlib1.dll) from the DLLs/Zoom.net folder
10. THE Installer SHALL include all AppFabric library files (Microsoft.ApplicationServer.Caching.Client.dll, Microsoft.ApplicationServer.Caching.Core.dll) from the DLLs/AppFabric folder

### Requirement 3: Package Data Files

**User Story:** As a user, I want the installer to include required data files, so that SMaRT can access configuration and reference data.

#### Acceptance Criteria

1. THE Installer SHALL include searchfields.xml from the Data folder
2. THE Installer SHALL include sobekcm.config from the Data folder
3. THE Installer SHALL include stopwords.xml from the Data folder
4. THE Installer SHALL preserve the Data folder structure in the installation directory

### Requirement 4: Package Image Resources

**User Story:** As a user, I want the installer to include all image resources, so that SMaRT displays correctly with all icons and graphics.

#### Acceptance Criteria

1. THE Installer SHALL include all files from the Images folder
2. THE Installer SHALL include SMaRT.ico as the application icon
3. THE Installer SHALL preserve the Images folder structure in the installation directory

### Requirement 5: Install to Program Files

**User Story:** As a user, I want SMaRT installed to the standard Program Files location, so that it follows Windows conventions.

#### Acceptance Criteria

1. THE Installer SHALL install Application_Files to the Program Files directory by default
2. THE Installer SHALL create a subdirectory path "University of Florida\SMaRT" within Program Files
3. THE Installer SHALL allow the user to choose a custom installation directory during installation
4. WHEN the Target_System is 64-bit Windows, THE Installer SHALL default to Program Files (x86) for .NET Framework 4.8 compatibility

### Requirement 6: Create Start Menu Shortcuts

**User Story:** As a user, I want shortcuts created in the Start Menu, so that I can easily launch SMaRT.

#### Acceptance Criteria

1. THE Installer SHALL create a Start_Menu folder named "UFDC SMaRT"
2. THE Installer SHALL create a shortcut to SMaRT.exe in the Start_Menu folder
3. THE Installer SHALL use SMaRT.ico as the shortcut icon
4. THE Installer SHALL set the shortcut name to "UFDC SMaRT"
5. THE Installer SHALL create an uninstall shortcut in the Start_Menu folder

### Requirement 7: Support Installation and Uninstallation

**User Story:** As a user, I want to cleanly install and uninstall SMaRT, so that I can manage the application on my system.

#### Acceptance Criteria

1. WHEN installation completes successfully, THE Installer SHALL register SMaRT in Windows Programs and Features
2. THE Installer SHALL display "UFDC SMaRT" as the program name in Programs and Features
3. THE Installer SHALL display version 3.52.0 in Programs and Features
4. WHEN the user uninstalls SMaRT, THE Installer SHALL remove all Application_Files from the installation directory
5. WHEN the user uninstalls SMaRT, THE Installer SHALL remove all Start_Menu shortcuts
6. WHEN the user uninstalls SMaRT, THE Installer SHALL remove the installation directory if it is empty after file removal
7. THE Installer SHALL create an uninstall executable in the installation directory

### Requirement 8: Support x64 Windows Systems

**User Story:** As a user, I want the installer to work on 64-bit Windows systems, so that I can install SMaRT on modern Windows computers.

#### Acceptance Criteria

1. THE Installer SHALL support installation on 64-bit Windows systems (Windows 10 and later)
2. THE Installer SHALL use ArchitecturesInstallIn64BitMode to specify x64 compatibility
3. WHEN the Target_System is 64-bit, THE Installer SHALL install to Program Files (x86) by default
4. THE Installer SHALL detect the system architecture and install to the appropriate Program Files directory

### Requirement 9: Handle Version Upgrades

**User Story:** As a user, I want to upgrade from an older version of SMaRT, so that I can get new features without manual uninstallation.

#### Acceptance Criteria

1. WHEN an older version of SMaRT is detected, THE Installer SHALL prompt the user to uninstall the old version or automatically remove it
2. THE Installer SHALL use the same AppId across all versions to enable upgrade detection
3. THE Installer SHALL generate a unique AppId for the SMaRT application
4. WHEN performing an upgrade, THE Installer SHALL preserve user data files if they exist in the installation directory
5. THE Installer SHALL use the Inno Setup uninstallable flag to enable proper upgrade handling

### Requirement 10: Display Installation UI

**User Story:** As a user, I want a clear installation wizard, so that I can understand the installation progress and options.

#### Acceptance Criteria

1. THE Installer SHALL display a welcome page at the start of installation
2. THE Installer SHALL display a license agreement page requiring user acceptance
3. THE Installer SHALL display an installation directory selection page
4. THE Installer SHALL display a Start Menu folder selection page
5. THE Installer SHALL display a ready to install confirmation page
6. THE Installer SHALL display a progress page showing installation status
7. THE Installer SHALL display a completion page indicating successful installation

### Requirement 11: Set Application Metadata

**User Story:** As a system administrator, I want proper application metadata in the installer, so that I can identify and manage the installation.

#### Acceptance Criteria

1. THE Installer SHALL set the application name to "UFDC SMaRT"
2. THE Installer SHALL set the application version to "3.52.0"
3. THE Installer SHALL set the publisher to "University of Florida Digital Collections"
4. THE Installer SHALL set the application URL to a valid support or information website
5. THE Installer SHALL set the installer output filename to "UFDC_SMART_Setup_3.52.0.exe"
6. THE Installer SHALL include copyright information for University of Florida

### Requirement 12: Include Configuration Files

**User Story:** As a user, I want configuration files included, so that SMaRT can be configured for my environment.

#### Acceptance Criteria

1. THE Installer SHALL include Zoom.Net.Factory.config from the DLLs/Zoom.net folder
2. WHEN SMaRT.exe.config exists in the build output, THE Installer SHALL include it in the installation directory
3. THE Installer SHALL place configuration files in the same directory as SMaRT.exe

### Requirement 13: Set File Permissions

**User Story:** As a user, I want appropriate file permissions set, so that SMaRT can read its files and write logs if needed.

#### Acceptance Criteria

1. THE Installer SHALL request administrator privileges for installation
2. THE Installer SHALL set read and execute permissions for all Application_Files
3. THE Installer SHALL allow the installation directory to be writable by the installing user
4. WHEN SMaRT requires write access to its installation directory, THE Installer SHALL set appropriate permissions for the Users group using Inno Setup permissions directives

### Requirement 14: Validate Installation Prerequisites

**User Story:** As a user, I want to be notified of missing prerequisites, so that I can install them before installing SMaRT.

#### Acceptance Criteria

1. WHEN .NET Framework 4.8 is not installed, THE Installer SHALL display an error message indicating the requirement
2. THE Installer SHALL check for .NET Framework 4.8 before proceeding with installation using Inno Setup's Check parameter
3. THE Installer SHALL provide a message with instructions for downloading .NET Framework 4.8
4. WHEN prerequisites are not met, THE Installer SHALL prevent installation from proceeding

### Requirement 15: Optimize Installer Size

**User Story:** As a user, I want a smaller installer download, so that I can download and distribute the installer more easily.

#### Acceptance Criteria

1. THE Installer SHALL use solid compression to minimize installer size
2. THE Installer SHALL use lzma2 compression algorithm for maximum compression ratio
3. THE Installer SHALL compress all Application_Files into the installer executable
4. THE Installer SHALL produce an installer smaller than 50 MB when possible

### Requirement 16: Support Custom Installation Pages

**User Story:** As a developer, I want the ability to add custom installation pages, so that I can collect configuration information during installation.

#### Acceptance Criteria

1. THE Inno_Script SHALL support adding custom wizard pages using Pascal scripting
2. THE Inno_Script SHALL provide a framework for custom page creation
3. THE Inno_Script SHALL allow custom pages to be inserted at any point in the installation wizard
4. THE Inno_Script SHALL support validation of custom page inputs before proceeding

### Requirement 17: Create Desktop Shortcut Option

**User Story:** As a user, I want the option to create a desktop shortcut, so that I can quickly access SMaRT from my desktop.

#### Acceptance Criteria

1. THE Installer SHALL offer a checkbox option to create a desktop shortcut during installation
2. WHEN the user selects the desktop shortcut option, THE Installer SHALL create a shortcut to SMaRT.exe on the desktop
3. THE Installer SHALL use SMaRT.ico as the desktop shortcut icon
4. THE Installer SHALL set the desktop shortcut name to "UFDC SMaRT"
5. THE Installer SHALL make the desktop shortcut option selected by default

### Requirement 18: Generate Inno Setup Script

**User Story:** As a developer, I want a well-structured Inno Setup script, so that I can easily maintain and customize the installer.

#### Acceptance Criteria

1. THE Inno_Script SHALL be saved with a .iss file extension
2. THE Inno_Script SHALL include clear section headers for [Setup], [Files], [Icons], [Run], and [Code]
3. THE Inno_Script SHALL include comments explaining key configuration options
4. THE Inno_Script SHALL use relative paths for source files to support building from different locations
5. THE Inno_Script SHALL be compatible with Inno Setup 6.0 or later

### Requirement 19: Support Silent Installation

**User Story:** As a system administrator, I want to perform silent installations, so that I can deploy SMaRT to multiple computers without user interaction.

#### Acceptance Criteria

1. THE Installer SHALL support the /SILENT command-line parameter for silent installation
2. THE Installer SHALL support the /VERYSILENT command-line parameter for completely silent installation
3. WHEN running in silent mode, THE Installer SHALL use default installation options
4. WHEN running in silent mode, THE Installer SHALL install to the default directory without prompting
5. THE Installer SHALL support the /DIR command-line parameter to specify installation directory in silent mode

### Requirement 20: Include License Agreement

**User Story:** As a user, I want to see the license agreement during installation, so that I understand the terms of use.

#### Acceptance Criteria

1. THE Installer SHALL display a license agreement page during installation
2. THE Installer SHALL require the user to accept the license agreement before proceeding
3. THE Installer SHALL include a license.txt file with the appropriate license text
4. THE Installer SHALL prevent installation if the user declines the license agreement
5. THE Inno_Script SHALL reference the license file using the LicenseFile directive

