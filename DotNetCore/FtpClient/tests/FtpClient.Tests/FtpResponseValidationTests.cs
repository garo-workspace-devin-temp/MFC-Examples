using System.Reflection;
using FtpClient.UI.Services;
using Shouldly;

namespace FtpClient.Tests;

[TestFixture]
public class FtpResponseValidationTests
{
    private MethodInfo _isPositiveCompletionReplyMethod;
    private MethodInfo _isPositiveIntermediateReplyMethod;
    private MethodInfo _isPositivePreliminaryReplyMethod;
    private MethodInfo _getResponseCodeMethod;

    [SetUp]
    public void SetUp()
    {
        var ftpClientType = typeof(FtpClient.UI.Services.FtpClient);
        _isPositiveCompletionReplyMethod = ftpClientType.GetMethod("IsPositiveCompletionReply", BindingFlags.NonPublic | BindingFlags.Static);
        _isPositiveIntermediateReplyMethod = ftpClientType.GetMethod("IsPositiveIntermediateReply", BindingFlags.NonPublic | BindingFlags.Static);
        _isPositivePreliminaryReplyMethod = ftpClientType.GetMethod("IsPositivePreliminaryReply", BindingFlags.NonPublic | BindingFlags.Static);
        _getResponseCodeMethod = ftpClientType.GetMethod("GetResponseCode", BindingFlags.NonPublic | BindingFlags.Static);
    }

    [TestCase("200 Command okay", true)]
    [TestCase("220 Service ready", true)]
    [TestCase("250 Requested file action okay", true)]
    [TestCase("299 Custom success", true)]
    [TestCase("150 File status okay", false)]
    [TestCase("300 Need account", false)]
    [TestCase("400 Bad request", false)]
    [TestCase("500 Syntax error", false)]
    [TestCase("", false)]
    [TestCase("20", false)]
    [TestCase("abc", false)]
    public void IsPositiveCompletionReply_WithVariousResponses_ReturnsExpectedResult(string response, bool expected)
    {
        var result = (bool)_isPositiveCompletionReplyMethod.Invoke(null, new object[] { response });

        result.ShouldBe(expected);
    }

    [TestCase("300 Need account", true)]
    [TestCase("331 User name okay, need password", true)]
    [TestCase("350 Requested file action pending", true)]
    [TestCase("399 Custom intermediate", true)]
    [TestCase("200 Command okay", false)]
    [TestCase("150 File status okay", false)]
    [TestCase("400 Bad request", false)]
    [TestCase("500 Syntax error", false)]
    [TestCase("", false)]
    [TestCase("30", false)]
    public void IsPositiveIntermediateReply_WithVariousResponses_ReturnsExpectedResult(string response, bool expected)
    {
        var result = (bool)_isPositiveIntermediateReplyMethod.Invoke(null, new object[] { response });

        result.ShouldBe(expected);
    }

    [TestCase("100 Continue", true)]
    [TestCase("150 Here comes the directory listing", true)]
    [TestCase("125 Data connection already open", true)]
    [TestCase("199 Custom preliminary", true)]
    [TestCase("200 Command okay", false)]
    [TestCase("300 Need account", false)]
    [TestCase("400 Bad request", false)]
    [TestCase("500 Syntax error", false)]
    [TestCase("", false)]
    [TestCase("10", false)]
    public void IsPositivePreliminaryReply_WithVariousResponses_ReturnsExpectedResult(string response, bool expected)
    {
        var result = (bool)_isPositivePreliminaryReplyMethod.Invoke(null, new object[] { response });

        result.ShouldBe(expected);
    }

    [TestCase("200 Command okay", 200)]
    [TestCase("150 Here comes the directory listing", 150)]
    [TestCase("331 User name okay, need password", 331)]
    [TestCase("500 Syntax error", 500)]
    [TestCase("", 0)]
    [TestCase("20", 0)]
    [TestCase("abc", 0)]
    [TestCase("200", 200)]
    [TestCase("1234 Long code", 123)]
    public void GetResponseCode_WithVariousResponses_ReturnsExpectedCode(string response, int expected)
    {
        var result = (int)_getResponseCodeMethod.Invoke(null, new object[] { response });

        result.ShouldBe(expected);
    }
}
