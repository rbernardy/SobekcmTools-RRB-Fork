# Design Document: WiX Installer for SMaRT Application

## Overview

This design specifies a WiX Toolset v3.x installer for the SMaRT (SobekCM Management Tool) application. The installer packages a .NET Framework 4.8 Windows Forms application with its dependencies, data files, and resources into a professional MSI installer that supports installation, upgrade, and uninstallation on 64-bit Windows systems.

The installer follows Windows installation best practices by installing to Program Files, creating Start Menu shortcuts, registering with Programs and Features, and implementing proper upgrade logic using Windows Installer technology.

### Key Design Goals

- Package all application files, dependencies, data files, and resources
- Provide a professional installation wizard experience
- Support seamless upgrades from previous versions
- Follow Windows installation conventions and best practices
- Validate prerequisites before installation
- Enable clean uninstallation

## Architecture

### WiX Project Structure

The installer will be implemented as a single WiX project with the following structure:

```
WixInstaller/
├── Product.wxs              # Main product definition and component structure
├── Files.wxs                # File and directory component definitions
├── UI.wxs                   # Custom UI dialog sequence (if needed)
├── WixInstaller.wixproj     # MSBuild project file
└── README.md                # Build and maintenance instructions
```

### Component Architecture

The installer uses a feature-based component model:

```
Product (UFDC SMaRT 3.52.0)
└── MainFeature (Complete Installation)
    ├── ApplicationComponent (SMaRT.exe + config)
    ├── CustomGridComponent (Custom_Grid.dll)
    ├── SobekCMLibrariesComponent (SobekCM_*.dll)
    ├── DatabaseAccessComponent (EngineAgnosticLayerDbAccess.dll)
    ├── ThirdPartyDependenciesComponent (GemBox, iTextSharp, etc.)
    ├── JILComponent (Jil.dll, Sigil.dll)
    ├── ProtobufComponent (protobuf-net.dll)
    ├── RecaptchaComponent (Recaptcha.dll)
    ├── SaxonComponent (IKVM.*.dll, saxon9he*.dll)
    ├── SolrNetComponent (SolrNet.dll, ServiceLocation.dll)
    ├── ZoomNetComponent (Zoom.Net.dll + native DLLs)
    ├── AppFabricComponent (Caching DLLs)
    ├── DataFilesComponent (searchfields.xml, sobekcm.config, stopwords.xml)
    ├── ImageResourcesComponent (all Images folder files)
    └── ShortcutsComponent (Start Menu shortcuts)
```

### Build Integration

The WiX project will integrate with the existing Visual Studio solution build process:

1. SMaRT and dependency projects build to their output directories
2. WiX project references these output directories using MSBuild properties
3. Heat.exe (WiX harvesting tool) can optionally auto-generate file lists
4. Candle.exe compiles .wxs files to .wixobj files
5. Light.exe links .wixobj files to produce the final .msi

## Components and Interfaces

### Product Definition Component

**File:** Product.wxs

**Responsibilities:**
- Define product metadata (name, version, manufacturer)
- Define unique ProductCode and stable UpgradeCode
- Configure upgrade logic
- Define installation directory structure
- Reference all feature and component definitions

**Key Attributes:**
```xml
<Product 
  Id="*"  <!-- Auto-generate new GUID for each version -->
  Name="UFDC SMaRT"
  Version="3.52.0"
  Manufacturer="University of Florida Digital Collections"
  UpgradeCode="PUT-STABLE-GUID-HERE"  <!-- Same across all versions -->
  Language="1033">
```

### Directory Structure Component

**Responsibilities:**
- Map logical directories to Windows standard locations
- Define installation folder hierarchy

**Directory Mapping:**
```
ProgramFilesFolder (C:\Program Files (x86)\)
└── ManufacturerFolder (University of Florida\)
    └── INSTALLFOLDER (SMaRT\)
        ├── [Application files]
        ├── Data\
        │   ├── searchfields.xml
        │   ├── sobekcm.config
        │   └── stopwords.xml
        └── Images\
            └── [All image files]

ProgramMenuFolder (Start Menu)
└── ApplicationProgramsFolder (UFDC SMaRT\)
    ├── UFDC SMaRT.lnk
    └── Uninstall UFDC SMaRT.lnk
```

