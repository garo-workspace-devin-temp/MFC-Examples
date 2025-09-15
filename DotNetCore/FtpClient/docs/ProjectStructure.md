# FTP Client Project Structure

## Executive Summary

The .NET Core FTP Client solution follows a clean architecture approach with clear separation of concerns across multiple projects. The structure supports maintainability, testability, and scalability while providing a modern development experience. The solution includes core business logic, UI components, infrastructure services, comprehensive testing, and proper dependency management using .NET 9 features.

## Solution Overview

### Solution File Structure
```
FtpClient.sln
├── src/
│   ├── FtpClient.Core/              # Core business logic and FTP protocol
│   ├── FtpClient.Contracts/         # Interfaces and data contracts
│   ├── FtpClient.Infrastructure/    # Infrastructure services
│   └── FtpClient.UI/                # WPF user interface
├── tests/
│   ├── FtpClient.Core.Tests/        # Unit tests for core library
│   ├── FtpClient.Integration.Tests/ # Integration tests
│   └── FtpClient.UI.Tests/          # UI and ViewModel tests
├── docs/
│   ├── api/                         # Generated API documentation
│   └── migration/                   # Migration documentation
├── tools/
│   ├── build/                       # Build scripts and tools
│   └── deployment/                  # Deployment configurations
└── samples/
    └── ConsoleClient/               # Console application example
```

## Core Library Project (FtpClient.Core)

### Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <AssemblyTitle>FTP Client Core Library</AssemblyTitle>
    <AssemblyDescription>Core FTP client functionality and protocol implementation</AssemblyDescription>
    <AssemblyVersion>2.0.0.0</AssemblyVersion>
    <FileVersion>2.0.0.0</FileVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
    <PackageReference Include="System.Text.Json" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\FtpClient.Contracts\FtpClient.Contracts.csproj" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="FtpClient.Core.Tests" />
    <InternalsVisibleTo Include="FtpClient.Integration.Tests" />
  </ItemGroup>

</Project>
```

### Directory Structure
```
FtpClient.Core/
├── Commands/
│   ├── FtpCommand.cs
│   ├── FtpCommandType.cs
│   └── FtpCommandFactory.cs
├── Responses/
│   ├── FtpReply.cs
│   ├── FtpReplyParser.cs
│   └── FtpReplyCode.cs
├── Client/
│   ├── FtpClient.cs
│   ├── FtpClientOptions.cs
│   └── FtpConnectionInfo.cs
├── Transfer/
│   ├── FtpTransferService.cs
│   ├── TransferProgress.cs
│   └── TransferStatistics.cs
├── Directory/
│   ├── FtpDirectoryService.cs
│   ├── FtpFileInfo.cs
│   └── FtpListParser.cs
├── Networking/
│   ├── FtpSocket.cs
│   ├── FtpSocketFactory.cs
│   └── FtpDataConnection.cs
├── Authentication/
│   ├── FtpAuthenticator.cs
│   ├── FirewallHandler.cs
│   └── FirewallType.cs
├── Utilities/
│   ├── FtpPathUtilities.cs
│   ├── FtpDateTimeParser.cs
│   └── FtpEncoding.cs
└── Extensions/
    ├── ServiceCollectionExtensions.cs
    └── StringExtensions.cs
```

### Namespace Organization
```csharp
// Root namespace
namespace FtpClient.Core;

// Feature-based namespaces
namespace FtpClient.Core.Commands;
namespace FtpClient.Core.Responses;
namespace FtpClient.Core.Client;
namespace FtpClient.Core.Transfer;
namespace FtpClient.Core.Directory;
namespace FtpClient.Core.Networking;
namespace FtpClient.Core.Authentication;
namespace FtpClient.Core.Utilities;
namespace FtpClient.Core.Extensions;
```

## Contracts Project (FtpClient.Contracts)

### Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <AssemblyTitle>FTP Client Contracts</AssemblyTitle>
    <AssemblyDescription>Interfaces and data contracts for FTP client</AssemblyDescription>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>

</Project>
```

