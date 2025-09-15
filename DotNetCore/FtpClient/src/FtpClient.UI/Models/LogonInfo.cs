using System;

namespace FtpClient.UI.Models;

public class LogonInfo
{
    public const ushort DefaultFtpPort = 21;
    public const string AnonymousUser = "anonymous";
    public const string AnonymousPassword = "anonymous@user.com";

    public string Hostname { get; set; } = string.Empty;
    public ushort Hostport { get; set; } = DefaultFtpPort;
    public string Username { get; set; } = AnonymousUser;
    public string Password { get; set; } = AnonymousPassword;
    public string Account { get; set; } = string.Empty;
    public bool PassiveMode { get; set; } = true;
    
    public bool UseFirewall { get; set; } = false;
    public string FirewallHostname { get; set; } = string.Empty;
    public ushort FirewallPort { get; set; } = DefaultFtpPort;
    public string FirewallUsername { get; set; } = string.Empty;
    public string FirewallPassword { get; set; } = string.Empty;
    public FirewallType FirewallType { get; set; } = FirewallType.None;

    public LogonInfo()
    {
    }

    public LogonInfo(string hostname, ushort hostport = DefaultFtpPort, 
                     string username = AnonymousUser, string password = AnonymousPassword, 
                     string account = "")
    {
        Hostname = hostname;
        Hostport = hostport;
        Username = username;
        Password = password;
        Account = account;
    }

    public void SetFirewall(string firewallHostname, string firewallUsername, 
                           string firewallPassword, ushort firewallPort, FirewallType firewallType)
    {
        FirewallHostname = firewallHostname;
        FirewallUsername = firewallUsername;
        FirewallPassword = firewallPassword;
        FirewallPort = firewallPort;
        FirewallType = firewallType;
        UseFirewall = firewallType != FirewallType.None;
    }

    public void DisableFirewall()
    {
        UseFirewall = false;
        FirewallType = FirewallType.None;
    }
}