### File Components

**File:** Files.wxs

**Responsibilities:**
- Define all file components to be installed
- Group files by logical component
- Set file attributes and permissions

**Component Design Principles:**
- One component per file for maximum flexibility
- Component GUIDs auto-generated or stable based on file path
- KeyPath set to the file itself for proper reference counting
- Group related files (e.g., all Saxon DLLs) under a single ComponentGroup

**Example Component Structure:**
```xml
<ComponentGroup Id="ApplicationFiles">
  <Component Id="SMaRT.exe" Directory="INSTALLFOLDER">
    <File Id="SMaRT.exe" Source="$(var.SMaRT.TargetPath)" KeyPath="yes" />
  </Component>
  <Component Id="SMaRT.exe.config" Directory="INSTALLFOLDER">
    <File Id="SMaRT.exe.config" Source="$(var.SMaRT.TargetDir)SMaRT.exe.config" KeyPath="yes" />
  </Component>
</ComponentGroup>

<ComponentGroup Id="DataFiles">
  <Component Id="searchfields.xml" Directory="DataFolder">
    <File Id="searchfields.xml" Source="$(var.ProjectDir)..\Data\searchfields.xml" KeyPath="yes" />
  </Component>
  <!-- Additional data files -->
</ComponentGroup>
```

### Dependency Resolution

**Project References:**
The WiX project will use MSBuild project references to automatically resolve build output paths:

```xml
<ItemGroup>
  <ProjectReference Include="..\SMaRT\SMaRT.csproj">
    <Name>SMaRT</Name>
    <Project>{C5385BAF-4740-45E5-8F26-59B58118F4B2}</Project>
  </ProjectReference>
  <ProjectReference Include="..\Custom_Grid\Custom_Grid.csproj">
    <Name>Custom_Grid</Name>
  </ProjectReference>
  <!-- Additional project references -->
</ItemGroup>
```

**Third-Party Dependencies:**
Third-party DLLs will be referenced using relative paths from the project directory:

```xml
<File Source="$(var.ProjectDir)..\DLLs\GemBox.Spreadsheet.dll" />
<File Source="$(var.ProjectDir)..\DLLs\itextsharp.dll" />
```

### Shortcuts Component

**Responsibilities:**
- Create Start Menu folder
- Create application shortcut
- Create uninstall shortcut
- Set shortcut icons and properties

**Implementation:**
```xml
<Component Id="ApplicationShortcut" Directory="ApplicationProgramsFolder">
  <Shortcut Id="ApplicationStartMenuShortcut"
            Name="UFDC SMaRT"
            Description="SobekCM Management Tool"
            Target="[INSTALLFOLDER]SMaRT.exe"
            WorkingDirectory="INSTALLFOLDER"
            Icon="SMaRT.ico" />
  <Shortcut Id="UninstallProduct"
            Name="Uninstall UFDC SMaRT"
            Target="[SystemFolder]msiexec.exe"
            Arguments="/x [ProductCode]"
            Description="Uninstalls UFDC SMaRT" />
  <RemoveFolder Id="ApplicationProgramsFolder" On="uninstall"/>
  <RegistryValue Root="HKCU" Key="Software\UF\SMaRT" Name="installed" Type="integer" Value="1" KeyPath="yes"/>
</Component>
```

### UI Component

**Responsibilities:**
- Configure installation wizard dialog sequence
- Customize dialogs with product branding
- Handle user input for installation directory

**Implementation:**
Uses WixUI_InstallDir dialog set with customization:

```xml
<UIRef Id="WixUI_InstallDir" />
<Property Id="WIXUI_INSTALLDIR" Value="INSTALLFOLDER" />

<!-- Optional: Custom license file -->
<WixVariable Id="WixUILicenseRtf" Value="License.rtf" />

<!-- Optional: Custom banner and dialog graphics -->
<WixVariable Id="WixUIBannerBmp" Value="Banner.bmp" />
<WixVariable Id="WixUIDialogBmp" Value="Dialog.bmp" />
```