### Directory Structure
```
FtpClient.Contracts/
├── Client/
│   ├── IFtpClient.cs
│   ├── IFtpClientFactory.cs
│   └── FtpResult.cs
├── Transfer/
│   ├── IFtpTransferService.cs
│   ├── ITransferProgress.cs
│   └── TransferOptions.cs
├── Directory/
│   ├── IFtpDirectoryService.cs
│   ├── IFtpFileInfo.cs
│   └── DirectoryListOptions.cs
├── Networking/
│   ├── IFtpSocket.cs
│   ├── IFtpSocketFactory.cs
│   └── ConnectionOptions.cs
├── Authentication/
│   ├── IFtpAuthenticator.cs
│   ├── IFirewallHandler.cs
│   └── AuthenticationOptions.cs
├── Events/
│   ├── FtpEventArgs.cs
│   ├── TransferEventArgs.cs
│   └── ErrorEventArgs.cs
└── Configuration/
    ├── IFtpConfiguration.cs
    ├── ConnectionProfile.cs
    └── ClientSettings.cs
```

## Infrastructure Project (FtpClient.Infrastructure)

### Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <AssemblyTitle>FTP Client Infrastructure</AssemblyTitle>
    <AssemblyDescription>Infrastructure services for FTP client</AssemblyDescription>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.0" />
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\FtpClient.Contracts\FtpClient.Contracts.csproj" />
  </ItemGroup>

</Project>
```

### Directory Structure
```
FtpClient.Infrastructure/
├── Configuration/
│   ├── FtpConfigurationService.cs
│   ├── JsonConfigurationProvider.cs
│   └── EncryptedSettingsProvider.cs
├── Logging/
│   ├── FtpLoggerProvider.cs
│   ├── FileLogger.cs
│   └── ProtocolLogger.cs
├── Security/
│   ├── PasswordEncryption.cs
│   ├── CertificateValidator.cs
│   └── SecureStringExtensions.cs
├── Persistence/
│   ├── ConnectionProfileRepository.cs
│   ├── TransferHistoryRepository.cs
│   └── SettingsRepository.cs
└── DependencyInjection/
    ├── InfrastructureServiceExtensions.cs
    └── ServiceRegistration.cs
```

## UI Project (FtpClient.UI)

### Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AssemblyTitle>FTP Client</AssemblyTitle>
    <AssemblyDescription>WPF FTP Client Application</AssemblyDescription>
    <ApplicationIcon>Resources\FtpClient.ico</ApplicationIcon>
    <StartupObject>FtpClient.UI.App</StartupObject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Toolkit.Mvvm" Version="7.1.2" />
    <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
    <PackageReference Include="MaterialDesignColors" Version="2.1.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\FtpClient.Core\FtpClient.Core.csproj" />
    <ProjectReference Include="..\FtpClient.Infrastructure\FtpClient.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Resource Include="Resources\**\*" />
  </ItemGroup>

</Project>
```

### Directory Structure
```
FtpClient.UI/
├── Views/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── ConnectionSettingsWindow.xaml
│   ├── ConnectionSettingsWindow.xaml.cs
│   ├── FtpBrowserWindow.xaml
│   ├── FtpBrowserWindow.xaml.cs
│   ├── TransferProgressWindow.xaml
│   └── TransferProgressWindow.xaml.cs
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── MainWindowViewModel.cs
│   ├── ConnectionSettingsViewModel.cs
│   ├── FtpBrowserViewModel.cs
│   ├── TransferProgressViewModel.cs
│   └── TransferQueueViewModel.cs
├── Controls/
│   ├── FtpTreeView.xaml
│   ├── FtpTreeView.xaml.cs
│   ├── ProtocolOutputControl.xaml
│   ├── ProtocolOutputControl.xaml.cs
│   ├── TransferProgressControl.xaml
│   └── TransferProgressControl.xaml.cs
├── Converters/
│   ├── BooleanToVisibilityConverter.cs
│   ├── FileSizeConverter.cs
│   ├── TransferRateConverter.cs
│   └── DateTimeConverter.cs
├── Commands/
│   ├── AsyncRelayCommand.cs
│   ├── RelayCommand.cs
│   └── DelegateCommand.cs
├── Services/
│   ├── DialogService.cs
│   ├── FileDialogService.cs
│   ├── NotificationService.cs
│   └── ThemeService.cs
├── Models/
│   ├── FtpTreeNode.cs
│   ├── TransferItem.cs
│   ├── ConnectionProfileModel.cs
│   └── SettingsModel.cs
├── Resources/
│   ├── Styles/
│   │   ├── Application.xaml
│   │   ├── ButtonStyles.xaml
│   │   ├── TextBoxStyles.xaml
│   │   └── TreeViewStyles.xaml
│   ├── Images/
│   │   ├── FtpClient.ico
│   │   ├── folder.png
│   │   ├── file.png
│   │   └── server.png
│   └── Themes/
│       ├── Light.xaml
│       └── Dark.xaml
├── App.xaml
├── App.xaml.cs
└── AssemblyInfo.cs
```

