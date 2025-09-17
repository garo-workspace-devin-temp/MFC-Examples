using FtpClient.UI.Models;
using Shouldly;

namespace FtpClient.Tests;

[TestFixture]
public class LogonInfoTests
{
    [Test]
    public void Constructor_WithDefaults_SetsCorrectValues()
    {
        var logonInfo = new LogonInfo();

        logonInfo.Hostname.ShouldBe(string.Empty);
        logonInfo.Hostport.ShouldBe(LogonInfo.DefaultFtpPort);
        logonInfo.Username.ShouldBe(LogonInfo.AnonymousUser);
        logonInfo.Password.ShouldBe(LogonInfo.AnonymousPassword);
        logonInfo.Account.ShouldBe(string.Empty);
        logonInfo.PassiveMode.ShouldBeTrue();
        logonInfo.UseFirewall.ShouldBeFalse();
        logonInfo.FirewallType.ShouldBe(FirewallType.None);
    }

    [Test]
    public void Constructor_WithParameters_SetsCorrectValues()
    {
        var logonInfo = new LogonInfo("ftp.example.com", 2121, "testuser", "testpass", "testaccount");

        logonInfo.Hostname.ShouldBe("ftp.example.com");
        logonInfo.Hostport.ShouldBe((ushort)2121);
        logonInfo.Username.ShouldBe("testuser");
        logonInfo.Password.ShouldBe("testpass");
        logonInfo.Account.ShouldBe("testaccount");
    }

    [Test]
    public void SetFirewall_WithValidParameters_ConfiguresFirewall()
    {
        var logonInfo = new LogonInfo();

        logonInfo.SetFirewall("firewall.example.com", "fwuser", "fwpass", 8080, FirewallType.SiteHostname);

        logonInfo.UseFirewall.ShouldBeTrue();
        logonInfo.FirewallHostname.ShouldBe("firewall.example.com");
        logonInfo.FirewallUsername.ShouldBe("fwuser");
        logonInfo.FirewallPassword.ShouldBe("fwpass");
        logonInfo.FirewallPort.ShouldBe((ushort)8080);
        logonInfo.FirewallType.ShouldBe(FirewallType.SiteHostname);
    }

    [Test]
    public void DisableFirewall_WhenCalled_DisablesFirewall()
    {
        var logonInfo = new LogonInfo();
        logonInfo.SetFirewall("firewall.example.com", "fwuser", "fwpass", 8080, FirewallType.SiteHostname);

        logonInfo.DisableFirewall();

        logonInfo.UseFirewall.ShouldBeFalse();
        logonInfo.FirewallType.ShouldBe(FirewallType.None);
    }

    [Test]
    public void SetFirewall_WithNoneType_DisablesFirewall()
    {
        var logonInfo = new LogonInfo();

        logonInfo.SetFirewall("firewall.example.com", "fwuser", "fwpass", 8080, FirewallType.None);

        logonInfo.UseFirewall.ShouldBeFalse();
        logonInfo.FirewallType.ShouldBe(FirewallType.None);
    }

    [TestCase(FirewallType.SiteHostname)]
    [TestCase(FirewallType.UserAfterLogon)]
    [TestCase(FirewallType.ProxyOpen)]
    [TestCase(FirewallType.Transparent)]
    [TestCase(FirewallType.UserWithNoLogon)]
    [TestCase(FirewallType.UserFirewallIdAtRemotehost)]
    [TestCase(FirewallType.UserRemoteIdAtRemotehostFirewallId)]
    [TestCase(FirewallType.UserRemoteIdAtFirewallIdAtRemotehost)]
    public void SetFirewall_WithNonNoneType_EnablesFirewall(FirewallType firewallType)
    {
        var logonInfo = new LogonInfo();

        logonInfo.SetFirewall("firewall.example.com", "fwuser", "fwpass", 8080, firewallType);

        logonInfo.UseFirewall.ShouldBeTrue();
        logonInfo.FirewallType.ShouldBe(firewallType);
    }
}
