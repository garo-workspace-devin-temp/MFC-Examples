using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
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
    private DispatcherTimer? _statusUpdateTimer;

    public MainWindow()
    {
        InitializeComponent();
        _ftpClient = new Services.FtpClient();
        
        _ftpClient.CommandSent += OnCommandSent;
        _ftpClient.ResponseReceived += OnResponseReceived;
        _ftpClient.ConnectionStateChanged += OnConnectionStateChanged;
        
        InitializeStatusUpdateTimer();
        UpdateConnectionStatus();
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!_ftpClient.IsAuthenticated)
        {
            if (ProtocolOutput != null)
            {
                ProtocolOutput.Text += "\nPlease connect to an FTP server first using Connection Settings.";
            }
            return;
        }

        try
        {
            var browseDialog = new Views.FtpBrowseDialog(_ftpClient);
            var result = await browseDialog.ShowDialogAsync(this);
            
            if (result && ProtocolOutput != null)
            {
                ProtocolOutput.Text += $"\nSelected: {browseDialog.SelectedPath}";
            }
        }
        catch (Exception ex)
        {
            if (ProtocolOutput != null)
            {
                ProtocolOutput.Text += $"\nBrowse error: {ex.Message}";
            }
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

    private async void DisconnectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += "\nDisconnecting from FTP server...";
        }
        
        await _ftpClient.LogoutAsync();
    }

    private async void ReconnectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += "\nReconnecting to FTP server...";
        }
        
        bool success = await _ftpClient.ReconnectAsync();
        
        if (ProtocolOutput != null)
        {
            if (success)
            {
                ProtocolOutput.Text += "\nReconnection successful!";
            }
            else
            {
                ProtocolOutput.Text += "\nReconnection failed. Please check your connection settings.";
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

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            UpdateConnectionStatus();
            
            if (ProtocolOutput != null)
            {
                string stateMessage = e.NewState switch
                {
                    ConnectionState.Connecting => "Connecting...",
                    ConnectionState.Connected => "Connected to server",
                    ConnectionState.Authenticating => "Authenticating...",
                    ConnectionState.Authenticated => "Authentication successful",
                    ConnectionState.Disconnecting => "Disconnecting...",
                    ConnectionState.Disconnected => "Disconnected",
                    ConnectionState.Error => $"Connection error: {e.Message ?? e.Exception?.Message ?? "Unknown error"}",
                    _ => $"State changed to {e.NewState}"
                };
                
                ProtocolOutput.Text += $"\n[{DateTime.Now:HH:mm:ss}] {stateMessage}";
            }
        });
    }

    private void UpdateConnectionStatus()
    {
        if (ConnectionStatusText == null || StatusIndicator == null || 
            DisconnectButton == null || ReconnectButton == null ||
            ServerInfoText == null) return;

        var state = _ftpClient.ConnectionState;
        
        ConnectionStatusText.Text = state switch
        {
            ConnectionState.Disconnected => "Disconnected",
            ConnectionState.Connecting => "Connecting...",
            ConnectionState.Connected => "Connected",
            ConnectionState.Authenticating => "Authenticating...",
            ConnectionState.Authenticated => "Authenticated",
            ConnectionState.Disconnecting => "Disconnecting...",
            ConnectionState.Error => "Error",
            _ => state.ToString()
        };

        StatusIndicator.Fill = state switch
        {
            ConnectionState.Disconnected => Brushes.Gray,
            ConnectionState.Connecting => Brushes.Orange,
            ConnectionState.Connected => Brushes.Yellow,
            ConnectionState.Authenticating => Brushes.Orange,
            ConnectionState.Authenticated => Brushes.Green,
            ConnectionState.Disconnecting => Brushes.Orange,
            ConnectionState.Error => Brushes.Red,
            _ => Brushes.Gray
        };

        DisconnectButton.IsEnabled = _ftpClient.IsConnected;
        ReconnectButton.IsEnabled = !_ftpClient.IsConnected && _ftpClient.LastLogonInfo != null;

        if (_ftpClient.LastLogonInfo != null && _ftpClient.IsAuthenticated)
        {
            ServerInfoText.Text = $"Connected to {_ftpClient.LastLogonInfo.Hostname}:{_ftpClient.LastLogonInfo.Hostport}";
        }
        else
        {
            ServerInfoText.Text = "";
        }
    }

    private void InitializeStatusUpdateTimer()
    {
        _statusUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _statusUpdateTimer.Tick += OnStatusUpdateTimer;
        _statusUpdateTimer.Start();
    }

    private void OnStatusUpdateTimer(object? sender, EventArgs e)
    {
        if (LastActivityText == null) return;

        if (_ftpClient.IsAuthenticated)
        {
            var timeSinceActivity = DateTime.UtcNow - _ftpClient.LastActivityTime;
            LastActivityText.Text = $"Last activity: {timeSinceActivity.TotalSeconds:F0}s ago";
        }
        else
        {
            LastActivityText.Text = "";
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _statusUpdateTimer?.Stop();
        _ftpClient?.Dispose();
        base.OnClosed(e);
    }
}
