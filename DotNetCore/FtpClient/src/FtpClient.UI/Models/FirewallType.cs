namespace FtpClient.UI.Models;

public enum FirewallType
{
    None = 0,
    SiteHostname = 1,
    UserAfterLogon = 2,
    ProxyOpen = 3,
    Transparent = 4,
    UserWithNoLogon = 5,
    UserFirewallIdAtRemotehost = 6,
    UserRemoteIdAtRemotehostFirewallId = 7,
    UserRemoteIdAtFirewallIdAtRemotehost = 8
}
