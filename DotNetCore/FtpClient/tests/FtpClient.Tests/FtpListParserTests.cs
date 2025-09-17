using System;
using System.Linq;
using FtpClient.UI.Models;
using FtpClient.UI.Services;
using Shouldly;

namespace FtpClient.Tests;

[TestFixture]
public class FtpListParserTests
{
    [Test]
    public void ParseListResponse_WithEmptyResponse_ReturnsEmptyList()
    {
        var result = FtpListParser.ParseListResponse("", "/");

        result.ShouldBeEmpty();
    }

    [Test]
    public void ParseListResponse_WithWhitespaceOnly_ReturnsEmptyList()
    {
        var result = FtpListParser.ParseListResponse("   \r\n  \n  ", "/");

        result.ShouldBeEmpty();
    }

    [Test]
    public void ParseListResponse_WithUnixFormat_ParsesCorrectly()
    {
        var listResponse = @"drwxr-xr-x   2 user     group        4096 Mar 15 10:30 documents
-rw-r--r--   1 user     group        1024 Mar 14 14:25 readme.txt
lrwxrwxrwx   1 user     group          10 Mar 13 09:15 link -> target";

        var result = FtpListParser.ParseListResponse(listResponse, "/home");

        result.Count.ShouldBe(3);
        
        var directory = result[0];
        directory.Name.ShouldBe("documents");
        directory.FullPath.ShouldBe("/home/documents");
        directory.Type.ShouldBe(FtpFileType.Directory);
        directory.Size.ShouldBe(0); // Directories have size 0
        directory.Owner.ShouldBe("user");
        directory.Group.ShouldBe("group");
        directory.Permissions.ShouldBe("drwxr-xr-x");

        var file = result[1];
        file.Name.ShouldBe("readme.txt");
        file.FullPath.ShouldBe("/home/readme.txt");
        file.Type.ShouldBe(FtpFileType.File);
        file.Size.ShouldBe(1024);

        var link = result[2];
        link.Name.ShouldBe("link -> target");
        link.Type.ShouldBe(FtpFileType.Link);
    }

    [Test]
    public void ParseListResponse_WithDosFormat_ParsesCorrectly()
    {
        var listResponse = @"03-15-25  10:30AM       <DIR>          documents
03-14-25  02:25PM                 1024 readme.txt
03-13-25  09:15AM                 2048 data.bin";

        var result = FtpListParser.ParseListResponse(listResponse, "/");

        result.Count.ShouldBe(3);
        
        var directory = result[0];
        directory.Name.ShouldBe("documents");
        directory.FullPath.ShouldBe("/documents");
        directory.Type.ShouldBe(FtpFileType.Directory);
        directory.Size.ShouldBe(0);

        var file1 = result[1];
        file1.Name.ShouldBe("readme.txt");
        file1.Type.ShouldBe(FtpFileType.File);
        file1.Size.ShouldBe(1024);

        var file2 = result[2];
        file2.Name.ShouldBe("data.bin");
        file2.Type.ShouldBe(FtpFileType.File);
        file2.Size.ShouldBe(2048);
    }

    [Test]
    public void ParseListResponse_WithGenericFormat_ParsesAsUnknown()
    {
        var listResponse = @"file1.txt
file2.doc
folder1";

        var result = FtpListParser.ParseListResponse(listResponse, "/data");

        result.Count.ShouldBe(3);
        result.All(f => f.Type == FtpFileType.Unknown).ShouldBeTrue();
        result[0].Name.ShouldBe("file1.txt");
        result[0].FullPath.ShouldBe("/data/file1.txt");
        result[1].Name.ShouldBe("file2.doc");
        result[2].Name.ShouldBe("folder1");
    }

    [Test]
    public void ParseListResponse_WithMixedFormats_ParsesAllCorrectly()
    {
        var listResponse = @"drwxr-xr-x   2 user     group        4096 Mar 15 10:30 unix_dir
03-14-25  02:25PM                 1024 dos_file.txt
generic_file";

        var result = FtpListParser.ParseListResponse(listResponse, "/");

        result.Count.ShouldBe(3);
        result[0].Type.ShouldBe(FtpFileType.Directory);
        result[0].Name.ShouldBe("unix_dir");
        result[1].Type.ShouldBe(FtpFileType.File);
        result[1].Name.ShouldBe("dos_file.txt");
        result[2].Type.ShouldBe(FtpFileType.Unknown);
        result[2].Name.ShouldBe("generic_file");
    }

    [Test]
    public void ParseListResponse_WithRootPath_CreatesCorrectFullPaths()
    {
        var listResponse = "drwxr-xr-x   2 user     group        4096 Mar 15 10:30 folder";

        var result = FtpListParser.ParseListResponse(listResponse, "/");

        result[0].FullPath.ShouldBe("/folder");
    }

    [Test]
    public void ParseListResponse_WithSubPath_CreatesCorrectFullPaths()
    {
        var listResponse = "drwxr-xr-x   2 user     group        4096 Mar 15 10:30 subfolder";

        var result = FtpListParser.ParseListResponse(listResponse, "/home/user");

        result[0].FullPath.ShouldBe("/home/user/subfolder");
    }

    [Test]
    public void ParseListResponse_WithUnixDateCurrentYear_ParsesCorrectly()
    {
        var currentYear = DateTime.Now.Year;
        var listResponse = $"-rw-r--r--   1 user     group        1024 Mar 15 10:30 file.txt";

        var result = FtpListParser.ParseListResponse(listResponse, "/");

        result[0].ModifiedTime.ShouldNotBeNull();
        result[0].ModifiedTime.Value.Year.ShouldBeLessThanOrEqualTo(currentYear);
        result[0].ModifiedTime.Value.Month.ShouldBe(3);
        result[0].ModifiedTime.Value.Day.ShouldBe(15);
        result[0].ModifiedTime.Value.Hour.ShouldBe(10);
        result[0].ModifiedTime.Value.Minute.ShouldBe(30);
    }

    [Test]
    public void ParseListResponse_WithUnixDateSpecificYear_ParsesCorrectly()
    {
        var listResponse = "-rw-r--r--   1 user     group        1024 Mar 15  2023 file.txt";

        var result = FtpListParser.ParseListResponse(listResponse, "/");

        result[0].ModifiedTime.ShouldNotBeNull();
        result[0].ModifiedTime.Value.Year.ShouldBe(2023);
        result[0].ModifiedTime.Value.Month.ShouldBe(3);
        result[0].ModifiedTime.Value.Day.ShouldBe(15);
    }
}