### Upgrade Component

**Responsibilities:**
- Detect previous installations
- Remove old version before installing new version
- Preserve user data during upgrades

**Implementation:**
```xml
<Upgrade Id="PUT-STABLE-GUID-HERE">
  <UpgradeVersion OnlyDetect="no"
                  Property="PREVIOUSFOUND"
                  Minimum="1.0.0" IncludeMinimum="yes"
                  Maximum="3.52.0" IncludeMaximum="no" />
</Upgrade>

<InstallExecuteSequence>
  <RemoveExistingProducts After="InstallInitialize" />
</InstallExecuteSequence>
```

This configuration uses the "early" removal strategy (RemoveExistingProducts after InstallInitialize), which uninstalls the old version before installing the new version. This is safer for major upgrades but requires more disk space during installation.

### Prerequisites Component

**Responsibilities:**
- Check for .NET Framework 4.8
- Display error message if prerequisites missing
- Provide guidance for installing prerequisites

**Implementation:**
```xml
<PropertyRef Id="NETFRAMEWORK48" />

<Condition Message="This application requires .NET Framework 4.8. Please install the .NET Framework 4.8 and run this installer again.">
  <![CDATA[Installed OR NETFRAMEWORK48]]>
</Condition>
```

The NETFRAMEWORK48 property is provided by WiX's built-in .NET Framework detection. The condition allows installation to proceed if either the product is already installed (upgrade scenario) or .NET Framework 4.8 is present.

## Data Models

### Product Identification

```
Product
├── ProductCode: GUID (unique per version, auto-generated)
├── UpgradeCode: GUID (stable across all versions)
├── Version: "3.52.0"
├── Name: "UFDC SMaRT"
└── Manufacturer: "University of Florida Digital Collections"
```

### Component Model

```
Component
├── Id: string (unique identifier)
├── Guid: GUID (stable or auto-generated)
├── Directory: DirectoryRef
├── KeyPath: File or RegistryValue
└── Files: File[]
```

### Feature Model

```
Feature
├── Id: "MainFeature"
├── Title: "UFDC SMaRT"
├── Level: 1 (always installed)
├── ConfigurableDirectory: "INSTALLFOLDER"
└── ComponentGroupRefs: ComponentGroup[]
```

### File Inventory

The installer packages the following file categories:

1. **Application Executables** (1 file)
   - SMaRT.exe

2. **Application Configuration** (1 file)
   - SMaRT.exe.config

3. **Project Dependencies** (6 DLLs)
   - Custom_Grid.dll
   - SobekCM_Core.dll
   - SobekCM_Engine_Library.dll
   - SobekCM_Resource_Database.dll
   - SobekCM_Resource_Object.dll
   - SobekCM_Tools.dll
   - EngineAgnosticLayerDbAccess.dll

4. **Third-Party Dependencies** (30+ DLLs)
   - Core: GemBox.Spreadsheet.dll, itextsharp.dll, Microsoft.ApplicationBlocks.Data.dll
   - JIL: Jil.dll, Sigil.dll
   - Protobuf: protobuf-net.dll
   - ReCaptcha: Recaptcha.dll
   - Saxon: IKVM.*.dll (5 files), saxon9he.dll, saxon9he-api.dll
   - SolrNet: SolrNet.dll, Microsoft.Practices.ServiceLocation.dll
   - Zoom.net: Zoom.Net.dll, Zoom.Net.YazSharp.dll, yaz.dll, libxml2.dll, libxslt.dll, iconv.dll, zlib1.dll
   - AppFabric: Microsoft.ApplicationServer.Caching.Client.dll, Microsoft.ApplicationServer.Caching.Core.dll

5. **Configuration Files** (1 file)
   - Zoom.Net.Factory.config

