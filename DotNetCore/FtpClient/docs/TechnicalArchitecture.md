# FTP Client Technical Architecture

## Executive Summary

The .NET Core migration of the FtpClient application requires a comprehensive architectural transformation from synchronous C++ patterns to modern asynchronous C# implementations. This document defines the technical architecture using .NET 9, async/await networking, dependency injection, and MVVM patterns while maintaining full compatibility with the existing FTP protocol implementation and feature set.

## Current C++ Architecture Analysis

### Core Components Overview

#### Socket Layer Architecture
**Current Implementation**: `BlockingSocket.h` (lines 183-215)
```cpp
class CBlockingSocket : public IBlockingSocket
{
    int Send(const char* pch, int nSize, int nSecs) const;
    int Receive(char* pch, int nSize, int nSecs) const;
    void Connect(LPCSOCKADDR psa) const;
    SOCKET m_hSocket;
};
```

**Analysis**: 
- Synchronous blocking operations with timeout handling
- Platform abstraction for Windows/Linux socket differences
- Manual timeout management using select() calls
- Exception-based error handling

#### FTP Protocol Engine
**Current Implementation**: `CFTPClient` class (lines 61-191)
```cpp
class CFTPClient
{
    bool Login(const CLogonInfo& loginInfo);
    bool DownloadFile(const tstring& strRemoteFile, const tstring& strLocalFile);
    bool UploadFile(const tstring& strLocalFile, const tstring& strRemoteFile);
    std::auto_ptr<IBlockingSocket> m_apSckControlConnection;
};
```

**Analysis**:
- State machine-based protocol implementation
- Observer pattern for notifications
- Smart pointer memory management
- Comprehensive command/response handling

#### Observer Pattern Implementation
**Current Implementation**: `CNotification` class (lines 197-215)
```cpp
class CFTPClient::CNotification
{
    virtual void OnBytesReceived(const TByteVector& vBuffer, long lReceivedBytes) {}
    virtual void OnPreReceiveFile(const tstring& strSourceFile, const tstring& strTargetFile, long lFileSize) {}
    virtual void OnSendCommand(const CCommand& Command, const CArg& Arguments) {}
};
```

**Analysis**:
- Virtual method-based callbacks
- Manual observer registration/deregistration
- Type-unsafe notification parameters

## .NET Core Target Architecture

### Architectural Principles

#### 1. Async-First Design
All I/O operations use async/await patterns to prevent UI blocking and improve scalability.

#### 2. Dependency Injection
Built-in Microsoft.Extensions.DependencyInjection for loose coupling and testability.

#### 3. SOLID Principles
- Single Responsibility: Each class has one clear purpose
- Open/Closed: Extensible through interfaces
- Liskov Substitution: Proper inheritance hierarchies
- Interface Segregation: Focused, minimal interfaces
- Dependency Inversion: Depend on abstractions

#### 4. Cross-Platform Compatibility
Leverage .NET Core's unified platform abstraction.

### Core Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   Main Window   │  │ Settings Dialog │  │File Browser │ │
│  │   (WPF/MVVM)    │  │   (WPF/MVVM)    │  │(WPF/MVVM)   │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   ViewModels    │  │    Commands     │  │   Services  │ │
│  │   (MVVM)        │  │  (ICommand)     │  │ (Business)  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   FTP Client    │  │  Transfer Mgr   │  │ Directory   │ │
│  │   (Core Logic)  │  │  (Operations)   │  │  Service    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │  Socket Layer   │  │   File System   │  │Configuration│ │
│  │  (Networking)   │  │   (I/O)         │  │  (Settings) │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Socket Layer Migration

### Interface Design

```csharp
public interface IFtpSocket : IDisposable
{
    // Connection Management
    Task ConnectAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    bool IsConnected { get; }
    
    // Data Transfer
    Task<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
    Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    
    // Stream Operations
    Task<Stream> GetDataStreamAsync(IPEndPoint dataEndpoint, CancellationToken cancellationToken = default);
    
    // Configuration
    TimeSpan Timeout { get; set; }
    int BufferSize { get; set; }
}
```

### Implementation Strategy

