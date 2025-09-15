using System;

namespace FtpClient.UI.Models;

public enum FtpFileType
{
    File,
    Directory,
    Link,
    Unknown
}

public class FtpFileInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public FtpFileType Type { get; set; } = FtpFileType.Unknown;
    public long Size { get; set; } = 0;
    public DateTime? ModifiedTime { get; set; }
    public string Permissions { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string RawListing { get; set; } = string.Empty;

    public bool IsDirectory => Type == FtpFileType.Directory;
    public bool IsFile => Type == FtpFileType.File;
    public bool IsLink => Type == FtpFileType.Link;

    public FtpFileInfo()
    {
    }

    public FtpFileInfo(string name, string fullPath, FtpFileType type = FtpFileType.Unknown)
    {
        Name = name;
        FullPath = fullPath;
        Type = type;
    }

    public override string ToString()
    {
        return $"{Name} ({Type})";
    }
}