6. **Data Files** (3 files)
   - searchfields.xml
   - sobekcm.config
   - stopwords.xml

7. **Image Resources** (20+ files)
   - All files from Images folder including SMaRT.ico

## Error Handling

### Build-Time Error Handling

**Missing Files:**
- WiX will fail compilation if referenced source files don't exist
- Use MSBuild conditions to verify project references resolve correctly
- Provide clear error messages in build output

**Invalid Configuration:**
- WiX validates XML schema during compilation
- Component GUID conflicts detected by Light.exe
- ICE validation checks for Windows Installer best practices

### Install-Time Error Handling

**Prerequisites Not Met:**
- Display clear error message with .NET Framework 4.8 requirement
- Prevent installation from proceeding
- Provide download link or instructions

**Insufficient Permissions:**
- Windows Installer requires administrator privileges for Program Files installation
- UAC prompt will appear automatically
- Installation fails gracefully if user declines elevation

**Disk Space:**
- Windows Installer automatically checks available disk space
- Installation fails if insufficient space with appropriate error message

**File In Use:**
- If SMaRT.exe is running during upgrade, Windows Installer will prompt to close it
- FilesInUse dialog allows user to close application or retry
- Reboot may be required if files cannot be replaced

**Rollback:**
- Windows Installer automatically rolls back on installation failure
- All files and registry entries are removed
- System restored to pre-installation state

### Uninstall Error Handling

**Files In Use:**
- Same handling as installation - prompt to close application
- Reboot scheduled if necessary

**Partial Uninstall:**
- Windows Installer tracks all installed components
- Uninstall removes only files installed by this product
- Shared components (if any) are reference-counted

## Testing Strategy

Since this is an Infrastructure as Code (IaC) project for creating an MSI installer, property-based testing is not applicable. The testing strategy focuses on installation scenarios, upgrade paths, and validation checks.

### Build Validation

**Objective:** Ensure the WiX project compiles successfully and produces a valid MSI

**Approach:**
- Automated build in CI/CD pipeline
- WiX compiler (candle.exe) validation
- WiX linker (light.exe) validation
- ICE validation (Internal Consistency Evaluators)
- MSI file integrity checks

**Test Cases:**
1. Clean build from source produces .msi file
2. No WiX compiler warnings or errors
3. All ICE validation checks pass
4. MSI file size is reasonable (expected range)
5. MSI can be opened with Orca or similar tool

### Installation Testing

**Objective:** Verify clean installation on target systems

**Test Scenarios:**

1. **Fresh Installation on Clean System**
   - Install on Windows 11 64-bit with .NET Framework 4.8
   - Verify all files copied to Program Files\University of Florida\SMaRT\
   - Verify Data and Images subdirectories created
   - Verify Start Menu shortcuts created
   - Verify Programs and Features entry created
   - Launch SMaRT.exe from Start Menu
   - Verify application runs without errors

2. **Installation Without Prerequisites**
   - Install on system without .NET Framework 4.8
   - Verify error message displayed
   - Verify installation does not proceed
   - Verify no files or registry entries left behind

3. **Custom Installation Directory**
   - Select custom directory during installation
   - Verify files installed to custom location
   - Verify shortcuts point to correct location
   - Verify application runs from custom location

4. **Installation with Insufficient Permissions**
   - Attempt installation as non-administrator
   - Verify UAC prompt appears
   - Verify installation succeeds after elevation
   - Verify installation fails gracefully if elevation declined

### Upgrade Testing

**Objective:** Verify seamless upgrades from previous versions

**Test Scenarios:**

1. **Upgrade from Version 3.51.0 to 3.52.0**
   - Install version 3.51.0
   - Run installer for version 3.52.0
   - Verify old version detected and removed
   - Verify new version installed successfully
   - Verify no duplicate entries in Programs and Features
   - Verify Start Menu shortcuts updated
   - Verify application runs with new version

2. **Upgrade with Application Running**
   - Install version 3.51.0 and launch application
   - Run installer for version 3.52.0
   - Verify FilesInUse dialog appears
   - Close application and retry
   - Verify upgrade completes successfully

