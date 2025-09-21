using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;

namespace Backend.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string user, string status = AuditStatus.Success, string? details = null, string? module = null, string severity = AuditSeverity.Info, string category = AuditCategory.General, object? oldValues = null, object? newValues = null);
        Task LogAsync(AuditLog auditLog);
        Task<List<AuditLog>> GetLogsAsync(int page = 1, int pageSize = 50, string? search = null, string? status = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<long> GetLogCountAsync(string? search = null, string? status = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLog>> ExportLogsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<string, int>> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IMongoCollection<AuditLog> _auditLogsCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IOptions<Backend.Models.MongoDBSettings> mongoDBSettings, IHttpContextAccessor httpContextAccessor)
        {
            var settings = mongoDBSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _auditLogsCollection = database.GetCollection<AuditLog>("audit_logs");
            _httpContextAccessor = httpContextAccessor;

            // Create indexes for better performance
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var timestampIndex = Builders<AuditLog>.IndexKeys.Descending(x => x.Timestamp);
                var userIndex = Builders<AuditLog>.IndexKeys.Ascending(x => x.User);
                var actionIndex = Builders<AuditLog>.IndexKeys.Ascending(x => x.Action);
                var statusIndex = Builders<AuditLog>.IndexKeys.Ascending(x => x.Status);
                var categoryIndex = Builders<AuditLog>.IndexKeys.Ascending(x => x.Category);

                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(timestampIndex));
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(userIndex));
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(actionIndex));
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(statusIndex));
                _auditLogsCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(categoryIndex));
            }
            catch (Exception ex)
            {
                // Log the error but don't fail startup
                Console.WriteLine($"Error creating audit log indexes: {ex.Message}");
            }
        }

        public async Task LogAsync(string action, string user, string status = AuditStatus.Success, string? details = null, string? module = null, string severity = AuditSeverity.Info, string category = AuditCategory.General, object? oldValues = null, object? newValues = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
                var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var auditLog = new AuditLog
                {
                    Action = action,
                    User = user,
                    UserId = userId,
                    Status = status,
                    Details = details,
                    Module = module,
                    Severity = severity,
                    Category = category,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Timestamp = DateTime.UtcNow
                };

                await _auditLogsCollection.InsertOneAsync(auditLog);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error logging audit entry: {ex.Message}");
            }
        }

        public async Task LogAsync(AuditLog auditLog)
        {
            try
            {
                if (auditLog.Timestamp == default)
                    auditLog.Timestamp = DateTime.UtcNow;

                await _auditLogsCollection.InsertOneAsync(auditLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging audit entry: {ex.Message}");
            }
        }

        public async Task<List<AuditLog>> GetLogsAsync(int page = 1, int pageSize = 50, string? search = null, string? status = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var filter = BuildFilter(search, status, category, fromDate, toDate);
            
            var skip = (page - 1) * pageSize;
            
            return await _auditLogsCollection
                .Find(filter)
                .Sort(Builders<AuditLog>.Sort.Descending(x => x.Timestamp))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> GetLogCountAsync(string? search = null, string? status = null, string? category = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var filter = BuildFilter(search, status, category, fromDate, toDate);
            return await _auditLogsCollection.CountDocumentsAsync(filter);
        }

        public async Task<List<AuditLog>> ExportLogsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var filter = BuildFilter(null, null, null, fromDate, toDate);
            
            return await _auditLogsCollection
                .Find(filter)
                .Sort(Builders<AuditLog>.Sort.Descending(x => x.Timestamp))
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var filter = BuildFilter(null, null, null, fromDate, toDate);
            
            var pipeline = new[]
            {
                new BsonDocument("$match", filter.ToBsonDocument()),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$status" },
                    { "count", new BsonDocument("$sum", 1) }
                })
            };

            var results = await _auditLogsCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            
            var statistics = new Dictionary<string, int>();
            foreach (var result in results)
            {
                var status = result["_id"].AsString;
                var count = result["count"].AsInt32;
                statistics[status] = count;
            }

            return statistics;
        }

        private FilterDefinition<AuditLog> BuildFilter(string? search, string? status, string? category, DateTime? fromDate, DateTime? toDate)
        {
            var filters = new List<FilterDefinition<AuditLog>>();

            if (!string.IsNullOrEmpty(search))
            {
                var searchFilter = Builders<AuditLog>.Filter.Or(
                    Builders<AuditLog>.Filter.Regex(x => x.Action, new BsonRegularExpression(search, "i")),
                    Builders<AuditLog>.Filter.Regex(x => x.User, new BsonRegularExpression(search, "i")),
                    Builders<AuditLog>.Filter.Regex(x => x.Details, new BsonRegularExpression(search, "i"))
                );
                filters.Add(searchFilter);
            }

            if (!string.IsNullOrEmpty(status))
            {
                filters.Add(Builders<AuditLog>.Filter.Eq(x => x.Status, status));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filters.Add(Builders<AuditLog>.Filter.Eq(x => x.Category, category));
            }

            if (fromDate.HasValue)
            {
                filters.Add(Builders<AuditLog>.Filter.Gte(x => x.Timestamp, fromDate.Value));
            }

            if (toDate.HasValue)
            {
                filters.Add(Builders<AuditLog>.Filter.Lte(x => x.Timestamp, toDate.Value.AddDays(1).AddTicks(-1)));
            }

            return filters.Count > 0 
                ? Builders<AuditLog>.Filter.And(filters) 
                : FilterDefinition<AuditLog>.Empty;
        }

        private string? GetClientIpAddress(HttpContext? context)
        {
            if (context == null) return null;

            // Check for forwarded IP first (in case of proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ip = forwardedFor.Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(ip))
                    return ip;
            }

            // Check for real IP
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
