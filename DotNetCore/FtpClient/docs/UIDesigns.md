# FTP Client UI Design Specifications

## Executive Summary

The MFC FtpClient application contains 4 primary dialog interfaces that provide comprehensive FTP client functionality. Each dialog has been analyzed from the resource file specifications and converted to ASCII representations for .NET Core/WPF migration planning. The designs maintain functional parity while enabling modernization opportunities through WPF's advanced layout and styling capabilities.

## Main Application Dialog (IDD_FTP_DLG_EXAMPLE)

### Resource Specifications
- **Dialog ID**: IDD_FTP_DLG_EXAMPLE
- **Size**: 320x200 dialog units
- **Style**: Modal dialog with system menu
- **Evidence**: `FTPexample.rc` lines 73-88

### ASCII Layout Diagram
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ FTPexample                                                              [×] │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ [Browse for FTP-Files and Folders] [Logon Settings]              [Close]   │
│                                                                             │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │                                                                         │ │
│ │                    Protocol Output Window                               │ │
│ │                                                                         │ │
│ │  > USER anonymous                                                       │ │
│ │  < 331 Guest login ok, send your complete e-mail address as password   │ │
│ │  > PASS anonymous@user.com                                              │ │
│ │  < 230 Guest login ok, access restrictions apply                       │ │
│ │  > PWD                                                                  │ │
│ │  < 257 "/" is current directory                                         │ │
│ │  > TYPE I                                                               │ │
│ │  < 200 Type set to I                                                    │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Control Specifications
- **Browse Button**: 117x17 DU, launches file browser dialog
- **Logon Settings Button**: 92x17 DU, opens connection configuration
- **Close Button**: 50x16 DU, application exit
- **Protocol Output**: 306x146 DU, rich edit control with scroll bars

### WPF Migration Approach
```xml
<Window Title="FTP Client" Width="480" Height="300">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="10">
            <Button Content="Browse FTP Files" Command="{Binding BrowseCommand}"/>
            <Button Content="Connection Settings" Command="{Binding SettingsCommand}"/>
            <Button Content="Close" Command="{Binding CloseCommand}" HorizontalAlignment="Right"/>
        </StackPanel>
        
        <TextBox Grid.Row="1" Text="{Binding ProtocolOutput}" IsReadOnly="True" 
                 VerticalScrollBarVisibility="Auto" FontFamily="Consolas"/>
    </Grid>
</Window>
```

## FTP Logon Information Dialog (IDD_FTP_DLG_LOGON_INFO)

### Resource Specifications
- **Dialog ID**: IDD_FTP_DLG_LOGON_INFO
- **Size**: 302x255 dialog units
- **Style**: Modal dialog with system menu
- **Evidence**: `FTPexample.rc` lines 103-140

### ASCII Layout Diagram
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ FTP-Logon Information                                               [×] │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ ┌─ FTP Server ──────────────────────────────────────────────────────────┐   │
│ │                                                                       │   │
│ │ Hostname                    Port                                      │   │
│ │ [________________________] [____]                                    │   │
│ │                                                                       │   │
│ │ Username         Password           Account                           │   │
│ │ [_____________] [_____________] [______________]                       │   │
│ │                                                                       │   │
│ │ [Anonymous]                                                           │   │
│ └───────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│ ☐ Firewall                    ☐ Passive                                    │
│                                                                             │
│ ┌─ Firewall ────────────────────────────────────────────────────────────┐   │
│ │                                                                       │   │
│ │ Hostname                              Port                            │   │
│ │ [_________________________________] [____]                          │   │
│ │                                                                       │   │
│ │ Username         Password                                             │   │
│ │ [_____________] [_____________]                                        │   │
│ │                                                                       │   │
│ │ Type                                                                  │   │
│ │ [None                          ▼]                                     │   │
│ │                                                                       │   │
│ └───────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│                                                    [OK]     [Cancel]       │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Control Specifications
- **Server Group**: Contains hostname (120 DU), port (30 DU), credentials (85 DU each)
- **Anonymous Button**: 55x13 DU, populates default anonymous credentials
- **Firewall Checkbox**: Enables/disables firewall configuration group
- **Passive Checkbox**: Sets passive/active transfer mode
- **Firewall Group**: Conditional visibility based on firewall checkbox
- **Firewall Type Combo**: 145 DU width, dropdown with 9 firewall types

