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
