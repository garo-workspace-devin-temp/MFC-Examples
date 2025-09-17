using System;
using System.Threading.Tasks;
using FtpClient.UI.Models;
using FtpClient.UI.Services;
using Shouldly;

namespace FtpClient.Tests;

[TestFixture]
public class ConnectionStateTests
{
    private FtpClient.UI.Services.FtpClient _ftpClient;
    private ConnectionStateChangedEventArgs _lastStateChangeEvent;

    [SetUp]
    public void SetUp()
    {
        _ftpClient = new FtpClient.UI.Services.FtpClient();
        _lastStateChangeEvent = null;
        _ftpClient.ConnectionStateChanged += (sender, args) => _lastStateChangeEvent = args;
    }

    [TearDown]
    public void TearDown()
    {
        _ftpClient?.Dispose();
    }

    [Test]
    public void Constructor_InitializesWithDisconnectedState()
    {
        _ftpClient.ConnectionState.ShouldBe(ConnectionState.Disconnected);
        _ftpClient.IsConnected.ShouldBeFalse();
        _ftpClient.IsAuthenticated.ShouldBeFalse();
        _ftpClient.LastLogonInfo.ShouldBeNull();
    }

    [Test]
    public void ConnectionTimeout_DefaultValue_IsThirtySeconds()
    {
        _ftpClient.ConnectionTimeout.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Test]
    public void KeepAliveInterval_DefaultValue_IsFiveMinutes()
    {
        _ftpClient.KeepAliveInterval.ShouldBe(TimeSpan.FromMinutes(5));
    }

    [Test]
    public void ConnectionTimeout_CanBeSet()
    {
        _ftpClient.ConnectionTimeout = TimeSpan.FromSeconds(60);

        _ftpClient.ConnectionTimeout.ShouldBe(TimeSpan.FromSeconds(60));
    }

    [Test]
    public void KeepAliveInterval_CanBeSet()
    {
        _ftpClient.KeepAliveInterval = TimeSpan.FromMinutes(10);

        _ftpClient.KeepAliveInterval.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Test]
    public void IsConnected_WhenStateIsConnected_ReturnsTrue()
    {
        var connectionStateProperty = typeof(FtpClient.UI.Services.FtpClient)
            .GetProperty("ConnectionState", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        connectionStateProperty.SetValue(_ftpClient, ConnectionState.Connected);

        _ftpClient.IsConnected.ShouldBeTrue();
        _ftpClient.IsAuthenticated.ShouldBeFalse();
    }

    [Test]
    public void IsAuthenticated_WhenStateIsAuthenticated_ReturnsTrue()
    {
        var connectionStateProperty = typeof(FtpClient.UI.Services.FtpClient)
            .GetProperty("ConnectionState", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        connectionStateProperty.SetValue(_ftpClient, ConnectionState.Authenticated);

        _ftpClient.IsConnected.ShouldBeTrue();
        _ftpClient.IsAuthenticated.ShouldBeTrue();
    }

    [Test]
    public async Task LogoutAsync_WhenCalled_SetsStateToDisconnected()
    {
        await _ftpClient.LogoutAsync();

        _ftpClient.ConnectionState.ShouldBe(ConnectionState.Disconnected);
    }

    [Test]
    public async Task ReconnectAsync_WithoutPreviousLogin_ReturnsFalse()
    {
        var result = await _ftpClient.ReconnectAsync();

        result.ShouldBeFalse();
    }
}
