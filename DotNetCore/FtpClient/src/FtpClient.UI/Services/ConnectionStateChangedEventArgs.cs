using System;

namespace FtpClient.UI.Services;

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