### WPF Migration Approach
```xml
<Window Title="FTP Connection Settings" Width="450" Height="380">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <GroupBox Grid.Row="0" Header="FTP Server">
            <Grid>
                <!-- Server connection controls -->
            </Grid>
        </GroupBox>
        
        <StackPanel Grid.Row="1" Orientation="Horizontal">
            <CheckBox Content="Use Firewall" IsChecked="{Binding UseFirewall}"/>
            <CheckBox Content="Passive Mode" IsChecked="{Binding PassiveMode}"/>
        </StackPanel>
        
        <GroupBox Grid.Row="2" Header="Firewall Settings" 
                  Visibility="{Binding UseFirewall, Converter={StaticResource BoolToVisibility}}">
            <Grid>
                <!-- Firewall configuration controls -->
            </Grid>
        </GroupBox>
        
        <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="OK" Command="{Binding OkCommand}"/>
            <Button Content="Cancel" Command="{Binding CancelCommand}"/>
        </StackPanel>
    </Grid>
</Window>
```

## FTP File Browser Dialog (IDD_FTP_DLG_BROWSE)

### Resource Specifications
- **Dialog ID**: IDD_FTP_DLG_BROWSE
- **Size**: 304x206 dialog units
- **Style**: Modal dialog with system menu
- **Evidence**: `FTPexample.rc` lines 90-101

### ASCII Layout Diagram
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ FTP File- and Folderselection                                           [×] │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ ⊞ ftp.example.com                                                      │ │
│ │   ├─⊞ pub                                                              │ │
│ │   │  ├─📁 documents                                                    │ │
│ │   │  ├─📁 software                                                     │ │
│ │   │  │  ├─📁 windows                                                   │ │
│ │   │  │  ├─📁 linux                                                     │ │
│ │   │  │  └─📁 mac                                                       │ │
│ │   │  ├─📄 readme.txt                                                   │ │
│ │   │  ├─📄 changelog.log                                                │ │
│ │   │  └─📄 license.pdf                                                  │ │
│ │   ├─⊞ incoming                                                         │ │
│ │   │  ├─📁 uploads                                                      │ │
│ │   │  └─📁 temp                                                         │ │
│ │   └─⊞ outgoing                                                         │ │
│ │      ├─📄 export_data.csv                                              │ │
│ │      └─📄 report.xml                                                   │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ │                                                                         │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│                                                    [OK]     [Cancel]       │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Control Specifications
- **Tree Control**: 290x173 DU, hierarchical file system display
- **Tree Features**: Lines at root, selection highlighting, expand/collapse buttons
- **OK Button**: 50x14 DU, confirms selection
- **Cancel Button**: 50x14 DU, cancels operation

### WPF Migration Approach
```xml
<Window Title="FTP File Browser" Width="450" Height="300">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <TreeView Grid.Row="0" ItemsSource="{Binding FtpDirectories}">
            <TreeView.ItemTemplate>
                <HierarchicalDataTemplate ItemsSource="{Binding Children}">
                    <StackPanel Orientation="Horizontal">
                        <Image Source="{Binding Icon}" Width="16" Height="16"/>
                        <TextBlock Text="{Binding Name}" Margin="5,0"/>
                    </StackPanel>
                </HierarchicalDataTemplate>
            </TreeView.ItemTemplate>
        </TreeView>
        
        <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="OK" Command="{Binding SelectCommand}"/>
            <Button Content="Cancel" Command="{Binding CancelCommand}"/>
        </StackPanel>
    </Grid>
</Window>
```

## FTP Progress Dialog (IDD_FTP_DLG_PROGRESS)

### Resource Specifications
- **Dialog ID**: IDD_FTP_DLG_PROGRESS
- **Size**: 305x102 dialog units
- **Style**: Modal dialog with 3D look
- **Evidence**: `FTPexample.rc` lines 142-157

### ASCII Layout Diagram
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ File Transfer Progress                                              [×] │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ [📁→📁] Transferring files...                           [Close] [Abort]    │
│                                                                             │
│                                                                             │
│ Source: /pub/software/application.zip                                      │
│                                                                             │
│ Target: C:\Downloads\application.zip                                       │
│                                                                             │
│ ████████████████████████████████████████████████████████████████████████    │
│ │████████████████████████████████████████████████████████████████████████│  │
│ └────────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│ Status: 15.2 MB of 20.8 MB transferred (73%) - 1.2 MB/s - 4s remaining    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Control Specifications
- **Animation Control**: 200x24 DU, displays transfer animation (AVI files)
- **Close Button**: 50x14 DU, closes dialog when transfer complete
- **Abort Button**: 50x14 DU, cancels active transfer
- **Progress Bar**: 291x13 DU, shows transfer completion percentage
- **Source File Label**: 291x12 DU, displays source file path
- **Target File Label**: 291x12 DU, displays destination file path
- **Status Label**: 291x12 DU, shows transfer statistics and rate