### MVVM Architecture Implementation

```csharp
// ViewModelBase.cs
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// MainWindowViewModel.cs
public class MainWindowViewModel : ViewModelBase
{
    private readonly IFtpClient _ftpClient;
    private readonly IDialogService _dialogService;
    private readonly ObservableCollection<string> _protocolOutput = new();
    
    public ReadOnlyObservableCollection<string> ProtocolOutput { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand BrowseCommand { get; }
    public ICommand SettingsCommand { get; }
    
    public MainWindowViewModel(IFtpClient ftpClient, IDialogService dialogService)
    {
        _ftpClient = ftpClient;
        _dialogService = dialogService;
        ProtocolOutput = new ReadOnlyObservableCollection<string>(_protocolOutput);
        
        ConnectCommand = new AsyncRelayCommand(ConnectAsync, CanConnect);
        DisconnectCommand = new AsyncRelayCommand(DisconnectAsync, CanDisconnect);
        BrowseCommand = new AsyncRelayCommand(BrowseAsync, CanBrowse);
        SettingsCommand = new RelayCommand(ShowSettings);
        
        _ftpClient.CommandSent += OnCommandSent;
        _ftpClient.ReplyReceived += OnReplyReceived;
    }
}
```

## Test Projects Structure

### Unit Tests Project (FtpClient.Core.Tests)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Testing" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\FtpClient.Core\FtpClient.Core.csproj" />
    <ProjectReference Include="..\..\src\FtpClient.Contracts\FtpClient.Contracts.csproj" />
  </ItemGroup>

</Project>
```

### Test Directory Structure
```
FtpClient.Core.Tests/
├── Client/
│   ├── FtpClientTests.cs
│   ├── FtpConnectionInfoTests.cs
│   └── FtpClientOptionsTests.cs
├── Commands/
│   ├── FtpCommandTests.cs
│   ├── FtpCommandFactoryTests.cs
│   └── FtpCommandTypeTests.cs
├── Responses/
│   ├── FtpReplyTests.cs
│   ├── FtpReplyParserTests.cs
│   └── FtpReplyCodeTests.cs
├── Transfer/
│   ├── FtpTransferServiceTests.cs
│   ├── TransferProgressTests.cs
│   └── TransferStatisticsTests.cs
├── Directory/
│   ├── FtpDirectoryServiceTests.cs
│   ├── FtpFileInfoTests.cs
│   └── FtpListParserTests.cs
├── Networking/
│   ├── FtpSocketTests.cs
│   ├── FtpSocketFactoryTests.cs
│   └── FtpDataConnectionTests.cs
├── Authentication/
│   ├── FtpAuthenticatorTests.cs
│   ├── FirewallHandlerTests.cs
│   └── FirewallTypeTests.cs
├── Utilities/
│   ├── FtpPathUtilitiesTests.cs
│   ├── FtpDateTimeParserTests.cs
│   └── FtpEncodingTests.cs
└── TestHelpers/
    ├── MockFtpSocket.cs
    ├── TestFtpServer.cs
    └── FtpTestUtilities.cs
```

### Integration Tests Project (FtpClient.Integration.Tests)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Docker.DotNet" Version="3.125.15" />
    <PackageReference Include="Testcontainers" Version="3.6.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\FtpClient.Core\FtpClient.Core.csproj" />
    <ProjectReference Include="..\..\src\FtpClient.Infrastructure\FtpClient.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="TestData\**\*" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

### UI Tests Project (FtpClient.UI.Tests)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.Toolkit.Mvvm" Version="7.1.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\FtpClient.UI\FtpClient.UI.csproj" />
    <ProjectReference Include="..\..\src\FtpClient.Core\FtpClient.Core.csproj" />
  </ItemGroup>

</Project>
```

## Build and Deployment Configuration