3. **Upgrade Rollback**
   - Install version 3.51.0
   - Simulate upgrade failure (e.g., disk full)
   - Verify rollback occurs
   - Verify version 3.51.0 still functional

### Uninstallation Testing

**Objective:** Verify clean removal of application

**Test Scenarios:**

1. **Clean Uninstall**
   - Install application
   - Uninstall via Programs and Features
   - Verify all files removed from installation directory
   - Verify installation directory removed (if empty)
   - Verify Start Menu shortcuts removed
   - Verify Programs and Features entry removed
   - Verify no orphaned registry entries

2. **Uninstall with Application Running**
   - Install and launch application
   - Attempt uninstall
   - Verify FilesInUse dialog appears
   - Close application and complete uninstall
   - Verify clean removal

### File Verification Testing

**Objective:** Ensure all required files are packaged

**Approach:**
- Extract MSI contents using msiexec /a or dark.exe
- Compare extracted files against requirements checklist
- Verify file versions and sizes

**Verification Checklist:**
- [ ] SMaRT.exe present
- [ ] SMaRT.exe.config present
- [ ] All 6 SobekCM DLLs present
- [ ] Custom_Grid.dll present
- [ ] EngineAgnosticLayerDbAccess.dll present
- [ ] All third-party DLLs present (30+ files)
- [ ] All Data files present (3 files)
- [ ] All Image files present (20+ files)
- [ ] Zoom.Net.Factory.config present
- [ ] SMaRT.ico present

### Compatibility Testing

**Objective:** Verify installer works on supported platforms

**Test Matrix:**
- Windows 11 64-bit (primary target)
- With and without .NET Framework 4.8 pre-installed
- Clean install vs. upgrade scenarios
- Default installation directory vs. custom directory

### Regression Testing

**Objective:** Ensure changes don't break existing functionality

**Approach:**
- Maintain test suite for each release
- Run full test suite before each release
- Document any known issues or limitations

### Manual Testing Checklist

For each release, manually verify:

1. [ ] Installer launches without errors
2. [ ] Welcome dialog displays correctly
3. [ ] License agreement displays correctly
4. [ ] Directory selection works
5. [ ] Installation progress displays
6. [ ] Completion dialog displays
7. [ ] Start Menu shortcuts created
8. [ ] Application launches from shortcut
9. [ ] Application functions correctly
10. [ ] Uninstall works cleanly
11. [ ] Upgrade from previous version works
12. [ ] No errors in Event Viewer

### Automated Testing Tools

**Recommended Tools:**
- **Orca:** MSI database editor for manual inspection
- **Dark.exe:** WiX decompiler for extracting MSI contents
- **MsiExec:** Command-line installation for automation
- **PowerShell:** Scripting installation test scenarios
- **Pester:** PowerShell testing framework for automated tests

**Example Automated Test (PowerShell):**
```powershell
Describe "SMaRT Installer Tests" {
    It "Installs successfully" {
        $result = Start-Process msiexec.exe -ArgumentList "/i UFDC_SMART_3.52.0.msi /qn" -Wait -PassThru
        $result.ExitCode | Should -Be 0
    }
    
    It "Creates installation directory" {
        Test-Path "C:\Program Files (x86)\University of Florida\SMaRT" | Should -Be $true
    }
    
    It "Creates Start Menu shortcut" {
        Test-Path "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\UFDC SMaRT\UFDC SMaRT.lnk" | Should -Be $true
    }
    
    It "Registers in Programs and Features" {
        $app = Get-WmiObject -Class Win32_Product | Where-Object { $_.Name -eq "UFDC SMaRT" }
        $app | Should -Not -BeNullOrEmpty
        $app.Version | Should -Be "3.52.0"
    }
}
```

### Test Documentation

**Test Plan Document:**
- Test objectives and scope
- Test environment requirements
- Test case descriptions
- Expected results
- Pass/fail criteria