```csharp
public class FtpSocket : IFtpSocket
{
    private readonly Socket _socket;
    private readonly ILogger<FtpSocket> _logger;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int BufferSize { get; set; } = 8192;
    
    public async Task ConnectAsync(IPEndPoint endpoint, CancellationToken cancellationToken = default)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(Timeout);
            
            await _socket.ConnectAsync(endpoint, timeoutCts.Token);
            _logger.LogInformation("Connected to {Endpoint}", endpoint);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }
    
    public async Task<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(Timeout);
        
        var sent = await _socket.SendAsync(buffer, SocketFlags.None, timeoutCts.Token);
        _logger.LogTrace("Sent {ByteCount} bytes", sent);
        return sent;
    }
    
    public async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(Timeout);
        
        var received = await _socket.ReceiveAsync(buffer, SocketFlags.None, timeoutCts.Token);
        _logger.LogTrace("Received {ByteCount} bytes", received);
        return received;
    }
}
```

### Cross-Platform Considerations

**.NET Core Advantages**:
- Unified socket implementation across platforms
- Automatic platform-specific optimizations
- Built-in async support with proper cancellation

**Migration Benefits**:
- Eliminates platform-specific `#ifdef` blocks
- Reduces code complexity by ~40%
- Improves maintainability and testing

## FTP Protocol Engine Migration

### Core Client Interface

```csharp
public interface IFtpClient : IDisposable
{
    // Connection Management
    Task<FtpResult> ConnectAsync(FtpConnectionInfo connectionInfo, CancellationToken cancellationToken = default);
    Task<FtpResult> DisconnectAsync(CancellationToken cancellationToken = default);
    
    // Authentication
    Task<FtpResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<FtpResult> LoginAnonymousAsync(CancellationToken cancellationToken = default);
    
    // File Operations
    Task<FtpResult> UploadFileAsync(string localPath, string remotePath, 
        IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default);
    Task<FtpResult> DownloadFileAsync(string remotePath, string localPath, 
        IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default);
    
    // Directory Operations
    Task<FtpResult<IEnumerable<FtpFileInfo>>> ListDirectoryAsync(string path = null, 
        CancellationToken cancellationToken = default);
    Task<FtpResult<string>> GetCurrentDirectoryAsync(CancellationToken cancellationToken = default);
    Task<FtpResult> ChangeDirectoryAsync(string path, CancellationToken cancellationToken = default);
    
    // Events
    event EventHandler<FtpCommandEventArgs> CommandSent;
    event EventHandler<FtpReplyEventArgs> ReplyReceived;
    event EventHandler<FtpErrorEventArgs> ErrorOccurred;
    
    // Properties
    bool IsConnected { get; }
    FtpConnectionInfo ConnectionInfo { get; }
    FtpClientOptions Options { get; }
}
```

### Result Pattern Implementation

```csharp
public class FtpResult
{
    public bool IsSuccess { get; }
    public FtpReply Reply { get; }
    public Exception Exception { get; }
    public string ErrorMessage { get; }
    
    public static FtpResult Success(FtpReply reply) => new(true, reply, null, null);
    public static FtpResult Failure(FtpReply reply, string message) => new(false, reply, null, message);
    public static FtpResult Error(Exception exception) => new(false, null, exception, exception.Message);
    
    protected FtpResult(bool isSuccess, FtpReply reply, Exception exception, string errorMessage)
    {
        IsSuccess = isSuccess;
        Reply = reply;
        Exception = exception;
        ErrorMessage = errorMessage;
    }
}

public class FtpResult<T> : FtpResult
{
    public T Value { get; }
    
    public static FtpResult<T> Success(T value, FtpReply reply) => new(true, value, reply, null, null);
    public static new FtpResult<T> Failure(FtpReply reply, string message) => new(false, default, reply, null, message);
    public static new FtpResult<T> Error(Exception exception) => new(false, default, null, exception, exception.Message);
    
    private FtpResult(bool isSuccess, T value, FtpReply reply, Exception exception, string errorMessage)
        : base(isSuccess, reply, exception, errorMessage)
    {
        Value = value;
    }
}
```

### Command/Response System

