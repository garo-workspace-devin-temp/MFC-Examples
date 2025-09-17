using FtpClient.UI.Models;

namespace FtpClient.UI.Views;

public class FtpFileViewModel
{
    public FtpFileInfo FileInfo { get; }
    
    public string Name => FileInfo.Name;
    public string TypeIcon => FileInfo.Type switch
    {
        FtpFileType.Directory => "📁",
        FtpFileType.File => "📄",
        FtpFileType.Link => "🔗",
        _ => "❓"
    };
    public string SizeText => FileInfo.IsDirectory ? "<DIR>" : FormatFileSize(FileInfo.Size);
    public string ModifiedText => FileInfo.ModifiedTime?.ToString("MM/dd/yyyy HH:mm") ?? "";

    public FtpFileViewModel(FtpFileInfo fileInfo)
    {
        FileInfo = fileInfo;
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;
        
        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }
        
        return $"{size:F1} {suffixes[suffixIndex]}";
    }
}
