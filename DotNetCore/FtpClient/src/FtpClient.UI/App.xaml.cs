using System.Windows;

namespace FtpClient.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = new Views.MainWindow();
        mainWindow.Show();
        
        base.OnStartup(e);
    }
}
