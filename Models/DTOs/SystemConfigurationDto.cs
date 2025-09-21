using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs
{
    public class SystemConfigDto
    {
        public string? Id { get; set; }
        
        [Required]
        public string ConfigKey { get; set; } = string.Empty;
        
        [Required]
        public string ConfigValue { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string DataType { get; set; } = "string";
        public string Category { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; } = false;
        public bool IsEditable { get; set; } = true;
        public string? ValidationRules { get; set; }
    }

    public class UpdateSystemConfigDto
    {
        [Required]
        public string ConfigValue { get; set; } = string.Empty;
    }

    public class ApiKeyDto
    {
        public string? Id { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class CreateApiKeyDto
    {
        [Required]
        public string KeyName { get; set; } = string.Empty;
        
        public List<string> Permissions { get; set; } = new List<string>();
        
        public DateTime? ExpiresAt { get; set; }
    }

    public class SecuritySettingsDto
    {
        public string? Id { get; set; }
        public PasswordPolicyDto PasswordPolicy { get; set; } = new PasswordPolicyDto();
        public int SessionTimeoutMinutes { get; set; } = 60;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;
    }

    public class PasswordPolicyDto
    {
        [Range(6, 50)]
        public int MinLength { get; set; } = 8;
        
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireNumbers { get; set; } = true;
        public bool RequireSpecialChars { get; set; } = true;
        
        [Range(30, 365)]
        public int? PasswordExpiryDays { get; set; } = 90;
    }

    public class SystemConfigurationSummaryDto
    {
        public SecuritySettingsDto SecuritySettings { get; set; } = new SecuritySettingsDto();
        public List<SystemConfigDto> GeneralSettings { get; set; } = new List<SystemConfigDto>();
        public List<ApiKeyDto> ApiKeys { get; set; } = new List<ApiKeyDto>();
        public SystemHealthDto SystemHealth { get; set; } = new SystemHealthDto();
    }

    public class SystemHealthDto
    {
        public bool DatabaseConnected { get; set; }
        public bool EmailServiceConnected { get; set; }
        public int ActiveUsers { get; set; }
        public DateTime LastBackup { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double CpuUsagePercent { get; set; }
    }
}
