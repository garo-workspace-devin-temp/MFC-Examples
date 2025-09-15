using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FtpClient.UI.Models;

namespace FtpClient.UI.Services;

public class FtpClient : IDisposable
{
    private TcpClient? _controlConnection;
    private NetworkStream? _controlStream;
    private StreamReader? _controlReader;
    private StreamWriter? _controlWriter;
    private bool _isConnected = false;
    private LogonInfo? _lastLogonInfo;

    public bool IsConnected => _isConnected && _controlConnection?.Connected == true;
    public LogonInfo? LastLogonInfo => _lastLogonInfo;

    public event Action<string>? CommandSent;
    public event Action<string>? ResponseReceived;

    public async Task<bool> LoginAsync(LogonInfo logonInfo)
    {
        _lastLogonInfo = logonInfo;

        int[,] logonSequences = new int[9, 18] {
            { 0,-2,3,    1,-2, 6,   2,-2,-1,   0, 0, 0,   0, 0, 0,   0, 0, 0 },
            { 3, 6,3,    4, 6,-1,   5,-1, 9,   0,-2,12,   1,-2,15,   2,-2,-1 },
            { 3, 6,3,    4, 6,-1,   6,-2, 9,   1,-2,12,   2,-2,-1,   0, 0, 0 },
            { 7, 3,3,    0,-2, 6,   1,-2, 9,   2,-2,-1,   0, 0, 0,   0, 0, 0 },
            { 3, 6,3,    4, 6,-1,   0,-2, 9,   1,-2,12,   2,-2,-1,   0, 0, 0 },
            { 6,-2,3,    1,-2, 6,   2,-2,-1,   0, 0, 0,   0, 0, 0,   0, 0, 0 },
            { 8, 6,3,    4, 6,-1,   0,-2, 9,   1,-2,12,   2,-2,-1,   0, 0, 0 },
            { 9,-1,3,    1,-2, 6,   2,-2,-1,   0, 0, 0,   0, 0, 0,   0, 0, 0 },
            {10,-2,3,   11,-2, 6,   2,-2,-1,   0, 0, 0,   0, 0, 0,   0, 0, 0 }
        };

        try
        {
            if (IsConnected)
                await LogoutAsync();

            string hostname = logonInfo.UseFirewall ? logonInfo.FirewallHostname : logonInfo.Hostname;
            ushort port = logonInfo.UseFirewall ? logonInfo.FirewallPort : logonInfo.Hostport;

            if (!await OpenControlChannelAsync(hostname, port))
                return false;

            string initialResponse = await GetResponseAsync();
            if (!IsPositiveCompletionReply(initialResponse))
                return false;

            int logonPoint = 0;
            int firewallTypeIndex = (int)logonInfo.FirewallType;
            string hostnamePort = logonInfo.Hostport == 21 ? logonInfo.Hostname : $"{logonInfo.Hostname}:{logonInfo.Hostport}";

            while (true)
            {
                string response;
                int commandIndex = logonSequences[firewallTypeIndex, logonPoint];

                switch (commandIndex)
                {
                    case 0:
                        response = await SendCommandAsync("USER", logonInfo.Username);
                        break;
                    case 1:
                        response = await SendCommandAsync("PASS", logonInfo.Password);
                        break;
                    case 2:
                        response = await SendCommandAsync("ACCT", logonInfo.Account);
                        break;
                    case 3:
                        response = await SendCommandAsync("USER", logonInfo.FirewallUsername);
                        break;
                    case 4:
                        response = await SendCommandAsync("PASS", logonInfo.FirewallPassword);
                        break;
                    case 5:
                        response = await SendCommandAsync("SITE", hostnamePort);
                        break;
                    case 6:
                        response = await SendCommandAsync("USER", $"{logonInfo.Username}@{hostnamePort}");
                        break;
                    case 7:
                        response = await SendCommandAsync("OPEN", hostnamePort);
                        break;
                    case 8:
                        response = await SendCommandAsync("USER", $"{logonInfo.FirewallUsername}@{hostnamePort}");
                        break;
                    case 9:
                        response = await SendCommandAsync("USER", $"{logonInfo.Username}@{hostnamePort} {logonInfo.FirewallUsername}");
                        break;
                    case 10:
                        response = await SendCommandAsync("USER", $"{logonInfo.Username}@{logonInfo.FirewallUsername}@{hostnamePort}");
                        break;
                    case 11:
                        response = await SendCommandAsync("PASS", $"{logonInfo.Password}@{logonInfo.FirewallPassword}");
                        break;
                    default:
                        return false;
                }

                if (!IsPositiveCompletionReply(response) && !IsPositiveIntermediateReply(response))
                    return false;

                int responseCode = GetResponseCode(response);
                int nextIndex = logonPoint + (responseCode / 100) - 1;
                logonPoint = logonSequences[firewallTypeIndex, nextIndex];

                if (logonPoint == -1)
                    return false;
                if (logonPoint == -2)
                {
                    _isConnected = true;
                    return true;
                }
            }
        }
        catch (Exception)
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (IsConnected)
            {
                await SendCommandAsync("QUIT", "");
            }
        }
        catch
        {
        }
        finally
        {
            CloseConnection();
        }
    }

    private async Task<bool> OpenControlChannelAsync(string hostname, ushort port)
    {
        try
        {
            _controlConnection = new TcpClient();
            await _controlConnection.ConnectAsync(hostname, port);
            
            _controlStream = _controlConnection.GetStream();
            _controlReader = new StreamReader(_controlStream, Encoding.ASCII);
            _controlWriter = new StreamWriter(_controlStream, Encoding.ASCII) { AutoFlush = true };
            
            return true;
        }
        catch
        {
            CloseConnection();
            return false;
        }
    }

    private async Task<string> SendCommandAsync(string command, string argument)
    {
        if (_controlWriter == null)
            throw new InvalidOperationException("Not connected");

        string fullCommand = string.IsNullOrEmpty(argument) ? command : $"{command} {argument}";
        CommandSent?.Invoke(fullCommand);
        
        await _controlWriter.WriteLineAsync(fullCommand);
        return await GetResponseAsync();
    }

    private async Task<string> GetResponseAsync()
    {
        if (_controlReader == null)
            throw new InvalidOperationException("Not connected");

        string response = await _controlReader.ReadLineAsync() ?? "";
        ResponseReceived?.Invoke(response);
        return response;
    }

    private static bool IsPositiveCompletionReply(string response)
    {
        return response.Length >= 3 && response[0] == '2';
    }

    private static bool IsPositiveIntermediateReply(string response)
    {
        return response.Length >= 3 && response[0] == '3';
    }

    private static int GetResponseCode(string response)
    {
        if (response.Length >= 3 && int.TryParse(response.Substring(0, 3), out int code))
            return code;
        return 0;
    }

    private void CloseConnection()
    {
        _isConnected = false;
        _controlReader?.Dispose();
        _controlWriter?.Dispose();
        _controlStream?.Dispose();
        _controlConnection?.Dispose();
        
        _controlReader = null;
        _controlWriter = null;
        _controlStream = null;
        _controlConnection = null;
    }

    public void Dispose()
    {
        CloseConnection();
    }
}