### WPF Migration Approach
```xml
<Window Title="Transfer Progress" Width="460" Height="150">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <StackPanel Grid.Row="0" Orientation="Horizontal">
            <ProgressRing IsActive="{Binding IsTransferring}" Width="24" Height="24"/>
            <TextBlock Text="Transferring files..." Margin="10,0"/>
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="Close" Command="{Binding CloseCommand}"/>
                <Button Content="Abort" Command="{Binding AbortCommand}"/>
            </StackPanel>
        </StackPanel>
        
        <TextBlock Grid.Row="1" Text="{Binding SourceFile, StringFormat='Source: {0}'}"/>
        <TextBlock Grid.Row="2" Text="{Binding TargetFile, StringFormat='Target: {0}'}"/>
        
        <ProgressBar Grid.Row="3" Value="{Binding ProgressPercentage}" Maximum="100" Height="20"/>
        
        <TextBlock Grid.Row="4" Text="{Binding StatusText}"/>
    </Grid>
</Window>
```

## Animation Resources

### Transfer Animations
- **Upload Animation**: `res\UPLOAD.AVI` - Visual feedback for upload operations
- **Download Animation**: `res\DOWNLOAD.AVI` - Visual feedback for download operations  
- **File Copy Animation**: `res\FILECOPY.AVI` - Visual feedback for FXP transfers

### WPF Animation Migration
```xml
<Storyboard x:Key="TransferAnimation">
    <DoubleAnimation Storyboard.TargetProperty="Opacity" 
                     From="0.5" To="1.0" Duration="0:0:1" 
                     RepeatBehavior="Forever" AutoReverse="True"/>
</Storyboard>
```

## Color Scheme and Styling

### Current MFC Styling
- **Background**: Standard Windows dialog background (typically light gray)
- **Controls**: Standard Windows control appearance
- **Fonts**: MS Shell Dlg, 8pt (system default)
- **Borders**: Standard 3D borders for edit controls and buttons

### Proposed WPF Styling
```xml
<Style x:Key="FtpDialogStyle" TargetType="Window">
    <Setter Property="Background" Value="#F0F0F0"/>
    <Setter Property="FontFamily" Value="Segoe UI"/>
    <Setter Property="FontSize" Value="12"/>
</Style>

<Style x:Key="FtpButtonStyle" TargetType="Button">
    <Setter Property="Padding" Value="10,5"/>
    <Setter Property="Margin" Value="5"/>
    <Setter Property="MinWidth" Value="75"/>
</Style>

<Style x:Key="FtpGroupBoxStyle" TargetType="GroupBox">
    <Setter Property="Margin" Value="5"/>
    <Setter Property="Padding" Value="10"/>
    <Setter Property="BorderBrush" Value="#CCCCCC"/>
</Style>
```

## Accessibility Considerations

### Current Accessibility
- **Keyboard Navigation**: Standard Windows tab order
- **Mnemonics**: Accelerator keys for buttons (Alt+key combinations)
- **Screen Reader**: Basic Windows accessibility support

### Enhanced WPF Accessibility
```xml
<!-- Enhanced accessibility attributes -->
<Button Content="_Browse" 
        AutomationProperties.Name="Browse FTP Files"
        AutomationProperties.HelpText="Opens dialog to browse remote FTP files"/>

<TextBox AutomationProperties.Name="FTP Hostname"
         AutomationProperties.HelpText="Enter the FTP server hostname or IP address"/>
```

## Responsive Design Considerations

### Fixed MFC Layout
- All dialogs use fixed dialog units
- No resizing capability
- Fixed control positions

### Flexible WPF Layout
```xml
<!-- Responsive grid layout -->
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
</Grid>

<!-- Adaptive button layout -->
<UniformGrid Columns="2" HorizontalAlignment="Right">
    <Button Content="OK"/>
    <Button Content="Cancel"/>
</UniformGrid>
```

## Implementation Priority

### Phase 1 - Core Dialogs
1. Main application window with protocol output
2. Connection settings dialog
3. Basic button and text control functionality

### Phase 2 - Advanced Controls
1. FTP file browser with tree view
2. Progress dialog with animations
3. Enhanced styling and theming

### Phase 3 - Polish and Accessibility
1. Keyboard navigation improvements
2. Screen reader compatibility
3. High DPI support
4. Dark theme support

## Testing Strategy

### Visual Regression Testing
- Screenshot comparison between MFC and WPF versions
- Layout verification across different screen resolutions
- Control alignment and spacing validation

### Functional Testing
- Dialog workflow testing (open, configure, close)
- Data binding validation
- Command execution verification
- Error handling and validation

### Accessibility Testing
- Keyboard-only navigation
- Screen reader compatibility
- High contrast mode support
- Focus management validation

## Conclusion

The FTP Client UI consists of 4 well-defined dialogs with clear functional boundaries. The migration to WPF enables significant improvements in styling, accessibility, and maintainability while preserving the familiar workflow. The ASCII diagrams provide clear visual specifications for the development team, and the proposed WPF implementations leverage modern MVVM patterns for better separation of concerns and testability.
