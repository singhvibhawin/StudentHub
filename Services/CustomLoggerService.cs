using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace ConnectingDatabase.Services
{
    public class CustomLoggerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _logDirectory;

        public CustomLoggerService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "CustomLogs");

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public async Task LogAsync(string message)
        {
            try
            {
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {ipAddress} | {message}";

                var logFilePath = Path.Combine(_logDirectory, $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt");

                // Ensure the log file exists
                using (var writer = new StreamWriter(logFilePath, true))
                {
                    await writer.WriteLineAsync(logEntry);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions during logging
                Console.WriteLine($"Error logging message: {ex.Message}");
            }
        }
    }
}
