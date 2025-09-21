using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services
{
    public interface ISystemConfigurationService
    {
        Task<SystemConfigurationSummaryDto> GetSystemConfigurationSummaryAsync();
        Task<List<SystemConfigDto>> GetConfigurationsByCategoryAsync(string category);
        Task<SystemConfigDto?> GetConfigurationAsync(string configKey);
        Task<SystemConfigDto> UpdateConfigurationAsync(string configKey, UpdateSystemConfigDto updateDto, string modifiedBy);
        Task<SystemConfigDto> CreateConfigurationAsync(SystemConfigDto configDto, string createdBy);
        Task<bool> DeleteConfigurationAsync(string configKey);
        
        // Security Settings
        Task<SecuritySettingsDto> GetSecuritySettingsAsync();
        Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(SecuritySettingsDto securityDto, string modifiedBy);
        
        // API Key Management
        Task<List<ApiKeyDto>> GetApiKeysAsync();
        Task<ApiKeyDto> CreateApiKeyAsync(CreateApiKeyDto createDto, string createdBy);
        Task<bool> RegenerateApiKeyAsync(string keyId, string modifiedBy);
        Task<bool> RevokeApiKeyAsync(string keyId);
        Task<bool> ValidateApiKeyAsync(string keyValue);
        
        // System Health
        Task<SystemHealthDto> GetSystemHealthAsync();
        
        // Cleanup method
        Task CleanupDeprecatedFieldsAsync();
    }

    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IMongoCollection<SystemConfiguration> _configCollection;
        private readonly IMongoCollection<SecuritySettings> _securityCollection;
        private readonly IMongoCollection<ApiKeyConfiguration> _apiKeyCollection;
        private readonly IMongoDatabase _database;
        private static bool _cleanupExecuted = false;

        public SystemConfigurationService(IMongoDatabase database)
        {
            _database = database;
            
            _configCollection = _database.GetCollection<SystemConfiguration>("systemConfigurations");
            _securityCollection = _database.GetCollection<SecuritySettings>("securitySettings");
            _apiKeyCollection = _database.GetCollection<ApiKeyConfiguration>("apiKeys");
        }

        public async Task<SystemConfigurationSummaryDto> GetSystemConfigurationSummaryAsync()
        {
            // Clean up deprecated fields only once
            if (!_cleanupExecuted)
            {
                await CleanupDeprecatedFieldsAsync();
                _cleanupExecuted = true;
            }
            
            // Initialize default configurations if needed
            await InitializeDefaultConfigurationsAsync();
            
            var summary = new SystemConfigurationSummaryDto
            {
                SecuritySettings = await GetSecuritySettingsAsync(),
                GeneralSettings = await GetConfigurationsByCategoryAsync("general"),
                ApiKeys = await GetApiKeysAsync(),
                SystemHealth = await GetSystemHealthAsync()
            };

            return summary;
        }

        public async Task<List<SystemConfigDto>> GetConfigurationsByCategoryAsync(string category)
        {
            var configurations = await _configCollection
                .Find(c => c.Category == category)
                .ToListAsync();

            return configurations.Select(MapToDto).ToList();
        }

        public async Task<SystemConfigDto?> GetConfigurationAsync(string configKey)
        {
            var config = await _configCollection
                .Find(c => c.ConfigKey == configKey)
                .FirstOrDefaultAsync();

            return config != null ? MapToDto(config) : null;
        }

        public async Task<SystemConfigDto> UpdateConfigurationAsync(string configKey, UpdateSystemConfigDto updateDto, string modifiedBy)
        {
            var filter = Builders<SystemConfiguration>.Filter.Eq(c => c.ConfigKey, configKey);
            var update = Builders<SystemConfiguration>.Update
                .Set(c => c.ConfigValue, updateDto.ConfigValue)
                .Set(c => c.LastModified, DateTime.UtcNow)
                .Set(c => c.ModifiedBy, modifiedBy);

            var options = new FindOneAndUpdateOptions<SystemConfiguration>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedConfig = await _configCollection.FindOneAndUpdateAsync(filter, update, options);
            
            if (updatedConfig == null)
            {
                throw new InvalidOperationException($"Configuration with key '{configKey}' not found");
            }

            return MapToDto(updatedConfig);
        }

        public async Task<SystemConfigDto> CreateConfigurationAsync(SystemConfigDto configDto, string createdBy)
        {
            var config = new SystemConfiguration
            {
                ConfigKey = configDto.ConfigKey,
                ConfigValue = configDto.ConfigValue,
                Description = configDto.Description,
                DataType = configDto.DataType,
                Category = configDto.Category,
                IsEncrypted = configDto.IsEncrypted,
                IsEditable = configDto.IsEditable,
                ValidationRules = configDto.ValidationRules,
                LastModified = DateTime.UtcNow,
                ModifiedBy = createdBy
            };

            await _configCollection.InsertOneAsync(config);
            return MapToDto(config);
        }

        public async Task<bool> DeleteConfigurationAsync(string configKey)
        {
            var result = await _configCollection.DeleteOneAsync(c => c.ConfigKey == configKey);
            return result.DeletedCount > 0;
        }

        // Security Settings Implementation
        public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
        {
            try
            {
                var settings = await _securityCollection.Find(_ => true).FirstOrDefaultAsync();
                
                if (settings == null)
                {
                    settings = new SecuritySettings
                    {
                        SessionTimeoutMinutes = 60,
                        MaxLoginAttempts = 5,
                        LockoutDurationMinutes = 30,
                        PasswordPolicy = new PasswordPolicy
                        {
                            MinLength = 8,
                            RequireUppercase = true,
                            RequireLowercase = true,
                            RequireNumbers = true,
                            RequireSpecialChars = true,
                            PasswordExpiryDays = 90
                        }
                    };
                    await _securityCollection.InsertOneAsync(settings);
                }

                // Ensure PasswordPolicy is not null
                if (settings.PasswordPolicy == null)
                {
                    settings.PasswordPolicy = new PasswordPolicy
                    {
                        MinLength = 8,
                        RequireUppercase = true,
                        RequireLowercase = true,
                        RequireNumbers = true,
                        RequireSpecialChars = true,
                        PasswordExpiryDays = 90
                    };
                }

                return new SecuritySettingsDto
                {
                    Id = settings.Id,
                    SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
                    MaxLoginAttempts = settings.MaxLoginAttempts,
                    LockoutDurationMinutes = settings.LockoutDurationMinutes,
                    PasswordPolicy = new PasswordPolicyDto
                    {
                        MinLength = settings.PasswordPolicy.MinLength,
                        RequireUppercase = settings.PasswordPolicy.RequireUppercase,
                        RequireLowercase = settings.PasswordPolicy.RequireLowercase,
                        RequireNumbers = settings.PasswordPolicy.RequireNumbers,
                        RequireSpecialChars = settings.PasswordPolicy.RequireSpecialChars,
                        PasswordExpiryDays = settings.PasswordPolicy.PasswordExpiryDays
                    }
                };
            }
            catch (Exception ex)
            {
                // Log the error and create default settings
                Console.WriteLine($"Error retrieving security settings: {ex.Message}");
                
                // Return default security settings if there's an error
                return new SecuritySettingsDto
                {
                    SessionTimeoutMinutes = 60,
                    MaxLoginAttempts = 5,
                    LockoutDurationMinutes = 30,
                    PasswordPolicy = new PasswordPolicyDto
                    {
                        MinLength = 8,
                        RequireUppercase = true,
                        RequireLowercase = true,
                        RequireNumbers = true,
                        RequireSpecialChars = true,
                        PasswordExpiryDays = 90
                    }
                };
            }
        }

        public async Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(SecuritySettingsDto securityDto, string modifiedBy)
        {
            var filter = Builders<SecuritySettings>.Filter.Empty;
            var update = Builders<SecuritySettings>.Update
                .Set(s => s.SessionTimeoutMinutes, securityDto.SessionTimeoutMinutes)
                .Set(s => s.MaxLoginAttempts, securityDto.MaxLoginAttempts)
                .Set(s => s.LockoutDurationMinutes, securityDto.LockoutDurationMinutes)
                .Set(s => s.PasswordPolicy, new PasswordPolicy
                {
                    MinLength = securityDto.PasswordPolicy.MinLength,
                    RequireUppercase = securityDto.PasswordPolicy.RequireUppercase,
                    RequireLowercase = securityDto.PasswordPolicy.RequireLowercase,
                    RequireNumbers = securityDto.PasswordPolicy.RequireNumbers,
                    RequireSpecialChars = securityDto.PasswordPolicy.RequireSpecialChars,
                    PasswordExpiryDays = securityDto.PasswordPolicy.PasswordExpiryDays
                })
                .Set(s => s.LastModified, DateTime.UtcNow)
                .Set(s => s.ModifiedBy, modifiedBy);

            var options = new FindOneAndUpdateOptions<SecuritySettings>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var updatedSettings = await _securityCollection.FindOneAndUpdateAsync(filter, update, options);
            
            return new SecuritySettingsDto
            {
                Id = updatedSettings.Id,
                SessionTimeoutMinutes = updatedSettings.SessionTimeoutMinutes,
                MaxLoginAttempts = updatedSettings.MaxLoginAttempts,
                LockoutDurationMinutes = updatedSettings.LockoutDurationMinutes,
                PasswordPolicy = new PasswordPolicyDto
                {
                    MinLength = updatedSettings.PasswordPolicy.MinLength,
                    RequireUppercase = updatedSettings.PasswordPolicy.RequireUppercase,
                    RequireLowercase = updatedSettings.PasswordPolicy.RequireLowercase,
                    RequireNumbers = updatedSettings.PasswordPolicy.RequireNumbers,
                    RequireSpecialChars = updatedSettings.PasswordPolicy.RequireSpecialChars,
                    PasswordExpiryDays = updatedSettings.PasswordPolicy.PasswordExpiryDays
                }
            };
        }

        // API Key Management Implementation
        public async Task<List<ApiKeyDto>> GetApiKeysAsync()
        {
            var apiKeys = await _apiKeyCollection
                .Find(k => k.IsActive)
                .ToListAsync();

            return apiKeys.Select(k => new ApiKeyDto
            {
                Id = k.Id,
                KeyName = k.KeyName,
                KeyValue = MaskApiKey(k.KeyValue),
                IsActive = k.IsActive,
                CreatedAt = k.CreatedAt,
                LastUsed = k.LastUsed,
                ExpiresAt = k.ExpiresAt,
                Permissions = k.Permissions
            }).ToList();
        }

        public async Task<ApiKeyDto> CreateApiKeyAsync(CreateApiKeyDto createDto, string createdBy)
        {
            var keyValue = GenerateApiKey();
            
            var apiKey = new ApiKeyConfiguration
            {
                KeyName = createDto.KeyName,
                KeyValue = keyValue,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = createDto.ExpiresAt,
                Permissions = createDto.Permissions,
                CreatedBy = createdBy
            };

            await _apiKeyCollection.InsertOneAsync(apiKey);

            return new ApiKeyDto
            {
                Id = apiKey.Id,
                KeyName = apiKey.KeyName,
                KeyValue = keyValue, // Return full key only on creation
                IsActive = apiKey.IsActive,
                CreatedAt = apiKey.CreatedAt,
                ExpiresAt = apiKey.ExpiresAt,
                Permissions = apiKey.Permissions
            };
        }

        public async Task<bool> RegenerateApiKeyAsync(string keyId, string modifiedBy)
        {
            var newKeyValue = GenerateApiKey();
            
            var filter = Builders<ApiKeyConfiguration>.Filter.Eq(k => k.Id, keyId);
            var update = Builders<ApiKeyConfiguration>.Update
                .Set(k => k.KeyValue, newKeyValue)
                .Set(k => k.CreatedAt, DateTime.UtcNow)
                .Set(k => k.CreatedBy, modifiedBy);

            var result = await _apiKeyCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RevokeApiKeyAsync(string keyId)
        {
            var filter = Builders<ApiKeyConfiguration>.Filter.Eq(k => k.Id, keyId);
            var update = Builders<ApiKeyConfiguration>.Update.Set(k => k.IsActive, false);

            var result = await _apiKeyCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ValidateApiKeyAsync(string keyValue)
        {
            var apiKey = await _apiKeyCollection
                .Find(k => k.KeyValue == keyValue && k.IsActive)
                .FirstOrDefaultAsync();

            if (apiKey == null) return false;

            // Check if key is expired
            if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            {
                return false;
            }

            // Update last used timestamp
            var filter = Builders<ApiKeyConfiguration>.Filter.Eq(k => k.Id, apiKey.Id);
            var update = Builders<ApiKeyConfiguration>.Update.Set(k => k.LastUsed, DateTime.UtcNow);
            await _apiKeyCollection.UpdateOneAsync(filter, update);

            return true;
        }

        // System Health Implementation
        public async Task<SystemHealthDto> GetSystemHealthAsync()
        {
            var health = new SystemHealthDto();

            try
            {
                // Test database connection
                await _database.RunCommandAsync<object>("{ ping: 1 }");
                health.DatabaseConnected = true;
            }
            catch
            {
                health.DatabaseConnected = false;
            }

            // Get active users count
            try
            {
                var userCollection = _database.GetCollection<object>("users");
                health.ActiveUsers = (int)await userCollection.CountDocumentsAsync(_ => true);
            }
            catch
            {
                health.ActiveUsers = 0;
            }

            // Mock data for other health metrics
            health.EmailServiceConnected = true; // You can implement actual email service check
            health.LastBackup = DateTime.UtcNow.AddDays(-1); // Mock data
            health.MemoryUsagePercent = new Random().NextDouble() * 30 + 40; // Mock: 40-70%
            health.CpuUsagePercent = new Random().NextDouble() * 20 + 10; // Mock: 10-30%

            return health;
        }

        // Clean up method to remove deprecated fields from existing documents
        public async Task CleanupDeprecatedFieldsAsync()
        {
            try
            {
                // Remove twoFactorEnabled field from all SecuritySettings documents
                var updateDefinition = Builders<SecuritySettings>.Update.Unset("twoFactorEnabled");
                var result = await _securityCollection.UpdateManyAsync(_ => true, updateDefinition);
                
                // Only log if documents were actually updated
                if (result.ModifiedCount > 0)
                {
                    Console.WriteLine($"✅ Cleaned up deprecated twoFactorEnabled fields from {result.ModifiedCount} SecuritySettings documents");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error cleaning up deprecated fields: {ex.Message}");
            }
        }

        // Private helper methods
        private async Task InitializeDefaultConfigurationsAsync()
        {
            var existingConfigs = await _configCollection.CountDocumentsAsync(_ => true);
            
            if (existingConfigs == 0)
            {
                var defaultConfigs = new List<SystemConfiguration>
                {
                    new SystemConfiguration
                    {
                        ConfigKey = "app.name",
                        ConfigValue = "InstaDash",
                        Description = "Application name",
                        Category = "general",
                        DataType = "string",
                        IsEditable = true
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "app.version",
                        ConfigValue = "1.0.0",
                        Description = "Application version",
                        Category = "general",
                        DataType = "string",
                        IsEditable = false
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "email.provider",
                        ConfigValue = "smtp",
                        Description = "Email service provider",
                        Category = "email",
                        DataType = "string",
                        IsEditable = true
                    },
                    new SystemConfiguration
                    {
                        ConfigKey = "backup.enabled",
                        ConfigValue = "true",
                        Description = "Enable automatic backups",
                        Category = "backup",
                        DataType = "boolean",
                        IsEditable = true
                    }
                };

                await _configCollection.InsertManyAsync(defaultConfigs);
            }
        }

        private static SystemConfigDto MapToDto(SystemConfiguration config)
        {
            return new SystemConfigDto
            {
                Id = config.Id,
                ConfigKey = config.ConfigKey,
                ConfigValue = config.ConfigValue,
                Description = config.Description,
                DataType = config.DataType,
                Category = config.Category,
                IsEncrypted = config.IsEncrypted,
                IsEditable = config.IsEditable,
                ValidationRules = config.ValidationRules
            };
        }

        private static string GenerateApiKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string MaskApiKey(string keyValue)
        {
            if (string.IsNullOrEmpty(keyValue) || keyValue.Length <= 8)
                return "••••••••";
                
            return keyValue.Substring(0, 4) + "••••••••" + keyValue.Substring(keyValue.Length - 4);
        }
    }
}
