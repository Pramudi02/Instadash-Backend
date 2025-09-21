using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    [BsonIgnoreExtraElements] // Ignore fields that don't exist in the model
    public class SystemConfiguration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("configKey")]
        public string ConfigKey { get; set; } = string.Empty;
        
        [BsonElement("configValue")]
        public string ConfigValue { get; set; } = string.Empty;
        
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        
        [BsonElement("dataType")]
        public string DataType { get; set; } = "string"; // string, boolean, number, json
        
        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;
        
        [BsonElement("isEncrypted")]
        public bool IsEncrypted { get; set; } = false;
        
        [BsonElement("isEditable")]
        public bool IsEditable { get; set; } = true;
        
        [BsonElement("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        [BsonElement("modifiedBy")]
        public string ModifiedBy { get; set; } = string.Empty;
        
        [BsonElement("validationRules")]
        public string? ValidationRules { get; set; }
    }

    [BsonIgnoreExtraElements] // Ignore fields that don't exist in the model
    public class ApiKeyConfiguration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("keyName")]
        public string KeyName { get; set; } = string.Empty;
        
        [BsonElement("keyValue")]
        public string KeyValue { get; set; } = string.Empty;
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("lastUsed")]
        public DateTime? LastUsed { get; set; }
        
        [BsonElement("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
        
        [BsonElement("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();
        
        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements] // Ignore fields that don't exist in the model (like removed twoFactorEnabled)
    public class SecuritySettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("passwordPolicy")]
        public PasswordPolicy PasswordPolicy { get; set; } = new PasswordPolicy();
        
        [BsonElement("sessionTimeout")]
        public int SessionTimeoutMinutes { get; set; } = 60;
        
        [BsonElement("maxLoginAttempts")]
        public int MaxLoginAttempts { get; set; } = 5;
        
        [BsonElement("lockoutDurationMinutes")]
        public int LockoutDurationMinutes { get; set; } = 30;
        
        [BsonElement("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        [BsonElement("modifiedBy")]
        public string ModifiedBy { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements] // Ignore fields that don't exist in the model
    public class PasswordPolicy
    {
        [BsonElement("minLength")]
        public int MinLength { get; set; } = 8;
        
        [BsonElement("requireUppercase")]
        public bool RequireUppercase { get; set; } = true;
        
        [BsonElement("requireLowercase")]
        public bool RequireLowercase { get; set; } = true;
        
        [BsonElement("requireNumbers")]
        public bool RequireNumbers { get; set; } = true;
        
        [BsonElement("requireSpecialChars")]
        public bool RequireSpecialChars { get; set; } = true;
        
        [BsonElement("passwordExpiryDays")]
        public int? PasswordExpiryDays { get; set; } = 90;
    }
}
