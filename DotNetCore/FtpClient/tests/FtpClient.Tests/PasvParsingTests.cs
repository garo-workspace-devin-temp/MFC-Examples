using System;
using System.Reflection;
using FtpClient.UI.Services;
using Shouldly;

namespace FtpClient.Tests;

[TestFixture]
public class PasvParsingTests
{
    private MethodInfo _parsePasvResponseMethod;

    [SetUp]
    public void SetUp()
    {
        var ftpClientType = typeof(FtpClient.UI.Services.FtpClient);
        _parsePasvResponseMethod = ftpClientType.GetMethod("ParsePasvResponse", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [Test]
    public void ParsePasvResponse_WithValidResponse_ReturnsCorrectHostAndPort()
    {
        var ftpClient = new FtpClient.UI.Services.FtpClient();
        var response = "227 Entering Passive Mode (192,168,1,100,20,21).";

        var result = _parsePasvResponseMethod.Invoke(ftpClient, new object[] { response });
        var (host, port) = ((string, int))result;

        host.ShouldBe("192.168.1.100");
        port.ShouldBe(5141); // (20 * 256) + 21 = 5141
    }

    [Test]
    public void ParsePasvResponse_WithDifferentFormat_ReturnsCorrectHostAndPort()
    {
        var ftpClient = new FtpClient.UI.Services.FtpClient();
        var response = "227 Entering Passive Mode (10,0,0,1,4,78)";

        var result = _parsePasvResponseMethod.Invoke(ftpClient, new object[] { response });
        var (host, port) = ((string, int))result;

        host.ShouldBe("10.0.0.1");
        port.ShouldBe(1102); // (4 * 256) + 78 = 1102
    }

    [Test]
    public void ParsePasvResponse_WithHighPortNumbers_ReturnsCorrectPort()
    {
        var ftpClient = new FtpClient.UI.Services.FtpClient();
        var response = "227 Entering Passive Mode (127,0,0,1,255,255).";

        var result = _parsePasvResponseMethod.Invoke(ftpClient, new object[] { response });
        var (host, port) = ((string, int))result;

        host.ShouldBe("127.0.0.1");
        port.ShouldBe(65535); // (255 * 256) + 255 = 65535
    }

    [TestCase("227 Invalid format")]
    [TestCase("227 Entering Passive Mode")]
    [TestCase("227 Entering Passive Mode (192,168,1)")]
    [TestCase("227 Entering Passive Mode (192,168,1,100,20)")]
    [TestCase("227 Entering Passive Mode (a,b,c,d,e,f)")]
    [TestCase("")]
    public void ParsePasvResponse_WithInvalidResponse_ThrowsException(string response)
    {
        var ftpClient = new FtpClient.UI.Services.FtpClient();

        Should.Throw<Exception>(() => _parsePasvResponseMethod.Invoke(ftpClient, new object[] { response }));
    }
}