### Directory.Build.props (Solution Root)
```xml
<Project>
  
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>NU1701</WarningsNotAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>

  <PropertyGroup>
    <Company>FTP Client Project</Company>
    <Product>FTP Client</Product>
    <Copyright>Copyright © 2024</Copyright>
    <NeutralLanguage>en-US</NeutralLanguage>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

### EditorConfig (.editorconfig)
```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,vb}]
indent_style = space
indent_size = 4

[*.{xml,xaml,json,yml,yaml}]
indent_style = space
indent_size = 2

[*.cs]
# C# Code Style Rules
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_members_in_anonymous_types = true
csharp_new_line_between_query_expression_clauses = true

# Indentation preferences
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_indent_labels = flush_left

# Space preferences
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_call_parameter_list_parentheses = false
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_between_parentheses = false
csharp_space_before_colon_in_inheritance_clause = true
csharp_space_after_colon_in_inheritance_clause = true
csharp_space_around_binary_operators = before_and_after
csharp_space_between_method_declaration_empty_parameter_list_parentheses = false
csharp_space_between_method_call_name_and_opening_parenthesis = false
csharp_space_between_method_call_empty_parameter_list_parentheses = false

# Wrapping preferences
csharp_preserve_single_line_statements = true
csharp_preserve_single_line_blocks = true

# .NET Code Style Rules
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Naming conventions
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.severity = warning
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.symbols = interface
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.style = prefix_interface_with_i

dotnet_naming_rule.types_should_be_pascal_case.severity = warning
dotnet_naming_rule.types_should_be_pascal_case.symbols = types
dotnet_naming_rule.types_should_be_pascal_case.style = pascal_case

dotnet_naming_rule.non_field_members_should_be_pascal_case.severity = warning
dotnet_naming_rule.non_field_members_should_be_pascal_case.symbols = non_field_members
dotnet_naming_rule.non_field_members_should_be_pascal_case.style = pascal_case

# Symbol specifications
dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_symbols.interface.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected

dotnet_naming_symbols.types.applicable_kinds = class, struct, interface, enum
dotnet_naming_symbols.types.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected

dotnet_naming_symbols.non_field_members.applicable_kinds = property, event, method
dotnet_naming_symbols.non_field_members.applicable_accessibilities = public, internal, private, protected, protected_internal, private_protected

# Naming styles
dotnet_naming_style.pascal_case.capitalization = pascal_case

dotnet_naming_style.prefix_interface_with_i.capitalization = pascal_case
dotnet_naming_style.prefix_interface_with_i.required_prefix = I
```

### Global.json
```json
{
  "sdk": {
    "version": "9.0.100",
    "rollForward": "latestMinor"
  },
  "msbuild-sdks": {
    "Microsoft.Build.Traversal": "3.4.0"
  }
}
```

## NuGet Package Management

### Central Package Management (Directory.Packages.props)
```xml
<Project>
  
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- Microsoft Extensions -->
    <PackageVersion Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.0" />
    
    <!-- UI Packages -->
    <PackageVersion Include="Microsoft.Toolkit.Mvvm" Version="7.1.2" />
    <PackageVersion Include="MaterialDesignThemes" Version="4.9.0" />
    <PackageVersion Include="MaterialDesignColors" Version="2.1.4" />
    
    <!-- System Packages -->
    <PackageVersion Include="System.Text.Json" Version="9.0.0" />
    <PackageVersion Include="System.ComponentModel.Annotations" Version="5.0.0" />
    <PackageVersion Include="System.Security.Cryptography.ProtectedData" Version="9.0.0" />
    
    <!-- Testing Packages -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageVersion Include="xunit" Version="2.6.1" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageVersion Include="Moq" Version="4.20.69" />
    <PackageVersion Include="FluentAssertions" Version="6.12.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Testing" Version="9.0.0" />
    <PackageVersion Include="Docker.DotNet" Version="3.125.15" />
    <PackageVersion Include="Testcontainers" Version="3.6.0" />
    
    <!-- Code Analysis -->
    <PackageVersion Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
    <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
  </ItemGroup>

</Project>
```

## Development Tools and Scripts

### Build Script (tools/build/build.ps1)
```powershell
#!/usr/bin/env pwsh

param(
    [string]$Configuration = "Release",
    [string]$Verbosity = "minimal",
    [switch]$Clean,
    [switch]$Test,
    [switch]$Pack,
    [switch]$Publish
)

