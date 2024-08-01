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
                var dateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var logFilePath = Path.Combine(_logDirectory, $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt");

                // Prepare log entry with appropriate formatting
                var logEntry = $"| {dateTime,-17} | {ipAddress,-15} | {message,-30} |";

                // Ensure the log file exists
                bool fileExists = File.Exists(logFilePath);
                using (var writer = new StreamWriter(logFilePath, true))
                {
                    if (!fileExists)
                    {
                        // Write the header if the file is new
                        await writer.WriteLineAsync("| Date and Time        | IP Address        | Message                                       |");
                        await writer.WriteLineAsync("|----------------------|-------------------|-----------------------------------------------|");
                    }

                    // Write the log entry
                    await writer.WriteLineAsync(logEntry);
                }
            }
            catch (Exception ex)
            {
                // Handle any potential exceptions
                // For example, log to a separate error log or notify an admin
            }
        }

    }
}
