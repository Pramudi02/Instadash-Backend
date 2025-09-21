using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : ControllerBase
    {
        private readonly CampaignService _campaignService;

        public CampaignController(CampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        // Full campaign details
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var campaigns = await _campaignService.GetAllAsync();
            return Ok(campaigns);
        }
        
        // Test endpoint to debug company-specific campaigns
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompanyId(string companyId)
        {
            Console.WriteLine($"Testing campaign retrieval for company ID: {companyId}");
            var campaigns = await _campaignService.GetAllByCompanyIdAsync(companyId);
            return Ok(new { 
                companyId = companyId,
                campaignCount = campaigns.Count,
                campaigns = campaigns
            });
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetCampaignPerformance([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"GetCampaignPerformance called with companyId: {companyId ?? "null"}");
            
            // Ensure test data exists for commonly used company IDs
            await _campaignService.EnsureTestDataExistsAsync();
            
            // List all distinct company IDs for debugging
            var companyIds = await _campaignService.GetDistinctCompanyIdsAsync();
            Console.WriteLine($"Available company IDs in campaigns collection: {string.Join(", ", companyIds)}");
            
            // Get campaigns for the specified company
            var campaigns = string.IsNullOrEmpty(companyId) 
                ? await _campaignService.GetAllAsync() 
                : await _campaignService.GetAllByCompanyIdAsync(companyId);
            
            Console.WriteLine($"Retrieved {campaigns.Count} campaigns from database for company ID: {companyId ?? "all"}");
            
            // If no campaigns found for the specified company, try to create test data for it
            if (campaigns.Count == 0 && !string.IsNullOrEmpty(companyId))
            {
                Console.WriteLine($"No campaigns found for company ID {companyId}, creating test data for this company");
                await _campaignService.CreateTestDataForCompanyAsync(companyId);
                campaigns = await _campaignService.GetAllByCompanyIdAsync(companyId);
                Console.WriteLine($"Created and retrieved {campaigns.Count} test campaigns for company {companyId}");
            }
            
            // Transform to appropriate format
            var results = campaigns.Select(c => new {
                id = c.Id,
                name = c.Description ?? "Campaign " + c.CamId,
                impressions = FormatNumber(c.NoOfVisitors * 10), // Each visitor sees multiple impressions
                clicks = ((c.NoOfVisitors > 0 ? (double)c.NoOfCustomers / c.NoOfVisitors * 100 : 0)).ToString("0.##"),
                cpc = (c.NoOfCustomers > 0 ? c.SpentAmount / c.NoOfCustomers : 0).ToString("0.##"),
                spend = c.SpentAmount.ToString("#,##0.##"),
                icon = GetInitial(c.Description ?? c.Platform ?? "C"),
                color = GetColorForPlatform(c.Platform ?? "other"),
                companyId = c.CompanyId // Include company ID in response for debugging
            }).ToList();
            
            return Ok(results);
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
                return $"{(number / 1000000.0).ToString("0.#")}M";
            if (number >= 1000)
                return $"{(number / 1000.0).ToString("0.#")}k";
            return number.ToString();
        }

        private string GetInitial(string text)
        {
            return !string.IsNullOrEmpty(text) ? text.Substring(0, 1).ToUpper() : "C";
        }

        private string GetColorForPlatform(string platform)
        {
            return platform.ToLower() switch
            {
                "google" => "#34A853",
                "tiktok" => "#00F2EA",
                "instagram" => "#C13584",
                "facebook" => "#1877F2",
                "twitter" => "#1DA1F2",
                _ => "#" + new Random().Next(0x1000000).ToString("X6") // Random color
            };
        }
    }
}
