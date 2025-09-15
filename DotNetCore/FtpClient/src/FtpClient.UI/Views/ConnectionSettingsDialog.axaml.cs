using Avalonia.Controls;
using Avalonia.Interactivity;
using FtpClient.UI.Models;
using System;

namespace FtpClient.UI.Views;

public partial class ConnectionSettingsDialog : Window
{
    public LogonInfo? LogonInfo { get; private set; }
    public bool DialogResult { get; private set; } = false;

    public ConnectionSettingsDialog()
    {
        InitializeComponent();
        InitializeFirewallControls();
    }

    public ConnectionSettingsDialog(LogonInfo logonInfo) : this()
    {
        SetLogonInfo(logonInfo);
    }

    private void InitializeFirewallControls()
    {
        if (FirewallTypeComboBox != null)
        {
            FirewallTypeComboBox.SelectedIndex = 0;
        }
        UpdateFirewallControlsState();
    }

    public void SetLogonInfo(LogonInfo logonInfo)
    {
        if (HostnameTextBox != null) HostnameTextBox.Text = logonInfo.Hostname;
        if (PortTextBox != null) PortTextBox.Text = logonInfo.Hostport.ToString();
        if (UsernameTextBox != null) UsernameTextBox.Text = logonInfo.Username;
        if (PasswordTextBox != null) PasswordTextBox.Text = logonInfo.Password;
        if (AccountTextBox != null) AccountTextBox.Text = logonInfo.Account;
        if (PassiveModeCheckBox != null) PassiveModeCheckBox.IsChecked = logonInfo.PassiveMode;

        if (UseFirewallCheckBox != null) UseFirewallCheckBox.IsChecked = logonInfo.UseFirewall;
        if (FirewallTypeComboBox != null) FirewallTypeComboBox.SelectedIndex = (int)logonInfo.FirewallType;
        if (FirewallHostnameTextBox != null) FirewallHostnameTextBox.Text = logonInfo.FirewallHostname;
        if (FirewallPortTextBox != null) FirewallPortTextBox.Text = logonInfo.FirewallPort.ToString();
        if (FirewallUsernameTextBox != null) FirewallUsernameTextBox.Text = logonInfo.FirewallUsername;
        if (FirewallPasswordTextBox != null) FirewallPasswordTextBox.Text = logonInfo.FirewallPassword;

        UpdateFirewallControlsState();
    }

    public LogonInfo GetLogonInfo()
    {
        var logonInfo = new LogonInfo();

        if (HostnameTextBox != null) logonInfo.Hostname = HostnameTextBox.Text ?? string.Empty;
        if (PortTextBox != null && ushort.TryParse(PortTextBox.Text, out ushort port)) logonInfo.Hostport = port;
        if (UsernameTextBox != null) logonInfo.Username = UsernameTextBox.Text ?? string.Empty;
        if (PasswordTextBox != null) logonInfo.Password = PasswordTextBox.Text ?? string.Empty;
        if (AccountTextBox != null) logonInfo.Account = AccountTextBox.Text ?? string.Empty;
        if (PassiveModeCheckBox != null) logonInfo.PassiveMode = PassiveModeCheckBox.IsChecked ?? false;

        if (UseFirewallCheckBox != null) logonInfo.UseFirewall = UseFirewallCheckBox.IsChecked ?? false;
        if (FirewallTypeComboBox != null) logonInfo.FirewallType = (FirewallType)(FirewallTypeComboBox.SelectedIndex);
        if (FirewallHostnameTextBox != null) logonInfo.FirewallHostname = FirewallHostnameTextBox.Text ?? string.Empty;
        if (FirewallPortTextBox != null && ushort.TryParse(FirewallPortTextBox.Text, out ushort fwPort)) logonInfo.FirewallPort = fwPort;
        if (FirewallUsernameTextBox != null) logonInfo.FirewallUsername = FirewallUsernameTextBox.Text ?? string.Empty;
        if (FirewallPasswordTextBox != null) logonInfo.FirewallPassword = FirewallPasswordTextBox.Text ?? string.Empty;

        return logonInfo;
    }

    private void AnonymousButton_Click(object? sender, RoutedEventArgs e)
    {
        if (UsernameTextBox != null) UsernameTextBox.Text = LogonInfo.AnonymousUser;
        if (PasswordTextBox != null) PasswordTextBox.Text = LogonInfo.AnonymousPassword;
    }

    private void UseFirewallCheckBox_Changed(object? sender, RoutedEventArgs e)
    {
        UpdateFirewallControlsState();
    }

    private void UpdateFirewallControlsState()
    {
        bool useFirewall = UseFirewallCheckBox?.IsChecked ?? false;
        
        if (FirewallTypeComboBox != null) FirewallTypeComboBox.IsEnabled = useFirewall;
        if (FirewallHostnameTextBox != null) FirewallHostnameTextBox.IsEnabled = useFirewall;
        if (FirewallPortTextBox != null) FirewallPortTextBox.IsEnabled = useFirewall;
        if (FirewallUsernameTextBox != null) FirewallUsernameTextBox.IsEnabled = useFirewall;
        if (FirewallPasswordTextBox != null) FirewallPasswordTextBox.IsEnabled = useFirewall;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        LogonInfo = GetLogonInfo();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
