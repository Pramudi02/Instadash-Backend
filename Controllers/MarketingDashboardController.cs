using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class MarketingDashboardController : ControllerBase
{
    private readonly MongoService _mongo;

    public MarketingDashboardController(MongoService mongo) => _mongo = mongo;

    // Dashboard stats for pie charts
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var campaigns = await _mongo.Campaigns.Find(_ => true).ToListAsync();

        int totalCampaigns = campaigns.Count;
        double totalSpent = campaigns.Sum(c => c.SpentAmount);
        int totalVisitors = campaigns.Sum(c => c.NoOfVisitors);
        int totalCustomers = campaigns.Sum(c => c.NoOfCustomers);

        // Pie chart example values (customize as needed)
        int totalOrder = totalCampaigns;
        int customerGrowth = totalCustomers;
        double totalRevenue = totalSpent;

        int totalPie = totalOrder + customerGrowth + (int)totalRevenue;
        double orderPercent = totalPie > 0 ? (double)totalOrder / totalPie * 100 : 0;
        double growthPercent = totalPie > 0 ? (double)customerGrowth / totalPie * 100 : 0;
        double revenuePercent = totalPie > 0 ? (double)totalRevenue / totalPie * 100 : 0;

        return Ok(new
        {
            totalCampaigns,
            totalSpent,
            totalVisitors,
            totalCustomers,
            pie = new {
                totalOrder,
                customerGrowth,
                totalRevenue,
                orderPercent,
                growthPercent,
                revenuePercent
            }
        });
    }

    // Number of campaigns
    [HttpGet("campaigns/count")]
    public async Task<IActionResult> GetCampaignCount()
    {
        var count = await _mongo.Campaigns.CountDocumentsAsync(_ => true);
        return Ok(count);
    }

    // Total spent amount
    [HttpGet("campaigns/spent")]
    public async Task<IActionResult> GetTotalSpent()
    {
        var campaigns = await _mongo.Campaigns.Find(_ => true).ToListAsync();
        double totalSpent = campaigns.Sum(c => c.SpentAmount);
        return Ok(totalSpent);
    }

    // Total visitors
    [HttpGet("campaigns/visitors")]
    public async Task<IActionResult> GetTotalVisitors()
    {
        var campaigns = await _mongo.Campaigns.Find(_ => true).ToListAsync();
        int totalVisitors = campaigns.Sum(c => c.NoOfVisitors);
        return Ok(totalVisitors);
    }

    // Total customers
    [HttpGet("campaigns/customers")]
    public async Task<IActionResult> GetTotalCustomers()
    {
        var campaigns = await _mongo.Campaigns.Find(_ => true).ToListAsync();
        int totalCustomers = campaigns.Sum(c => c.NoOfCustomers);
        return Ok(totalCustomers);
    }
}