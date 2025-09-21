using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication
    public class SystemConfigurationController : ControllerBase
    {
        private readonly ISystemConfigurationService _systemConfigService;
        private readonly ILogger<SystemConfigurationController> _logger;

        public SystemConfigurationController(
            ISystemConfigurationService systemConfigService,
            ILogger<SystemConfigurationController> logger)
        {
            _systemConfigService = systemConfigService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        }

        // GET: api/systemconfiguration
        [HttpGet]
        public async Task<IActionResult> GetSystemConfigurationSummary()
        {
            try
            {
                _logger.LogDebug("Getting system configuration summary");
                var summary = await _systemConfigService.GetSystemConfigurationSummaryAsync();
                _logger.LogDebug("System configuration summary retrieved successfully");
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system configuration summary: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Error retrieving system configuration: {ex.Message}" });
            }
        }

        // GET: api/systemconfiguration/category/{category}
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetConfigurationsByCategory(string category)
        {
            try
            {
                var configurations = await _systemConfigService.GetConfigurationsByCategoryAsync(category);
                return Ok(new { success = true, data = configurations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configurations for category: {Category}", category);
                return StatusCode(500, new { success = false, message = "Error retrieving configurations" });
            }
        }

        // GET: api/systemconfiguration/{configKey}
        [HttpGet("{configKey}")]
        public async Task<IActionResult> GetConfiguration(string configKey)
        {
            try
            {
                var configuration = await _systemConfigService.GetConfigurationAsync(configKey);
                
                if (configuration == null)
                {
                    return NotFound(new { success = false, message = "Configuration not found" });
                }

                return Ok(new { success = true, data = configuration });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration: {ConfigKey}", configKey);
                return StatusCode(500, new { success = false, message = "Error retrieving configuration" });
            }
        }

        // PUT: api/systemconfiguration/{configKey}
        [HttpPut("{configKey}")]
        public async Task<IActionResult> UpdateConfiguration(string configKey, [FromBody] UpdateSystemConfigDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
                }

                var updatedConfig = await _systemConfigService.UpdateConfigurationAsync(
                    configKey, updateDto, GetCurrentUserEmail());

                _logger.LogInformation("Configuration updated: {ConfigKey} by {User}", configKey, GetCurrentUserEmail());
                
                return Ok(new { success = true, data = updatedConfig, message = "Configuration updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration: {ConfigKey}", configKey);
                return StatusCode(500, new { success = false, message = "Error updating configuration" });
            }
        }

        // POST: api/systemconfiguration
        [HttpPost]
        public async Task<IActionResult> CreateConfiguration([FromBody] SystemConfigDto configDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
                }

                var createdConfig = await _systemConfigService.CreateConfigurationAsync(configDto, GetCurrentUserEmail());
                
                _logger.LogInformation("Configuration created: {ConfigKey} by {User}", configDto.ConfigKey, GetCurrentUserEmail());
                
                return CreatedAtAction(nameof(GetConfiguration), new { configKey = createdConfig.ConfigKey }, 
                    new { success = true, data = createdConfig, message = "Configuration created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration: {ConfigKey}", configDto.ConfigKey);
                return StatusCode(500, new { success = false, message = "Error creating configuration" });
            }
        }

        // DELETE: api/systemconfiguration/{configKey}
        [HttpDelete("{configKey}")]
        public async Task<IActionResult> DeleteConfiguration(string configKey)
        {
            try
            {
                var deleted = await _systemConfigService.DeleteConfigurationAsync(configKey);
                
                if (!deleted)
                {
                    return NotFound(new { success = false, message = "Configuration not found" });
                }

                _logger.LogInformation("Configuration deleted: {ConfigKey} by {User}", configKey, GetCurrentUserEmail());
                
                return Ok(new { success = true, message = "Configuration deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration: {ConfigKey}", configKey);
                return StatusCode(500, new { success = false, message = "Error deleting configuration" });
            }
        }

        // Security Settings Endpoints

        // GET: api/systemconfiguration/security
        [HttpGet("security")]
        public async Task<IActionResult> GetSecuritySettings()
        {
            try
            {
                var securitySettings = await _systemConfigService.GetSecuritySettingsAsync();
                return Ok(new { success = true, data = securitySettings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security settings");
                return StatusCode(500, new { success = false, message = "Error retrieving security settings" });
            }
        }

        // PUT: api/systemconfiguration/security
        [HttpPut("security")]
        public async Task<IActionResult> UpdateSecuritySettings([FromBody] SecuritySettingsDto securityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
                }

                var updatedSettings = await _systemConfigService.UpdateSecuritySettingsAsync(securityDto, GetCurrentUserEmail());
                
                _logger.LogInformation("Security settings updated by {User}", GetCurrentUserEmail());
                
                return Ok(new { success = true, data = updatedSettings, message = "Security settings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security settings");
                return StatusCode(500, new { success = false, message = "Error updating security settings" });
            }
        }

        // API Key Management Endpoints

        // GET: api/systemconfiguration/apikeys
        [HttpGet("apikeys")]
        public async Task<IActionResult> GetApiKeys()
        {
            try
            {
                var apiKeys = await _systemConfigService.GetApiKeysAsync();
                return Ok(new { success = true, data = apiKeys });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API keys");
                return StatusCode(500, new { success = false, message = "Error retrieving API keys" });
            }
        }

        // POST: api/systemconfiguration/apikeys
        [HttpPost("apikeys")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
                }

                var apiKey = await _systemConfigService.CreateApiKeyAsync(createDto, GetCurrentUserEmail());
                
                _logger.LogInformation("API key created: {KeyName} by {User}", createDto.KeyName, GetCurrentUserEmail());
                
                return CreatedAtAction(nameof(GetApiKeys), 
                    new { success = true, data = apiKey, message = "API key created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key: {KeyName}", createDto.KeyName);
                return StatusCode(500, new { success = false, message = "Error creating API key" });
            }
        }

        // PUT: api/systemconfiguration/apikeys/{keyId}/regenerate
        [HttpPut("apikeys/{keyId}/regenerate")]
        public async Task<IActionResult> RegenerateApiKey(string keyId)
        {
            try
            {
                var regenerated = await _systemConfigService.RegenerateApiKeyAsync(keyId, GetCurrentUserEmail());
                
                if (!regenerated)
                {
                    return NotFound(new { success = false, message = "API key not found" });
                }

                _logger.LogInformation("API key regenerated: {KeyId} by {User}", keyId, GetCurrentUserEmail());
                
                return Ok(new { success = true, message = "API key regenerated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating API key: {KeyId}", keyId);
                return StatusCode(500, new { success = false, message = "Error regenerating API key" });
            }
        }

        // DELETE: api/systemconfiguration/apikeys/{keyId}
        [HttpDelete("apikeys/{keyId}")]
        public async Task<IActionResult> RevokeApiKey(string keyId)
        {
            try
            {
                var revoked = await _systemConfigService.RevokeApiKeyAsync(keyId);
                
                if (!revoked)
                {
                    return NotFound(new { success = false, message = "API key not found" });
                }

                _logger.LogInformation("API key revoked: {KeyId} by {User}", keyId, GetCurrentUserEmail());
                
                return Ok(new { success = true, message = "API key revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key: {KeyId}", keyId);
                return StatusCode(500, new { success = false, message = "Error revoking API key" });
            }
        }

        // System Health Endpoint

        // GET: api/systemconfiguration/health
        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var health = await _systemConfigService.GetSystemHealthAsync();
                return Ok(new { success = true, data = health });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system health");
                return StatusCode(500, new { success = false, message = "Error retrieving system health" });
            }
        }

        // Validate API Key Endpoint (for external use)

        // POST: api/systemconfiguration/apikeys/validate
        [HttpPost("apikeys/validate")]
        [AllowAnonymous] // Allow anonymous access for API key validation
        public async Task<IActionResult> ValidateApiKey([FromBody] ValidateApiKeyDto validateDto)
        {
            try
            {
                if (string.IsNullOrEmpty(validateDto.ApiKey))
                {
                    return BadRequest(new { success = false, message = "API key is required" });
                }

                var isValid = await _systemConfigService.ValidateApiKeyAsync(validateDto.ApiKey);
                
                return Ok(new { success = true, valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return StatusCode(500, new { success = false, message = "Error validating API key" });
            }
        }
    }

    public class ValidateApiKeyDto
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