**Test Results Document:**
- Test execution date
- Tester name
- Test environment details
- Test results (pass/fail)
- Issues found
- Screenshots of key steps

## Build Integration Approach

### MSBuild Integration

The WiX project integrates with the existing Visual Studio solution using MSBuild:

**WixInstaller.wixproj Structure:**
```xml
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">x86</Platform>
    <ProductVersion>3.x</ProductVersion>
    <ProjectGuid>{NEW-GUID-HERE}</ProjectGuid>
    <SchemaVersion>2.0</SchemaVersion>
    <OutputName>UFDC_SMART_3.52.0</OutputName>
    <OutputType>Package</OutputType>
    <WixTargetsPath Condition=" '$(WixTargetsPath)' == '' ">$(MSBuildExtensionsPath)\WiX Toolset\v3.x\Wix.targets</WixTargetsPath>
  </PropertyGroup>
  
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|x86' ">
    <OutputPath>bin\$(Configuration)\</OutputPath>
    <IntermediateOutputPath>obj\$(Configuration)\</IntermediateOutputPath>
    <DefineConstants>ProductVersion=3.52.0</DefineConstants>
  </PropertyGroup>
  
  <ItemGroup>
    <Compile Include="Product.wxs" />
    <Compile Include="Files.wxs" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\SMaRT\SMaRT.csproj">
      <Name>SMaRT</Name>
      <Project>{C5385BAF-4740-45E5-8F26-59B58118F4B2}</Project>
      <Private>True</Private>
      <DoNotHarvest>True</DoNotHarvest>
      <RefProjectOutputGroups>Binaries;Content;Satellites</RefProjectOutputGroups>
      <RefTargetDir>INSTALLFOLDER</RefTargetDir>
    </ProjectReference>
  </ItemGroup>
  
  <ItemGroup>
    <WixExtension Include="WixUIExtension">
      <HintPath>$(WixExtDir)\WixUIExtension.dll</HintPath>
      <Name>WixUIExtension</Name>
    </WixExtension>
    <WixExtension Include="WixNetFxExtension">
      <HintPath>$(WixExtDir)\WixNetFxExtension.dll</HintPath>
      <Name>WixNetFxExtension</Name>
    </WixExtension>
  </ItemGroup>
  
  <Import Project="$(WixTargetsPath)" />
</Project>
```

### Build Dependencies

**Build Order:**
1. Custom_Grid project builds → Custom_Grid.dll
2. SobekCM library projects build → SobekCM_*.dll
3. EngineAgnosticLayerDbAccess builds → EngineAgnosticLayerDbAccess.dll
4. SMaRT project builds → SMaRT.exe
5. WixInstaller project builds → UFDC_SMART_3.52.0.msi

**Dependency Configuration:**
The WiX project should have project dependencies on all referenced projects to ensure correct build order. This is configured in Visual Studio solution file or via MSBuild ProjectReference elements.

### Build Variables

**MSBuild Variables for File Paths:**
```xml
<!-- Automatically available from ProjectReference -->
$(var.SMaRT.TargetPath)          → Full path to SMaRT.exe
$(var.SMaRT.TargetDir)           → Directory containing SMaRT.exe
$(var.SMaRT.TargetFileName)      → SMaRT.exe

<!-- Custom variables for third-party DLLs -->
$(var.ProjectDir)                → WixInstaller project directory
$(var.SolutionDir)               → Solution root directory
```

**Usage in WXS Files:**
```xml
<File Source="$(var.SMaRT.TargetPath)" />
<File Source="$(var.SMaRT.TargetDir)SMaRT.exe.config" />
<File Source="$(var.ProjectDir)..\DLLs\GemBox.Spreadsheet.dll" />
```

### Preprocessor Directives

**Version Management:**
```xml
<?define ProductVersion="3.52.0" ?>
<?define ProductName="UFDC SMaRT" ?>
<?define Manufacturer="University of Florida Digital Collections" ?>

<Product Version="$(var.ProductVersion)" Name="$(var.ProductName)" Manufacturer="$(var.Manufacturer)">
```

