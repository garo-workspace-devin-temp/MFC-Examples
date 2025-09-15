using System.Windows;

namespace FtpClient.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Browse FTP Files functionality will be implemented in future iterations.", 
                       "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Connection Settings functionality will be implemented in future iterations.", 
                       "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
