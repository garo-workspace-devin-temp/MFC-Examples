using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace FtpClient.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += "\nBrowse FTP Files functionality will be implemented in future iterations.";
        }
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ProtocolOutput != null)
        {
            ProtocolOutput.Text += "\nConnection Settings functionality will be implemented in future iterations.";
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}
