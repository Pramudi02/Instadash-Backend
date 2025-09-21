using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    [BsonIgnoreExtraElements]
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("action")]
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [BsonElement("user")]
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [BsonElement("userId")]
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [BsonElement("timestamp")]
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Success";

        [BsonElement("ipAddress")]
        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [BsonElement("userAgent")]
        [JsonPropertyName("userAgent")]
        public string? UserAgent { get; set; }

        [BsonElement("module")]
        [JsonPropertyName("module")]
        public string? Module { get; set; }

        [BsonElement("details")]
        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [BsonElement("oldValues")]
        [JsonPropertyName("oldValues")]
        public object? OldValues { get; set; }

        [BsonElement("newValues")]
        [JsonPropertyName("newValues")]
        public object? NewValues { get; set; }

        [BsonElement("severity")]
        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical

        [BsonElement("category")]
        [JsonPropertyName("category")]
        public string Category { get; set; } = "General"; // Authentication, Authorization, Data, System, etc.
    }

    public static class AuditActions
    {
        public const string Login = "User Login";
        public const string Logout = "User Logout";
        public const string LoginFailed = "Login Failed";
        public const string PasswordReset = "Password Reset";
        public const string PasswordChange = "Password Change";
        public const string UserCreated = "User Created";
        public const string UserUpdated = "User Updated";
        public const string UserDeleted = "User Deleted";
        public const string PermissionChange = "Permission Change";
        public const string RoleAssigned = "Role Assigned";
        public const string RoleRemoved = "Role Removed";
        public const string DataAccess = "Data Access";
        public const string DataModification = "Data Modification";
        public const string DataDeletion = "Data Deletion";
        public const string SystemConfigChange = "System Configuration Change";
        public const string SecuritySettingChange = "Security Setting Change";
        public const string ApiKeyCreated = "API Key Created";
        public const string ApiKeyDeleted = "API Key Deleted";
        public const string UnauthorizedAccess = "Unauthorized Access Attempt";
    }

    public static class AuditStatus
    {
        public const string Success = "Success";
        public const string Failed = "Failed";
        public const string Pending = "Pending";
        public const string Warning = "Warning";
        public const string Error = "Error";
    }

    public static class AuditSeverity
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Error = "Error";
        public const string Critical = "Critical";
    }

    public static class AuditCategory
    {
        public const string Authentication = "Authentication";
        public const string Authorization = "Authorization";
        public const string UserManagement = "User Management";
        public const string DataAccess = "Data Access";
        public const string SystemConfig = "System Configuration";
        public const string Security = "Security";
        public const string General = "General";
    }
}