```csharp
public class FtpCommand
{
    public FtpCommandType Type { get; }
    public string[] Arguments { get; }
    public string CommandText => FormatCommand();
    
    public FtpCommand(FtpCommandType type, params string[] arguments)
    {
        Type = type;
        Arguments = arguments ?? Array.Empty<string>();
    }
    
    private string FormatCommand()
    {
        var command = Type.ToString().ToUpperInvariant();
        return Arguments.Length > 0 
            ? $"{command} {string.Join(" ", Arguments)}"
            : command;
    }
    
    // Factory methods for common commands
    public static FtpCommand User(string username) => new(FtpCommandType.USER, username);
    public static FtpCommand Pass(string password) => new(FtpCommandType.PASS, password);
    public static FtpCommand List(string path = null) => new(FtpCommandType.LIST, path ?? "");
    public static FtpCommand Retr(string filename) => new(FtpCommandType.RETR, filename);
    public static FtpCommand Stor(string filename) => new(FtpCommandType.STOR, filename);
}

public class FtpReply
{
    public int Code { get; }
    public string Message { get; }
    public string[] Lines { get; }
    public bool IsSuccess => Code >= 200 && Code < 300;
    public bool IsIntermediate => Code >= 100 && Code < 200;
    public bool IsError => Code >= 400;
    
    public FtpReply(int code, string message, string[] lines = null)
    {
        Code = code;
        Message = message ?? string.Empty;
        Lines = lines ?? new[] { message };
    }
    
    public static FtpReply Parse(string response)
    {
        var lines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return new FtpReply(0, "Empty response");
        
        var firstLine = lines[0];
        if (firstLine.Length < 4 || !int.TryParse(firstLine.Substring(0, 3), out var code))
            return new FtpReply(0, "Invalid response format");
        
        var message = firstLine.Length > 4 ? firstLine.Substring(4) : string.Empty;
        return new FtpReply(code, message, lines);
    }
}
```

## Event System Migration

### From Observer Pattern to .NET Events

**Current C++ Observer Pattern**:
```cpp
class CFTPClient::CNotification
{
    virtual void OnBytesReceived(const TByteVector& vBuffer, long lReceivedBytes) {}
    virtual void OnSendCommand(const CCommand& Command, const CArg& Arguments) {}
};
```

**New .NET Event System**:
```csharp
public class FtpClient : IFtpClient
{
    // Strongly-typed events with proper EventArgs
    public event EventHandler<FtpCommandEventArgs> CommandSent;
    public event EventHandler<FtpReplyEventArgs> ReplyReceived;
    public event EventHandler<FtpTransferProgressEventArgs> TransferProgress;
    public event EventHandler<FtpErrorEventArgs> ErrorOccurred;
    
    protected virtual void OnCommandSent(FtpCommand command)
    {
        CommandSent?.Invoke(this, new FtpCommandEventArgs(command));
    }
    
    protected virtual void OnReplyReceived(FtpReply reply)
    {
        ReplyReceived?.Invoke(this, new FtpReplyEventArgs(reply));
    }
}

public class FtpCommandEventArgs : EventArgs
{
    public FtpCommand Command { get; }
    public DateTime Timestamp { get; }
    
    public FtpCommandEventArgs(FtpCommand command)
    {
        Command = command;
        Timestamp = DateTime.UtcNow;
    }
}

public class FtpReplyEventArgs : EventArgs
{
    public FtpReply Reply { get; }
    public DateTime Timestamp { get; }
    
    public FtpReplyEventArgs(FtpReply reply)
    {
        Reply = reply;
        Timestamp = DateTime.UtcNow;
    }
}
```

### Progress Reporting Migration

**Current C++ Progress System**:
```cpp
virtual void OnBytesReceived(const TByteVector& vBuffer, long lReceivedBytes) {}
virtual void OnPreReceiveFile(const tstring& strSourceFile, const tstring& strTargetFile, long lFileSize) {}
```

