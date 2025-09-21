using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;

        public SystemController(ILogger<SystemController> logger)
        {
            _logger = logger;
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                await Task.Delay(1); // Small delay to make it truly async
                var systemHealth = new
                {
                    status = "Healthy",
                    uptime = GetSystemUptime(),
                    cpuUsage = GetCpuUsage(),
                    memoryUsage = GetMemoryUsage(),
                    diskUsage = GetDiskUsage(),
                    responseTime = 45,
                    timestamp = DateTime.UtcNow
                };

                return Ok(systemHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                return StatusCode(500, new { error = "Failed to get system health" });
            }
        }

        [HttpGet("api-health")]
        public async Task<IActionResult> GetApiHealth()
        {
            try
            {
                // Check database connection and other services
                var dbConnectionOk = await CheckDatabaseConnectionInternal();
                var apiResponseTime = await MeasureApiResponseTime();

                var apiHealth = new
                {
                    status = dbConnectionOk && apiResponseTime < 200 ? "Normal" : 
                            apiResponseTime < 500 ? "Degraded" : "Down",
                    responseTime = apiResponseTime,
                    endpoints = new
                    {
                        auth = true,
                        database = dbConnectionOk,
                        external = await CheckExternalServices()
                    },
                    timestamp = DateTime.UtcNow
                };

                return Ok(apiHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API health");
                return StatusCode(500, new { error = "Failed to get API health" });
            }
        }

        [HttpGet("metrics")]
        public IActionResult GetSystemMetrics()
        {
            try
            {
                var metrics = new
                {
                    cpu = GetCpuUsage(),
                    memory = GetMemoryUsage(),
                    disk = GetDiskUsage(),
                    network = GetNetworkUsage(),
                    timestamp = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
                return StatusCode(500, new { error = "Failed to get system metrics" });
            }
        }

        [HttpGet("db-check")]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            try
            {
                var isConnected = await CheckDatabaseConnectionInternal();
                return Ok(new { connected = isConnected });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database connection");
                return Ok(new { connected = false });
            }
        }

        private double GetSystemUptime()
        {
            try
            {
                var uptime = Environment.TickCount64 / 1000.0 / 60 / 60; // Convert to hours
                var uptimePercentage = Math.Min(99.9, 95 + (uptime / 24) * 2); // Simulate realistic uptime
                return Math.Round(uptimePercentage, 1);
            }
            catch
            {
                return 99.5; // Default value
            }
        }

        private int GetCpuUsage()
        {
            try
            {
                // Simulate CPU usage - in a real scenario, you'd use performance counters
                var random = new Random();
                return random.Next(15, 65); // Random CPU usage between 15-65%
            }
            catch
            {
                return 25; // Default value
            }
        }

        private int GetMemoryUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var totalMemory = GC.GetTotalMemory(false);
                
                // Simulate memory usage percentage
                var random = new Random();
                return random.Next(30, 70); // Random memory usage between 30-70%
            }
            catch
            {
                return 45; // Default value
            }
        }

        private int GetDiskUsage()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed);

                if (drives.Any())
                {
                    var mainDrive = drives.First();
                    var usedSpace = mainDrive.TotalSize - mainDrive.AvailableFreeSpace;
                    var usagePercentage = (double)usedSpace / mainDrive.TotalSize * 100;
                    return (int)Math.Round(usagePercentage);
                }
                
                return 55; // Default value
            }
            catch
            {
                return 55; // Default value
            }
        }

        private int GetNetworkUsage()
        {
            try
            {
                // Simulate network usage - in a real scenario, you'd monitor network interfaces
                var random = new Random();
                return random.Next(5, 40); // Random network usage between 5-40%
            }
            catch
            {
                return 15; // Default value
            }
        }

        private async Task<bool> CheckDatabaseConnectionInternal()
        {
            try
            {
                // In a real scenario, you'd check your actual database connection
                // For now, simulate a connection check
                await Task.Delay(10); // Simulate async operation
                var random = new Random();
                return random.NextDouble() > 0.05; // 95% success rate
            }
            catch
            {
                return false;
            }
        }

        private async Task<int> MeasureApiResponseTime()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await Task.Delay(1); // Simulate API call
                stopwatch.Stop();
                
                // Add some random variation to simulate real response times
                var random = new Random();
                var baseTime = (int)stopwatch.ElapsedMilliseconds;
                return baseTime + random.Next(20, 150);
            }
            catch
            {
                return 100; // Default response time
            }
        }

        private async Task<bool> CheckExternalServices()
        {
            try
            {
                // Simulate checking external service availability
                await Task.Delay(5);
                var random = new Random();
                return random.NextDouble() > 0.1; // 90% success rate
            }
            catch
            {
                return false;
            }
        }
    }
}