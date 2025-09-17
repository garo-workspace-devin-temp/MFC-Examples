//using System;
using System.Threading.Tasks;

namespace TestFtpConnection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing FTP Connection to ftp.dlptest.com...");
            
            var ftpClient = new FtpClient.UI.Services.FtpClient();
            
            ftpClient.CommandSent += (cmd) => Console.WriteLine($"SENT: {cmd}");
            ftpClient.ResponseReceived += (resp) => Console.WriteLine($"RECV: {resp}");
            ftpClient.ConnectionStateChanged += (sender, args) => 
                Console.WriteLine($"STATE: {args.OldState} -> {args.NewState} ({args.Message})");
            
            var logonInfo = new FtpClient.UI.Models.LogonInfo
            {
                Hostname = "ftp.dlptest.com",
                Hostport = 21,
                Username = "dlpuser",
                Password = "rNrKYTX9g7z3RgJRmxWuGHbeu",
                PassiveMode = true,
                FirewallType = FtpClient.UI.Models.FirewallType.None
            };
            
            try
            {
                Console.WriteLine("Attempting to login...");
                bool loginSuccess = await ftpClient.LoginAsync(logonInfo);
                Console.WriteLine($"Login result: {loginSuccess}");
                Console.WriteLine($"Is authenticated: {ftpClient.IsAuthenticated}");
                Console.WriteLine($"Connection state: {ftpClient.ConnectionState}");
                
                if (loginSuccess && ftpClient.IsAuthenticated)
                {
                    Console.WriteLine("\nAttempting directory listing...");
                    var files = await ftpClient.ListDirectoryAsync("/");
                    Console.WriteLine($"Directory listing successful! Found {files.Count} items:");
                    
                    foreach (var file in files)
                    {
                        Console.WriteLine($"  {file.Type} - {file.Name} ({file.Size} bytes)");
                    }
                }
                else
                {
                    Console.WriteLine("Login failed - cannot test directory listing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                Console.WriteLine($"STACK: {ex.StackTrace}");
            }
            finally
            {
                await ftpClient.LogoutAsync();
                ftpClient.Dispose();
            }
            
            Console.WriteLine("Test completed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