**New .NET Progress System**:
```csharp
public class TransferProgress
{
    public string SourcePath { get; }
    public string TargetPath { get; }
    public long BytesTransferred { get; }
    public long TotalBytes { get; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesTransferred / TotalBytes * 100 : 0;
    public TimeSpan Elapsed { get; }
    public double TransferRate => Elapsed.TotalSeconds > 0 ? BytesTransferred / Elapsed.TotalSeconds : 0;
    public TimeSpan EstimatedTimeRemaining { get; }
    public TransferState State { get; }
    
    public TransferProgress(string sourcePath, string targetPath, long bytesTransferred, 
        long totalBytes, TimeSpan elapsed, TransferState state)
    {
        SourcePath = sourcePath;
        TargetPath = targetPath;
        BytesTransferred = bytesTransferred;
        TotalBytes = totalBytes;
        Elapsed = elapsed;
        State = state;
        EstimatedTimeRemaining = CalculateTimeRemaining();
    }
    
    private TimeSpan CalculateTimeRemaining()
    {
        if (TransferRate <= 0 || BytesTransferred >= TotalBytes) 
            return TimeSpan.Zero;
        
        var remainingBytes = TotalBytes - BytesTransferred;
        var remainingSeconds = remainingBytes / TransferRate;
        return TimeSpan.FromSeconds(remainingSeconds);
    }
}

public enum TransferState
{
    Initializing,
    Transferring,
    Paused,
    Completed,
    Cancelled,
    Error
}
```

## Dependency Injection Architecture

### Service Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFtpClient(this IServiceCollection services, 
        Action<FtpClientOptions> configureOptions = null)
    {
        // Core services
        services.AddSingleton<IFtpSocketFactory, FtpSocketFactory>();
        services.AddTransient<IFtpSocket, FtpSocket>();
        services.AddTransient<IFtpClient, FtpClient>();
        
        // Business services
        services.AddTransient<IFtpTransferService, FtpTransferService>();
        services.AddTransient<IFtpDirectoryService, FtpDirectoryService>();
        services.AddTransient<IFtpFileService, FtpFileService>();
        
        // Configuration
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });
        
        return services;
    }
}

// Usage in application startup
public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        
        services.AddFtpClient(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
            options.BufferSize = 8192;
            options.EnableLogging = true;
        });
        
        // UI services
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ConnectionSettingsViewModel>();
        services.AddTransient<FtpBrowserViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = new MainWindow
        {
            DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
        };
        
        mainWindow.Show();
        base.OnStartup(e);
    }
}
```

### Factory Pattern for Socket Creation

```csharp
public interface IFtpSocketFactory
{
    IFtpSocket CreateSocket();
    IFtpSocket CreateSecureSocket(); // For FTPS support
}

public class FtpSocketFactory : IFtpSocketFactory
{
    private readonly ILogger<FtpSocketFactory> _logger;
    private readonly IOptions<FtpClientOptions> _options;
    
    public FtpSocketFactory(ILogger<FtpSocketFactory> logger, IOptions<FtpClientOptions> options)
    {
        _logger = logger;
        _options = options;
    }
    
    public IFtpSocket CreateSocket()
    {
        var socket = new FtpSocket(_logger)
        {
            Timeout = _options.Value.DefaultTimeout,
            BufferSize = _options.Value.BufferSize
        };
        
        return socket;
    }
    
    public IFtpSocket CreateSecureSocket()
    {
        // Future FTPS implementation
        throw new NotImplementedException("FTPS support planned for future release");
    }
}
```

## Configuration System Migration

### From C++ Configuration to .NET Options Pattern

**Current C++ Configuration**:
```cpp
class CLogonInfo
{
    tstring m_strHostname;
    USHORT m_ushHostport;
    tstring m_strUsername;
    tstring m_strPassword;
    CFirewallType m_FwType;
};
```

**New .NET Configuration System**:
```csharp
public class FtpClientOptions
{
    public const string SectionName = "FtpClient";
    
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int BufferSize { get; set; } = 8192;
    public bool EnableLogging { get; set; } = true;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool EnableResume { get; set; } = true;
    public FtpTransferMode DefaultTransferMode { get; set; } = FtpTransferMode.Binary;
    public List<FtpConnectionProfile> SavedConnections { get; set; } = new();
}

public class FtpConnectionProfile
{
    public string Name { get; set; }
    public string Hostname { get; set; }
    public int Port { get; set; } = 21;
    public string Username { get; set; }
    public bool SavePassword { get; set; }
    public string EncryptedPassword { get; set; } // Encrypted storage
    public bool UsePassiveMode { get; set; } = true;
    public bool UseFirewall { get; set; }
    public FtpFirewallSettings FirewallSettings { get; set; } = new();
}

