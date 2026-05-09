; Inno Setup Script for UFDC SMaRT (SobekCM Management Tool)
; This script creates a Windows installer for the SMaRT application
; including all dependencies, data files, and resources.

[Setup]
; Unique application identifier - MUST remain constant across all versions for upgrade detection
AppId={{A7F3B8C2-4D5E-4A1B-9C3D-8E7F6A5B4C3D}}

; Application information displayed in installer and Programs & Features
AppName=UFDC SMaRT Per-User
AppVersion=3.52.8
AppPublisher=University of Florida Digital Collections
AppPublisherURL=http://ufdc.ufl.edu
AppSupportURL=http://ufdc.ufl.edu
AppUpdatesURL=http://ufdc.ufl.edu

; Default installation directory - uses {localappdata} for per-user installation without admin rights
; Installs to C:\Users\[username]\AppData\Local\University of Florida\SMaRT
DefaultDirName={localappdata}\University of Florida\SMaRT
DefaultGroupName=UFDC SMaRT Per-User

; Allow user to choose custom installation directory
DisableDirPage=no
DisableProgramGroupPage=no

; Output configuration
OutputDir=Output
OutputBaseFilename=UFDC_SMART_Setup_3.52.8

; Compression settings - LZMA2 with maximum compression for smallest installer size
; SolidCompression compresses all files together for better compression ratio
Compression=lzma2/max
SolidCompression=yes

; Architecture configuration - support 64-bit Windows systems
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

; Privileges and security - 'lowest' allows installation without admin rights
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; License agreement
LicenseFile=license.txt

; Uninstall configuration
UninstallDisplayName=UFDC SMaRT Per-User
UninstallDisplayIcon={app}\SMaRT.exe

; Version information displayed in Programs & Features
VersionInfoVersion=3.52.8
VersionInfoCompany=University of Florida Digital Collections
VersionInfoDescription=SobekCM Management Tool
VersionInfoCopyright=Copyright (c) University of Florida

; Wizard appearance
WizardStyle=modern
DisableWelcomePage=no

; Minimum Windows version - Windows 10 and later
MinVersion=10.0

[Files]
; ============================================================================
; MAIN APPLICATION FILES
; ============================================================================
; Main executable and configuration
Source: "..\SMaRT\bin\Release\SMaRT.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SMaRT\bin\Release\SMaRT.exe.config"; DestDir: "{app}"; Flags: ignoreversion

; ============================================================================
; PROJECT DEPENDENCY DLLS
; ============================================================================
; Custom Grid component
Source: "..\Custom_Grid\bin\Release\Custom_Grid_1_3.dll"; DestDir: "{app}"; Flags: ignoreversion

; SobekCM library components
Source: "..\SobekCM_Core\bin\Release\SobekCM_Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SobekCM_Engine_Library\bin\Release\SobekCM_Engine_Library.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SobekCM_Resource_Database\bin\Release\SobekCM_Resource_Database.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SobekCM_Resource_Object\bin\Release\SobekCM_Resource_Object.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SobekCM_Tools\bin\Release\SobekCM_Tools.dll"; DestDir: "{app}"; Flags: ignoreversion

; Engine Agnostic Layer Database Access
Source: "..\EngineAgnosticLayerDbAccess\bin\Release\EngineAgnosticLayerDbAccess.dll"; DestDir: "{app}"; Flags: ignoreversion

; ============================================================================
; THIRD-PARTY LIBRARY DLLS
; ============================================================================
; GemBox Spreadsheet library
Source: "..\DLLs\GemBox.Spreadsheet.dll"; DestDir: "{app}"; Flags: ignoreversion

; iTextSharp PDF library
Source: "..\DLLs\itextsharp.dll"; DestDir: "{app}"; Flags: ignoreversion

; Microsoft Application Blocks
Source: "..\DLLs\Microsoft.ApplicationBlocks.Data.dll"; DestDir: "{app}"; Flags: ignoreversion

; JIL JSON serialization library
Source: "..\DLLs\JIL\Jil.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\JIL\Sigil.dll"; DestDir: "{app}"; Flags: ignoreversion

; Protocol Buffers library
Source: "..\DLLs\Protobuf-net\protobuf-net.dll"; DestDir: "{app}"; Flags: ignoreversion

; ReCaptcha library
Source: "..\DLLs\ReCaptcha\Recaptcha.dll"; DestDir: "{app}"; Flags: ignoreversion