$ErrorActionPreference = "Stop"

$solutionPath = Join-Path $PSScriptRoot "../../FtpClient.sln"

if ($Clean) {
    Write-Host "Cleaning solution..." -ForegroundColor Green
    dotnet clean $solutionPath --configuration $Configuration --verbosity $Verbosity
}

Write-Host "Restoring packages..." -ForegroundColor Green
dotnet restore $solutionPath --verbosity $Verbosity

Write-Host "Building solution..." -ForegroundColor Green
dotnet build $solutionPath --configuration $Configuration --no-restore --verbosity $Verbosity

if ($Test) {
    Write-Host "Running tests..." -ForegroundColor Green
    dotnet test $solutionPath --configuration $Configuration --no-build --verbosity $Verbosity --collect:"XPlat Code Coverage"
}

if ($Pack) {
    Write-Host "Creating NuGet packages..." -ForegroundColor Green
    dotnet pack $solutionPath --configuration $Configuration --no-build --verbosity $Verbosity
}

if ($Publish) {
    Write-Host "Publishing application..." -ForegroundColor Green
    $publishPath = Join-Path $PSScriptRoot "../../publish"
    dotnet publish "src/FtpClient.UI/FtpClient.UI.csproj" --configuration $Configuration --output $publishPath --verbosity $Verbosity
}

Write-Host "Build completed successfully!" -ForegroundColor Green
```

### Development Environment Setup (tools/setup-dev.ps1)
```powershell
#!/usr/bin/env pwsh

Write-Host "Setting up FTP Client development environment..." -ForegroundColor Green

# Check .NET SDK version
$requiredSdkVersion = "9.0.100"
$installedSdk = dotnet --version

if ($installedSdk -lt $requiredSdkVersion) {
    Write-Error "Required .NET SDK version $requiredSdkVersion or higher. Found: $installedSdk"
    exit 1
}

Write-Host "✓ .NET SDK version: $installedSdk" -ForegroundColor Green

# Restore tools
Write-Host "Restoring .NET tools..." -ForegroundColor Yellow
dotnet tool restore

# Install/update global tools
Write-Host "Installing global tools..." -ForegroundColor Yellow
dotnet tool install --global dotnet-format --version 5.1.250801
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.0

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Debug

Write-Host "Development environment setup completed!" -ForegroundColor Green
Write-Host "You can now open the solution in Visual Studio or VS Code." -ForegroundColor Cyan
```

## Deployment Configuration

### ClickOnce Deployment (FtpClient.UI.csproj additions)
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <PublishUrl>publish\</PublishUrl>
  <Install>true</Install>
  <InstallFrom>Disk</InstallFrom>
  <UpdateEnabled>true</UpdateEnabled>
  <UpdateMode>Foreground</UpdateMode>
  <UpdateInterval>7</UpdateInterval>
  <UpdateIntervalUnits>Days</UpdateIntervalUnits>
  <UpdatePeriodically>false</UpdatePeriodically>
  <UpdateRequired>false</UpdateRequired>
  <MapFileExtensions>true</MapFileExtensions>
  <ApplicationRevision>1</ApplicationRevision>
  <ApplicationVersion>2.0.0.1</ApplicationVersion>
  <UseApplicationTrust>false</UseApplicationTrust>
  <PublishWizardShown>false</PublishWizardShown>
  <BootstrapperEnabled>true</BootstrapperEnabled>
</PropertyGroup>
```

