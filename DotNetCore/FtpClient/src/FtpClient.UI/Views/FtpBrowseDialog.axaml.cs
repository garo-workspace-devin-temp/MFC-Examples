using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FtpClient.UI.Models;
using FtpClient.UI.Services;

namespace FtpClient.UI.Views;

public partial class FtpBrowseDialog : Window
{
    private readonly Services.FtpClient _ftpClient;
    private string _currentPath = "/";
    private readonly ObservableCollection<FtpFileViewModel> _files = new();
    private FtpFileViewModel? _selectedFile;

    public bool DialogResult { get; private set; } = false;
    public string SelectedPath { get; private set; } = string.Empty;
    public bool ShowFilesOnly { get; set; } = false;

    public FtpBrowseDialog() : this(new Services.FtpClient())
    {
    }

    public FtpBrowseDialog(Services.FtpClient ftpClient)
    {
        InitializeComponent();
        _ftpClient = ftpClient;
        
        FileListBox.ItemsSource = _files;
        CurrentPathTextBox.Text = _currentPath;
    }

    public async Task<bool> ShowDialogAsync(Window? parent = null, string initialPath = "/")
    {
        _currentPath = initialPath;
        CurrentPathTextBox.Text = _currentPath;
        
        await LoadDirectoryAsync(_currentPath);
        
        if (parent != null)
        {
            try
            {
                await ShowDialog(parent);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("non-visible owner"))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Show();
                
                var tcs = new TaskCompletionSource<bool>();
                Closed += (s, e) => tcs.SetResult(DialogResult);
                await tcs.Task;
            }
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Show();
            
            var tcs = new TaskCompletionSource<bool>();
            Closed += (s, e) => tcs.SetResult(DialogResult);
            await tcs.Task;
        }
        
        return DialogResult;
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        await LoadDirectoryAsync(_currentPath);
    }

    private async void UpButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentPath != "/")
        {
            try
            {
                bool success = await _ftpClient.ChangeToParentDirectoryAsync();
                if (success)
                {
                    var parentPath = GetParentPath(_currentPath);
                    await LoadDirectoryAsync(parentPath);
                }
                else
                {
                    await ShowErrorAsync("Failed to navigate to parent directory");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Navigation error: {ex.Message}");
            }
        }
    }

    private void FileListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (FileListBox.SelectedItem is FtpFileViewModel selectedFile)
        {
            _selectedFile = selectedFile;
            
            if (selectedFile.FileInfo.IsDirectory)
            {
                if (selectedFile.Name == "..")
                {
                    SelectedPathTextBox.Text = GetParentPath(_currentPath);
                    OkButton.IsEnabled = !ShowFilesOnly;
                    NavigateButton.IsEnabled = true;
                }
                else
                {
                    var newPath = CombinePath(_currentPath, selectedFile.Name);
                    SelectedPathTextBox.Text = newPath;
                    OkButton.IsEnabled = !ShowFilesOnly;
                    NavigateButton.IsEnabled = true;
                }
            }
            else
            {
                SelectedPathTextBox.Text = selectedFile.FileInfo.FullPath;
                OkButton.IsEnabled = true;
                NavigateButton.IsEnabled = false;
            }
        }
        else
        {
            _selectedFile = null;
            SelectedPathTextBox.Text = "";
            OkButton.IsEnabled = false;
            NavigateButton.IsEnabled = false;
        }
    }

    private async void FileListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (_selectedFile?.FileInfo.IsDirectory == true)
        {
            await NavigateToDirectoryAsync(_selectedFile);
        }
    }

    private async Task NavigateToDirectoryAsync(FtpFileViewModel directoryItem)
    {
        try
        {
            string newPath;
            bool success;

            if (directoryItem.Name == "..")
            {
                success = await _ftpClient.ChangeToParentDirectoryAsync();
                newPath = GetParentPath(_currentPath);
            }
            else
            {
                var targetPath = CombinePath(_currentPath, directoryItem.Name);
                success = await _ftpClient.ChangeWorkingDirectoryAsync(targetPath);
                newPath = targetPath;
            }

            if (success)
            {
                await LoadDirectoryAsync(newPath);
            }
            else
            {
                await ShowErrorAsync($"Failed to navigate to directory: {directoryItem.Name}");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Navigation error: {ex.Message}");
        }
    }

    private async Task LoadDirectoryAsync(string path)
    {
        try
        {
            if (!_ftpClient.IsAuthenticated)
            {
                await ShowErrorAsync("Not connected to FTP server");
                return;
            }

            _currentPath = path;
            CurrentPathTextBox.Text = _currentPath;
            
            var files = await _ftpClient.ListDirectoryAsync(path);
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _files.Clear();
                
                if (path != "/")
                {
                    var parentDir = new FtpFileInfo("..", GetParentPath(path), FtpFileType.Directory);
                    _files.Add(new FtpFileViewModel(parentDir));
                }
                
                var sortedFiles = files
                    .Where(f => !ShowFilesOnly || f.IsFile || f.IsDirectory)
                    .OrderBy(f => f.IsDirectory ? 0 : 1)
                    .ThenBy(f => f.Name);
                
                foreach (var file in sortedFiles)
                {
                    _files.Add(new FtpFileViewModel(file));
                }
            });
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Failed to load directory: {ex.Message}");
        }
    }

    private async Task ShowErrorAsync(string message)
    {
        var errorDialog = new Window
        {
            Title = "Error",
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var stackPanel = new StackPanel { Margin = new Avalonia.Thickness(20) };
        stackPanel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        
        var okButton = new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 20, 0, 0) };
        okButton.Click += (s, e) => errorDialog.Close();
        stackPanel.Children.Add(okButton);
        
        errorDialog.Content = stackPanel;
        
        try
        {
            await errorDialog.ShowDialog(this);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("non-visible owner"))
        {
            errorDialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            errorDialog.Show();
            
            var tcs = new TaskCompletionSource<bool>();
            errorDialog.Closed += (s, e) => tcs.SetResult(true);
            await tcs.Task;
        }
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedFile != null)
        {
            SelectedPath = _selectedFile.FileInfo.FullPath;
        }
        else
        {
            SelectedPath = _currentPath;
        }
        
        DialogResult = true;
        Close();
    }

    private async void NavigateButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedFile?.FileInfo.IsDirectory == true)
        {
            await NavigateToDirectoryAsync(_selectedFile);
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static string GetParentPath(string path)
    {
        if (path == "/" || string.IsNullOrEmpty(path))
            return "/";
        
        var trimmed = path.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        
        if (lastSlash <= 0)
            return "/";
        
        return trimmed.Substring(0, lastSlash);
    }

    private static string CombinePath(string basePath, string name)
    {
        if (string.IsNullOrEmpty(basePath) || basePath == "/")
            return "/" + name;
        
        return basePath.TrimEnd('/') + "/" + name;
    }
}
