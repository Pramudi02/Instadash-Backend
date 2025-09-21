using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceCategoryController : ControllerBase
    {
        private readonly DeviceCategoryService _deviceCategoryService;

        public DeviceCategoryController(DeviceCategoryService deviceCategoryService)
        {
            _deviceCategoryService = deviceCategoryService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDeviceCategoryStats([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"DeviceCategoryController - GetDeviceCategoryStats - Received companyId: {companyId ?? "null"}");
            
            try
            {
                var stats = await _deviceCategoryService.GetDeviceCategoryStatsAsync(companyId);
                Console.WriteLine($"DeviceCategoryController - GetDeviceCategoryStats - Returning {stats.Count} device categories");
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeviceCategoryController - GetDeviceCategoryStats - Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving device category stats." });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDeviceCategorySummary([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"DeviceCategoryController - GetDeviceCategorySummary - Received companyId: {companyId ?? "null"}");
            
            try
            {
                var summary = await _deviceCategoryService.GetDeviceCategorySummaryAsync(companyId);
                Console.WriteLine($"DeviceCategoryController - GetDeviceCategorySummary - Returning summary data");
                
                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeviceCategoryController - GetDeviceCategorySummary - Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving device category summary." });
            }
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackDeviceSession([FromBody] DeviceCategory deviceData)
        {
            Console.WriteLine($"DeviceCategoryController - TrackDeviceSession - Received device: {deviceData.DeviceType} for company: {deviceData.CompanyId}");
            
            try
            {
                await _deviceCategoryService.TrackDeviceSessionAsync(deviceData);
                Console.WriteLine($"DeviceCategoryController - TrackDeviceSession - Successfully tracked device session");
                
                return Ok(new { message = "Device session tracked successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeviceCategoryController - TrackDeviceSession - Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while tracking device session." });
            }
        }

        [HttpPost("create-sample-data")]
        public async Task<IActionResult> CreateSampleData([FromQuery] string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
                return BadRequest("CompanyId is required");

            Console.WriteLine($"DeviceCategoryController - CreateSampleData - Creating sample data for company: {companyId}");
            
            try
            {
                await _deviceCategoryService.CreateSampleDeviceDataAsync(companyId);
                return Ok(new { message = $"Sample device category data created for company {companyId}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeviceCategoryController - CreateSampleData - Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while creating sample data." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeviceCategories([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"DeviceCategoryController - GetAllDeviceCategories - Received companyId: {companyId ?? "null"}");
            
            try
            {
                var deviceCategories = await _deviceCategoryService.GetAllDeviceCategoriesAsync(companyId);
                Console.WriteLine($"DeviceCategoryController - GetAllDeviceCategories - Returning {deviceCategories.Count} records");
                
                return Ok(deviceCategories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeviceCategoryController - GetAllDeviceCategories - Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving device categories." });
            }
        }
    }
}