### Docker Support (Dockerfile)
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/FtpClient.UI/FtpClient.UI.csproj", "src/FtpClient.UI/"]
COPY ["src/FtpClient.Core/FtpClient.Core.csproj", "src/FtpClient.Core/"]
COPY ["src/FtpClient.Contracts/FtpClient.Contracts.csproj", "src/FtpClient.Contracts/"]
COPY ["src/FtpClient.Infrastructure/FtpClient.Infrastructure.csproj", "src/FtpClient.Infrastructure/"]
RUN dotnet restore "src/FtpClient.UI/FtpClient.UI.csproj"
COPY . .
WORKDIR "/src/src/FtpClient.UI"
RUN dotnet build "FtpClient.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FtpClient.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FtpClient.UI.dll"]
```

## Documentation Generation

### API Documentation (docs/api/docfx.json)
```json
{
  "metadata": [
    {
      "src": [
        {
          "files": ["src/**/*.csproj"],
          "exclude": ["**/bin/**", "**/obj/**"]
        }
      ],
      "dest": "api",
      "includePrivateMembers": false,
      "disableGitFeatures": false,
      "disableDefaultFilter": false,
      "noRestore": false,
      "namespaceLayout": "flattened",
      "memberLayout": "samePage"
    }
  ],
  "build": {
    "content": [
      {
        "files": ["api/**.yml", "api/index.md"]
      },
      {
        "files": ["migration/**.md"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "overwrite": [
      {
        "files": ["apidoc/**.md"],
        "exclude": ["obj/**", "_site/**"]
      }
    ],
    "dest": "_site",
    "globalMetadataFiles": [],
    "fileMetadataFiles": [],
    "template": ["default"],
    "postProcessors": [],
    "markdownEngineName": "markdig",
    "noLangKeyword": false,
    "keepFileLink": false,
    "cleanupCacheHistory": false,
    "disableGitFeatures": false
  }
}
```

## Sample Console Application

### Console Client Project (samples/ConsoleClient/ConsoleClient.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\FtpClient.Core\FtpClient.Core.csproj" />
    <ProjectReference Include="..\..\src\FtpClient.Infrastructure\FtpClient.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
  </ItemGroup>

</Project>
```

### Console Application Example (samples/ConsoleClient/Program.cs)
```csharp
using FtpClient.Core.Extensions;
using FtpClient.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure services
builder.Services.AddFtpClient();
builder.Services.AddFtpInfrastructure();
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var host = builder.Build();

// Example usage
var ftpClient = host.Services.GetRequiredService<IFtpClient>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting FTP client example...");
    
    var connectionInfo = new FtpConnectionInfo("ftp.example.com", 21, "anonymous", "user@example.com");
    var result = await ftpClient.ConnectAsync(connectionInfo);
    
    if (result.IsSuccess)
    {
        logger.LogInformation("Connected successfully!");
        
        var files = await ftpClient.ListDirectoryAsync("/");
        if (files.IsSuccess)
        {
            foreach (var file in files.Value)
            {
                logger.LogInformation("File: {Name} ({Size} bytes)", file.Name, file.Size);
            }
        }
        
        await ftpClient.DisconnectAsync();
    }
    else
    {
        logger.LogError("Connection failed: {Error}", result.ErrorMessage);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred");
}

logger.LogInformation("Example completed.");
```

## IDE Configuration

### Visual Studio Solution Items
```
FtpClient.sln
├── Solution Items/
│   ├── .editorconfig
│   ├── .gitignore
│   ├── Directory.Build.props
│   ├── Directory.Packages.props
│   ├── global.json
│   ├── README.md
│   └── LICENSE
```

### VS Code Configuration (.vscode/settings.json)
```json
{
  "dotnet.defaultSolution": "FtpClient.sln",
  "omnisharp.enableEditorConfigSupport": true,
  "omnisharp.enableRoslynAnalyzers": true,
  "files.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/.vs": true
  },
  "csharp.format.enable": true,
  "csharp.semanticHighlighting.enabled": true,
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true,
    "source.organizeImports": true
  }
}
```

### VS Code Tasks (.vscode/tasks.json)
```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build", "${workspaceFolder}/FtpClient.sln"],
      "group": "build",
      "presentation": {
        "reveal": "silent"
      },
      "problemMatcher": "$msCompile"
    },
    {
      "label": "test",
      "command": "dotnet",
      "type": "process",
      "args": ["test", "${workspaceFolder}/FtpClient.sln"],
      "group": "test",
      "presentation": {
        "reveal": "always"
      },
      "problemMatcher": "$msCompile"
    },
    {
      "label": "clean",
      "command": "dotnet",
      "type": "process",
      "args": ["clean", "${workspaceFolder}/FtpClient.sln"],
      "group": "build",
      "presentation": {
        "reveal": "silent"
      },
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## Conclusion

This project structure provides a solid foundation for the .NET Core FTP Client application with clear separation of concerns, comprehensive testing support, and modern development practices. The structure supports both Visual Studio and VS Code development environments, includes proper dependency management, and provides deployment flexibility through multiple distribution channels.

The modular architecture allows for independent development and testing of components while maintaining clear boundaries between layers. The extensive tooling and configuration ensure consistent code quality and development experience across the team.
