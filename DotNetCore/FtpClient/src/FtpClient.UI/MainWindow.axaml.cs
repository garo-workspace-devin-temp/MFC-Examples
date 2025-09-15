using Avalonia.Controls;
using Avalonia.Interactivity;
using FtpClient.UI.Models;
using FtpClient.UI.Services;
using FtpClient.UI.Views;
using System;
using System.Threading.Tasks;

namespace FtpClient.UI;

public partial class MainWindow : Window
{
    private readonly Services.FtpClient _ftpClient;
    private LogonInfo? _currentLogonInfo;

    public MainWindow()
    {
        InitializeComponent();
        _ftpClient = new Services.FtpClient();
        
        _ftpClient.CommandSent += OnCommandSent;
        _ftpClient.ResponseReceived += OnResponseReceived;
    }

    private void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += "\nBrowse FTP Files functionality will be implemented in future iterations.";
        }
    }

    private async void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new ConnectionSettingsDialog(_currentLogonInfo ?? new LogonInfo());
        
        await dialog.ShowDialog(this);
        
        if (dialog.DialogResult)
        {
            _currentLogonInfo = dialog.LogonInfo;
            if (_currentLogonInfo != null)
            {
                await AttemptLoginAsync(_currentLogonInfo);
            }
        }
    }

    private async Task AttemptLoginAsync(LogonInfo logonInfo)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += $"\nConnecting to {logonInfo.Hostname}:{logonInfo.Hostport}...";
        }

        try
        {
            bool success = await _ftpClient.LoginAsync(logonInfo);
            
            if (ProtocolOutput != null)
            {
                if (success)
                {
                    ProtocolOutput.Text += "\nLogin successful! Connected to FTP server.";
                }
                else
                {
                    ProtocolOutput.Text += "\nLogin failed. Please check your connection settings.";
                }
            }
        }
        catch (Exception ex)
        {
            if (ProtocolOutput != null)
            {
                ProtocolOutput.Text += $"\nConnection error: {ex.Message}";
            }
        }
    }

    private void OnCommandSent(string command)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += $"\n> {command}";
        }
    }

    private void OnResponseReceived(string response)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += $"\n< {response}";
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        _ftpClient?.Dispose();
        Environment.Exit(0);
    }

    protected override void OnClosed(EventArgs e)
    {
        _ftpClient?.Dispose();
        base.OnClosed(e);
    }
}
