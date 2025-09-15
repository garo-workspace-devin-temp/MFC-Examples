using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FtpClient.UI.Models;

namespace FtpClient.UI.Services;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Authenticating,
    Authenticated,
    Disconnecting,
    Error
}

public class ConnectionStateChangedEventArgs : EventArgs
{
    public ConnectionState OldState { get; }
    public ConnectionState NewState { get; }
    public string? Message { get; }
    public Exception? Exception { get; }

    public ConnectionStateChangedEventArgs(ConnectionState oldState, ConnectionState newState, string? message = null, Exception? exception = null)
    {
        OldState = oldState;
        NewState = newState;
        Message = message;
        Exception = exception;
    }
}

public class FtpClient : IDisposable
{
    private TcpClient? _controlConnection;
    private NetworkStream? _controlStream;
    private StreamReader? _controlReader;
    private StreamWriter? _controlWriter;
    private ConnectionState _connectionState = ConnectionState.Disconnected;
    private LogonInfo? _lastLogonInfo;
    private DateTime _lastActivityTime = DateTime.UtcNow;
    private readonly Timer _keepAliveTimer;
    private readonly object _stateLock = new object();

    public ConnectionState ConnectionState 
    { 
        get 
        { 
            lock (_stateLock) 
            { 
                return _connectionState; 
            } 
        } 
        private set 
        { 
            ConnectionState oldState;
            lock (_stateLock) 
            { 
                oldState = _connectionState;
                _connectionState = value;
            }
            if (oldState != value)
            {
                ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(oldState, value));
            }
        } 
    }

    public bool IsConnected => ConnectionState == ConnectionState.Connected || ConnectionState == ConnectionState.Authenticated;
    public bool IsAuthenticated => ConnectionState == ConnectionState.Authenticated;
    public LogonInfo? LastLogonInfo => _lastLogonInfo;
    public DateTime LastActivityTime => _lastActivityTime;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromMinutes(5);

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
    public event Action<string>? CommandSent;
    public event Action<string>? ResponseReceived;

    public FtpClient()
    {
        _keepAliveTimer = new Timer(OnKeepAliveTimer, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task<bool> LoginAsync(LogonInfo logonInfo)
    {
        try
        {
            ConnectionState = ConnectionState.Connecting;
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

            if (IsConnected)
                await LogoutAsync();

            string hostname = logonInfo.UseFirewall ? logonInfo.FirewallHostname : logonInfo.Hostname;
            ushort port = logonInfo.UseFirewall ? logonInfo.FirewallPort : logonInfo.Hostport;

            if (!await OpenControlChannelAsync(hostname, port))
            {
                ConnectionState = ConnectionState.Error;
                return false;
            }

            ConnectionState = ConnectionState.Connected;

            string initialResponse = await GetResponseAsync();
            if (!IsPositiveCompletionReply(initialResponse))
            {
                ConnectionState = ConnectionState.Error;
                return false;
            }

            ConnectionState = ConnectionState.Authenticating;

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
                        ConnectionState = ConnectionState.Error;
                        return false;
                }

                if (!IsPositiveCompletionReply(response) && !IsPositiveIntermediateReply(response))
                {
                    ConnectionState = ConnectionState.Error;
                    return false;
                }

                int responseCode = GetResponseCode(response);
                int nextIndex = logonPoint + (responseCode / 100) - 1;
                logonPoint = logonSequences[firewallTypeIndex, nextIndex];

                if (logonPoint == -1)
                {
                    ConnectionState = ConnectionState.Error;
                    return false;
                }
                if (logonPoint == -2)
                {
                    ConnectionState = ConnectionState.Authenticated;
                    StartKeepAliveTimer();
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            ConnectionState = ConnectionState.Error;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Error, ConnectionState.Error, "Login failed", ex));
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            ConnectionState = ConnectionState.Disconnecting;
            StopKeepAliveTimer();

            if (IsConnected && _controlWriter != null)
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
            ConnectionState = ConnectionState.Disconnected;
        }
    }

    private async Task<bool> OpenControlChannelAsync(string hostname, ushort port)
    {
        try
        {
            _controlConnection = new TcpClient();
            
            using var cts = new CancellationTokenSource(ConnectionTimeout);
            await _controlConnection.ConnectAsync(hostname, port, cts.Token);
            
            _controlStream = _controlConnection.GetStream();
            _controlReader = new StreamReader(_controlStream, Encoding.ASCII);
            _controlWriter = new StreamWriter(_controlStream, Encoding.ASCII) { AutoFlush = true };
            
            UpdateLastActivityTime();
            return true;
        }
        catch (Exception ex)
        {
            ConnectionState = ConnectionState.Error;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Connecting, ConnectionState.Error, $"Failed to connect to {hostname}:{port}", ex));
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
        UpdateLastActivityTime();
        return await GetResponseAsync();
    }

    private async Task<string> GetResponseAsync()
    {
        if (_controlReader == null)
            throw new InvalidOperationException("Not connected");

        string response = await _controlReader.ReadLineAsync() ?? "";
        ResponseReceived?.Invoke(response);
        UpdateLastActivityTime();
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
        StopKeepAliveTimer();
        
        _controlReader?.Dispose();
        _controlWriter?.Dispose();
        _controlStream?.Dispose();
        _controlConnection?.Dispose();
        
        _controlReader = null;
        _controlWriter = null;
        _controlStream = null;
        _controlConnection = null;
    }

    private void UpdateLastActivityTime()
    {
        _lastActivityTime = DateTime.UtcNow;
    }

    private void StartKeepAliveTimer()
    {
        if (KeepAliveInterval > TimeSpan.Zero)
        {
            _keepAliveTimer.Change(KeepAliveInterval, KeepAliveInterval);
        }
    }

    private void StopKeepAliveTimer()
    {
        _keepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private async void OnKeepAliveTimer(object? state)
    {
        try
        {
            if (IsAuthenticated && DateTime.UtcNow - _lastActivityTime > KeepAliveInterval)
            {
                await SendCommandAsync("NOOP", "");
            }
        }
        catch (Exception ex)
        {
            ConnectionState = ConnectionState.Error;
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(ConnectionState.Authenticated, ConnectionState.Error, "Keep-alive failed", ex));
        }
    }

    public async Task<bool> ReconnectAsync()
    {
        if (_lastLogonInfo == null)
            return false;

        await LogoutAsync();
        return await LoginAsync(_lastLogonInfo);
    }

    public void Dispose()
    {
        _keepAliveTimer?.Dispose();
        CloseConnection();
    }
}
