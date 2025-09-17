using FtpClient.UI.Models;
using Shouldly;

namespace FtpClient.Tests;

[TestFixture]
public class FtpFileInfoTests
{
    [Test]
    public void Constructor_WithDefaults_SetsCorrectValues()
    {
        var fileInfo = new FtpFileInfo();

        fileInfo.Name.ShouldBe(string.Empty);
        fileInfo.FullPath.ShouldBe(string.Empty);
        fileInfo.Type.ShouldBe(FtpFileType.Unknown);
        fileInfo.Size.ShouldBe(0);
        fileInfo.ModifiedTime.ShouldBeNull();
        fileInfo.Permissions.ShouldBe(string.Empty);
        fileInfo.Owner.ShouldBe(string.Empty);
        fileInfo.Group.ShouldBe(string.Empty);
        fileInfo.RawListing.ShouldBe(string.Empty);
    }

    [Test]
    public void Constructor_WithParameters_SetsCorrectValues()
    {
        var fileInfo = new FtpFileInfo("test.txt", "/home/test.txt", FtpFileType.File);

        fileInfo.Name.ShouldBe("test.txt");
        fileInfo.FullPath.ShouldBe("/home/test.txt");
        fileInfo.Type.ShouldBe(FtpFileType.File);
    }

    [Test]
    public void IsDirectory_WhenTypeIsDirectory_ReturnsTrue()
    {
        var fileInfo = new FtpFileInfo("folder", "/home/folder", FtpFileType.Directory);

        fileInfo.IsDirectory.ShouldBeTrue();
        fileInfo.IsFile.ShouldBeFalse();
        fileInfo.IsLink.ShouldBeFalse();
    }

    [Test]
    public void IsFile_WhenTypeIsFile_ReturnsTrue()
    {
        var fileInfo = new FtpFileInfo("file.txt", "/home/file.txt", FtpFileType.File);

        fileInfo.IsFile.ShouldBeTrue();
        fileInfo.IsDirectory.ShouldBeFalse();
        fileInfo.IsLink.ShouldBeFalse();
    }

    [Test]
    public void IsLink_WhenTypeIsLink_ReturnsTrue()
    {
        var fileInfo = new FtpFileInfo("link", "/home/link", FtpFileType.Link);

        fileInfo.IsLink.ShouldBeTrue();
        fileInfo.IsDirectory.ShouldBeFalse();
        fileInfo.IsFile.ShouldBeFalse();
    }

    [Test]
    public void ToString_ReturnsNameAndType()
    {
        var fileInfo = new FtpFileInfo("test.txt", "/home/test.txt", FtpFileType.File);

        var result = fileInfo.ToString();

        result.ShouldBe("test.txt (File)");
    }
}