; Saxon XSLT processor and IKVM dependencies
Source: "..\DLLs\Saxon\IKVM.OpenJDK.Charsets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\IKVM.OpenJDK.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\IKVM.OpenJDK.Text.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\IKVM.OpenJDK.Util.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\IKVM.OpenJDK.XML.API.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\IKVM.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\saxon9he.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Saxon\saxon9he-api.dll"; DestDir: "{app}"; Flags: ignoreversion

; SolrNet search library
Source: "..\DLLs\SolrNet\SolrNet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\SolrNet\Microsoft.Practices.ServiceLocation.dll"; DestDir: "{app}"; Flags: ignoreversion

; Zoom.Net Z39.50 library and native dependencies
Source: "..\DLLs\Zoom.net\Zoom.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Zoom.net\Zoom.Net.YazSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Zoom.net\yaz.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Zoom.net\libxml2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Zoom.net\libxslt.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Zoom.net\iconv.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\Zoom.net\zlib1.dll"; DestDir: "{app}"; Flags: ignoreversion

; AppFabric caching libraries
Source: "..\DLLs\AppFabric\Microsoft.ApplicationServer.Caching.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DLLs\AppFabric\Microsoft.ApplicationServer.Caching.Core.dll"; DestDir: "{app}"; Flags: ignoreversion

; ============================================================================
; CONFIGURATION FILES
; ============================================================================
; Zoom.Net factory configuration
Source: "..\DLLs\Zoom.net\Zoom.Net.Factory.config"; DestDir: "{app}"; Flags: ignoreversion

; Default SobekCM configuration from Debug config folder
Source: "..\SMaRT\bin\Debug\config\sobekcm.config"; DestDir: "{app}\config"; Flags: ignoreversion

; ============================================================================
; DATA FILES
; ============================================================================
; Application data files - preserve folder structure with recursesubdirs
; Note: sobekcm.config is handled separately from Debug config folder
Source: "..\Data\searchfields.xml"; DestDir: "{app}\Data"; Flags: ignoreversion
Source: "..\Data\stopwords.xml"; DestDir: "{app}\Data"; Flags: ignoreversion

; ============================================================================
; IMAGE RESOURCES
; ============================================================================
; Application images and icons - preserve folder structure with recursesubdirs
Source: "..\Images\*"; DestDir: "{app}\Images"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; ============================================================================
; SHORTCUTS (per-user locations for non-admin install)
; ============================================================================
; Start Menu shortcuts - {userprograms} is the user's Start Menu Programs folder
Name: "{userprograms}\UFDC SMaRT Per-User\UFDC SMaRT Per-User"; Filename: "{app}\SMaRT.exe"; IconFilename: "{app}\Images\SMaRT.ico"; Comment: "Launch UFDC SMaRT Per-User"
Name: "{userprograms}\UFDC SMaRT Per-User\Uninstall UFDC SMaRT Per-User"; Filename: "{uninstallexe}"; Comment: "Uninstall UFDC SMaRT Per-User"

; Desktop shortcut (optional, controlled by desktopicon task)
; {userdesktop} is the current user's desktop folder
Name: "{userdesktop}\UFDC SMaRT Per-User"; Filename: "{app}\SMaRT.exe"; IconFilename: "{app}\Images\SMaRT.ico"; Comment: "Launch UFDC SMaRT Per-User"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
; ============================================================================
; POST-INSTALLATION ACTIONS
; ============================================================================
; Optional launch of SMaRT after installation completes
Filename: "{app}\SMaRT.exe"; Description: "Launch UFDC SMaRT Per-User"; Flags: nowait postinstall skipifsilent unchecked

[Code]
// Check if .NET Framework 4.8 is installed
function IsDotNet48Installed: Boolean;
var
  Success: Boolean;
  InstallValue: Cardinal;
begin
  // Check for .NET Framework 4.8 in registry
  // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full
  // Release value >= 528040 indicates .NET 4.8 or later
  Success := RegQueryDWordValue(HKEY_LOCAL_MACHINE,
    'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full',
    'Release', InstallValue);
  
  Result := Success and (InstallValue >= 528040);
end;

// Initialize setup - check prerequisites before installation begins
function InitializeSetup: Boolean;
begin
  // Check for .NET Framework 4.8
  if not IsDotNet48Installed then
  begin
    MsgBox('.NET Framework 4.8 or later is required to run UFDC SMaRT.' + #13#10 + #13#10 +
           'Please download and install .NET Framework 4.8 from:' + #13#10 +
           'https://dotnet.microsoft.com/download/dotnet-framework/net48' + #13#10 + #13#10 +
           'After installing .NET Framework 4.8, run this installer again.',
           mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;
