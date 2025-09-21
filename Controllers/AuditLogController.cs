using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Temporarily commented for testing
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var logs = await _auditLogService.GetLogsAsync(page, pageSize, search, status, category, fromDate, toDate);
                var totalCount = await _auditLogService.GetLogCountAsync(search, status, category, fromDate, toDate);
                
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    logs = logs,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving audit logs", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, int>>> GetStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _auditLogService.GetLogStatisticsAsync(fromDate, toDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving audit statistics", error = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportLogs(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string format = "csv")
        {
            try
            {
                var logs = await _auditLogService.ExportLogsAsync(fromDate, toDate);

                if (format.ToLower() == "csv")
                {
                    var csv = GenerateCsv(logs);
                    var bytes = Encoding.UTF8.GetBytes(csv);
                    
                    return File(bytes, "text/csv", $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                }
                else
                {
                    return Ok(logs);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting audit logs", error = ex.Message });
            }
        }

        [HttpPost("log")]
        public async Task<IActionResult> CreateLog([FromBody] AuditLog auditLog)
        {
            try
            {
                await _auditLogService.LogAsync(auditLog);
                return Ok(new { message = "Audit log created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating audit log", error = ex.Message });
            }
        }

        [HttpPost("create-sample-logs")]
        [AllowAnonymous] // Allow anonymous access for testing
        public async Task<IActionResult> CreateSampleLogs()
        {
            try
            {
                var sampleLogs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        Action = AuditActions.Login,
                        User = "admin@example.com",
                        Status = AuditStatus.Success,
                        Details = "Admin user successfully logged in",
                        Category = AuditCategory.Authentication,
                        Severity = AuditSeverity.Info,
                        IpAddress = "192.168.1.100",
                        Timestamp = DateTime.UtcNow.AddHours(-2)
                    },
                    new AuditLog
                    {
                        Action = AuditActions.UserCreated,
                        User = "admin@example.com",
                        Status = AuditStatus.Success,
                        Details = "New user account created for john.doe@example.com",
                        Category = AuditCategory.UserManagement,
                        Severity = AuditSeverity.Info,
                        IpAddress = "192.168.1.100",
                        Timestamp = DateTime.UtcNow.AddHours(-1)
                    },
                    new AuditLog
                    {
                        Action = AuditActions.LoginFailed,
                        User = "unknown@example.com",
                        Status = AuditStatus.Failed,
                        Details = "Invalid password attempt",
                        Category = AuditCategory.Authentication,
                        Severity = AuditSeverity.Warning,
                        IpAddress = "192.168.1.200",
                        Timestamp = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new AuditLog
                    {
                        Action = AuditActions.SystemConfigChange,
                        User = "admin@example.com",
                        Status = AuditStatus.Success,
                        Details = "Updated system security settings",
                        Category = AuditCategory.SystemConfig,
                        Severity = AuditSeverity.Info,
                        IpAddress = "192.168.1.100",
                        Timestamp = DateTime.UtcNow.AddMinutes(-15)
                    },
                    new AuditLog
                    {
                        Action = AuditActions.UnauthorizedAccess,
                        User = "suspicious@example.com",
                        Status = AuditStatus.Error,
                        Details = "Attempted to access restricted endpoint without proper authorization",
                        Category = AuditCategory.Security,
                        Severity = AuditSeverity.Critical,
                        IpAddress = "10.0.0.50",
                        Timestamp = DateTime.UtcNow.AddMinutes(-5)
                    }
                };

                foreach (var log in sampleLogs)
                {
                    await _auditLogService.LogAsync(log);
                }

                return Ok(new { message = $"Created {sampleLogs.Count} sample audit logs successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating sample logs", error = ex.Message });
            }
        }

        [HttpGet("categories")]
        public ActionResult<List<string>> GetCategories()
        {
            var categories = new List<string>
            {
                AuditCategory.Authentication,
                AuditCategory.Authorization,
                AuditCategory.UserManagement,
                AuditCategory.DataAccess,
                AuditCategory.SystemConfig,
                AuditCategory.Security,
                AuditCategory.General
            };

            return Ok(categories);
        }

        [HttpGet("statuses")]
        public ActionResult<List<string>> GetStatuses()
        {
            var statuses = new List<string>
            {
                AuditStatus.Success,
                AuditStatus.Failed,
                AuditStatus.Pending,
                AuditStatus.Warning,
                AuditStatus.Error
            };

            return Ok(statuses);
        }

        private string GenerateCsv(List<AuditLog> logs)
        {
            var csv = new StringBuilder();
            
            // Add headers
            csv.AppendLine("Timestamp,Action,User,Status,Category,Severity,IP Address,Details");
            
            // Add data
            foreach (var log in logs)
            {
                var line = $"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(log.Action)}\",\"{EscapeCsv(log.User)}\",\"{EscapeCsv(log.Status)}\",\"{EscapeCsv(log.Category)}\",\"{EscapeCsv(log.Severity)}\",\"{EscapeCsv(log.IpAddress ?? "")}\",\"{EscapeCsv(log.Details ?? "")}\"";
                csv.AppendLine(line);
            }
            
            return csv.ToString();
        }

        private string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
                
            return value.Replace("\"", "\"\"");
        }
    }
}