public class FtpFirewallSettings
{
    public FtpFirewallType Type { get; set; } = FtpFirewallType.None;
    public string Hostname { get; set; }
    public int Port { get; set; } = 21;
    public string Username { get; set; }
    public string EncryptedPassword { get; set; }
}
```

### JSON Configuration File

```json
{
  "FtpClient": {
    "DefaultTimeout": "00:00:30",
    "BufferSize": 8192,
    "EnableLogging": true,
    "LogLevel": "Information",
    "EnableResume": true,
    "DefaultTransferMode": "Binary",
    "SavedConnections": [
      {
        "Name": "Test Server",
        "Hostname": "ftp.example.com",
        "Port": 21,
        "Username": "testuser",
        "SavePassword": false,
        "UsePassiveMode": true,
        "UseFirewall": false
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FtpClient": "Debug"
    }
  }
}
```

## Error Handling Strategy

### Exception Hierarchy

```csharp
public abstract class FtpException : Exception
{
    public FtpReply Reply { get; }
    
    protected FtpException(string message, FtpReply reply = null, Exception innerException = null)
        : base(message, innerException)
    {
        Reply = reply;
    }
}

public class FtpConnectionException : FtpException
{
    public FtpConnectionException(string message, Exception innerException = null)
        : base(message, null, innerException) { }
}

public class FtpAuthenticationException : FtpException
{
    public FtpAuthenticationException(string message, FtpReply reply)
        : base(message, reply) { }
}

public class FtpTransferException : FtpException
{
    public string LocalPath { get; }
    public string RemotePath { get; }
    public long BytesTransferred { get; }
    
    public FtpTransferException(string message, string localPath, string remotePath, 
        long bytesTransferred, FtpReply reply = null, Exception innerException = null)
        : base(message, reply, innerException)
    {
        LocalPath = localPath;
        RemotePath = remotePath;
        BytesTransferred = bytesTransferred;
    }
}
```

### Structured Logging

```csharp
public class FtpClient : IFtpClient
{
    private readonly ILogger<FtpClient> _logger;
    
    public async Task<FtpResult> ConnectAsync(FtpConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
    {
        using var scope = _logger.BeginScope("Connecting to {Hostname}:{Port}", 
            connectionInfo.Hostname, connectionInfo.Port);
        
        try
        {
            _logger.LogInformation("Attempting connection to FTP server");
            
            await _socket.ConnectAsync(connectionInfo.EndPoint, cancellationToken);
            
            _logger.LogInformation("Successfully connected to FTP server");
            return FtpResult.Success(null);
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "Failed to connect to FTP server: {ErrorMessage}", ex.Message);
            return FtpResult.Error(new FtpConnectionException("Connection failed", ex));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Connection attempt was cancelled");
            return FtpResult.Error(new FtpConnectionException("Connection cancelled"));
        }
    }
}
```

## Performance Considerations

### Memory Management

**C++ Smart Pointers Migration**:
```cpp
std::auto_ptr<IBlockingSocket> m_apSckControlConnection;
std::auto_ptr<CRepresentation> m_apCurrentRepresentation;
```

**C# Memory Management**:
```csharp
public class FtpClient : IFtpClient, IDisposable
{
    private readonly IFtpSocket _controlSocket;
    private readonly SemaphoreSlim _commandSemaphore = new(1, 1);
    private bool _disposed;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _controlSocket?.Dispose();
            _commandSemaphore?.Dispose();
            _disposed = true;
        }
    }
}
```

### Async Performance Optimization

```csharp
public class FtpTransferService : IFtpTransferService
{
    private readonly IFtpClient _ftpClient;
    private readonly ILogger<FtpTransferService> _logger;
    