This allows version numbers to be updated in one place.

### Heat.exe Integration (Optional)

Heat.exe can automatically harvest files from a directory, but for this project, manual file listing is recommended for better control. If automatic harvesting is desired:

```xml
<Target Name="BeforeBuild">
  <HeatDirectory Directory="..\SMaRT\bin\$(Configuration)" 
                 PreprocessorVariable="var.SMaRT.TargetDir"
                 DirectoryRefId="INSTALLFOLDER" 
                 ComponentGroupName="HarvestedFiles" 
                 SuppressCom="true" 
                 SuppressFragments="true" 
                 SuppressRegistry="true" 
                 SuppressRootDirectory="true"
                 AutogenerateGuids="true" 
                 GenerateGuidsNow="true"
                 ToolPath="$(WixToolPath)"
                 OutputFile="HarvestedFiles.wxs" />
</Target>
```

However, manual file listing provides better control over component organization and is recommended for this project.

### CI/CD Integration

**Build Script (PowerShell):**
```powershell
# Build all projects in Release configuration
msbuild SMaRT.sln /p:Configuration=Release /p:Platform="Any CPU"

# Build WiX installer
msbuild WixInstaller\WixInstaller.wixproj /p:Configuration=Release /p:Platform=x86

# Run ICE validation
smoke.exe WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi

# Copy output to artifacts directory
Copy-Item WixInstaller\bin\Release\UFDC_SMART_3.52.0.msi -Destination artifacts\
```

**Build Validation:**
- Verify exit codes from msbuild and smoke.exe
- Check for warnings in build output
- Validate MSI file size and structure
- Run automated installation tests

### Version Management Strategy

**For Each Release:**
1. Update version number in Product.wxs: `Version="3.52.0"`
2. Update version in app.config: `<add name="Version" value="3.52.0"/>`
3. Update output filename in .wixproj: `<OutputName>UFDC_SMART_3.52.0</OutputName>`
4. ProductCode is auto-generated (Id="*")
5. UpgradeCode remains constant across all versions

**Upgrade Code Management:**
- Generate once and never change
- Store in version control
- Document in README.md
- Same UpgradeCode enables upgrade detection

## Implementation Notes

### GUID Management

**UpgradeCode:**
- Generate once using `uuidgen` or online GUID generator
- Must remain constant across all versions
- Enables Windows Installer to detect previous installations

**ProductCode:**
- Use `Id="*"` to auto-generate for each build
- Ensures each version is uniquely identified
- Required for proper upgrade behavior

**Component GUIDs:**
- Use `Guid="*"` for auto-generation (recommended)
- Or use stable GUIDs based on file path
- Auto-generation is simpler and works well for most scenarios

### Platform Configuration

**Package Platform:**
```xml
<Package Platform="x86" />
```

Even though targeting 64-bit Windows, use x86 platform for the package itself. This is standard practice for .NET Framework applications and ensures compatibility. The application will install to Program Files (x86) on 64-bit systems.

### File Permissions

By default, files installed to Program Files have appropriate read/execute permissions. If SMaRT requires write access to its installation directory (e.g., for log files), add permission components:

```xml
<Component Id="InstallFolderPermissions" Directory="INSTALLFOLDER">
  <CreateFolder>
    <Permission User="Users" GenericAll="yes" />
  </CreateFolder>
</Component>
```

However, it's better practice to write user data to AppData or ProgramData instead of the installation directory.

### Icon Configuration

```xml
<Icon Id="SMaRT.ico" SourceFile="$(var.ProjectDir)..\Images\SMaRT.ico" />
<Property Id="ARPPRODUCTICON" Value="SMaRT.ico" />
```

This sets the icon for Programs and Features and shortcuts.

### Localization

The installer is configured for English (Language="1033"). For multi-language support, create separate builds with different language codes or use WiX localization features.

### Logging

Users can enable installation logging for troubleshooting:
```
msiexec /i UFDC_SMART_3.52.0.msi /l*v install.log
```

Consider documenting this in user-facing materials for support purposes.