    public async Task<bool> UploadFileAsync(string localPath, string remotePath, 
        IProgress<TransferProgress> progress = null, CancellationToken cancellationToken = default)
    {
        const int bufferSize = 65536; // 64KB buffer for optimal performance
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        
        try
        {
            using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read, 
                FileShare.Read, bufferSize, FileOptions.SequentialScan);
            using var dataStream = await _ftpClient.OpenDataStreamAsync(remotePath, cancellationToken);
            
            var totalBytes = fileStream.Length;
            var bytesTransferred = 0L;
            var stopwatch = Stopwatch.StartNew();
            
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, bufferSize, cancellationToken)) > 0)
            {
                await dataStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                
                bytesTransferred += bytesRead;
                progress?.Report(new TransferProgress(localPath, remotePath, bytesTransferred, 
                    totalBytes, stopwatch.Elapsed, TransferState.Transferring));
            }
            
            return true;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
```

## Testing Architecture

### Unit Testing Strategy

```csharp
public class FtpClientTests
{
    private readonly Mock<IFtpSocket> _mockSocket;
    private readonly Mock<ILogger<FtpClient>> _mockLogger;
    private readonly FtpClient _ftpClient;
    
    public FtpClientTests()
    {
        _mockSocket = new Mock<IFtpSocket>();
        _mockLogger = new Mock<ILogger<FtpClient>>();
        _ftpClient = new FtpClient(_mockSocket.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task ConnectAsync_ValidEndpoint_ReturnsSuccess()
    {
        // Arrange
        var endpoint = new IPEndPoint(IPAddress.Loopback, 21);
        var connectionInfo = new FtpConnectionInfo("localhost", 21, "user", "pass");
        
        _mockSocket.Setup(s => s.ConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        
        // Act
        var result = await _ftpClient.ConnectAsync(connectionInfo);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockSocket.Verify(s => s.ConnectAsync(endpoint, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Integration Testing

```csharp
[Collection("FTP Integration Tests")]
public class FtpIntegrationTests : IClassFixture<FtpServerFixture>
{
    private readonly FtpServerFixture _ftpServer;
    private readonly IFtpClient _ftpClient;
    
    public FtpIntegrationTests(FtpServerFixture ftpServer)
    {
        _ftpServer = ftpServer;
        _ftpClient = new FtpClient(new FtpSocket(), Mock.Of<ILogger<FtpClient>>());
    }
    
    [Fact]
    public async Task UploadDownload_LargeFile_MaintainsIntegrity()
    {
        // Arrange
        var testFile = CreateTestFile(1024 * 1024 * 10); // 10MB test file
        var remotePath = "/test/large_file.bin";
        var downloadPath = Path.GetTempFileName();
        
        try
        {
            // Act
            await _ftpClient.ConnectAsync(_ftpServer.ConnectionInfo);
            var uploadResult = await _ftpClient.UploadFileAsync(testFile, remotePath);
            var downloadResult = await _ftpClient.DownloadFileAsync(remotePath, downloadPath);
            
            // Assert
            Assert.True(uploadResult.IsSuccess);
            Assert.True(downloadResult.IsSuccess);
            Assert.True(FilesAreEqual(testFile, downloadPath));
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(downloadPath);
        }
    }
}
```

## Migration Benefits Summary

### Performance Improvements
- **Async Operations**: Eliminates UI blocking during network operations
- **Memory Efficiency**: Reduced memory allocation through ArrayPool usage
- **Scalability**: Better resource utilization with async/await patterns

### Maintainability Enhancements
- **Dependency Injection**: Improved testability and loose coupling
- **Structured Logging**: Better debugging and monitoring capabilities
- **Modern C# Features**: Cleaner, more readable code

### Cross-Platform Compatibility
- **Unified Codebase**: Single implementation for Windows and Linux
- **Platform Abstraction**: .NET Core handles platform differences
- **Deployment Flexibility**: Multiple deployment options

### Developer Experience
- **IntelliSense**: Better IDE support and code completion
- **Debugging**: Enhanced debugging capabilities in Visual Studio
- **Testing**: Comprehensive unit and integration testing support

## Conclusion

The technical architecture for the .NET Core migration provides a solid foundation for modernizing the FtpClient application while maintaining full functional compatibility. The async-first design, dependency injection architecture, and modern C# patterns will result in a more maintainable, testable, and performant application. The structured approach to error handling, logging, and configuration management ensures enterprise-grade quality and supportability.
