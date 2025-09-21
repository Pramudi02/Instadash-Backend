using Backend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CampaignService
    {
        private readonly IMongoCollection<Campaign> _campaigns;

        // Inject the shared IMongoDatabase directly
        public CampaignService(IMongoDatabase database)
        {
            _campaigns = database.GetCollection<Campaign>("campaigns");
        }

        public async Task<List<Campaign>> GetAllAsync() =>
            await _campaigns.Find(_ => true).ToListAsync();
            
        public async Task<List<Campaign>> GetAllByCompanyIdAsync(string companyId)
        {
            // Case-insensitive regex filter for better matching
            var regexFilter = Builders<Campaign>.Filter.Regex(c => c.CompanyId, new MongoDB.Bson.BsonRegularExpression($"^{companyId}$", "i"));
            var campaigns = await _campaigns.Find(regexFilter).ToListAsync();
            
            Console.WriteLine($"Found {campaigns.Count} campaigns for company ID: {companyId}");
            
            // If no campaigns found with regex, try exact match as fallback
            if (campaigns.Count == 0)
            {
                var exactFilter = Builders<Campaign>.Filter.Eq(c => c.CompanyId, companyId);
                campaigns = await _campaigns.Find(exactFilter).ToListAsync();
                Console.WriteLine($"Found {campaigns.Count} campaigns with exact match for company ID: {companyId}");
            }
            
            return campaigns;
        }
        
        // Get all distinct CompanyId values in the campaigns collection
        public async Task<List<string>> GetDistinctCompanyIdsAsync()
        {
            var companyIds = await _campaigns.Distinct<string>("CompanyId", FilterDefinition<Campaign>.Empty).ToListAsync();
            return companyIds.Where(id => !string.IsNullOrEmpty(id)).ToList();
        }
        
        // Ensure test data exists for company C00002
        public async Task EnsureTestDataExistsAsync()
        {
            var companyId = "C00002";
            await RecreateTestDataForCompanyAsync(companyId);
        }
        
        // Recreate test campaign data for any company ID (removes existing and creates new)
        public async Task RecreateTestDataForCompanyAsync(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                Console.WriteLine("Cannot recreate test data for null or empty company ID");
                return;
            }
            
            // First, remove existing campaigns for this company
            var filter = Builders<Campaign>.Filter.Eq(c => c.CompanyId, companyId);
            var existingCount = await _campaigns.CountDocumentsAsync(filter);
            
            if (existingCount > 0)
            {
                Console.WriteLine($"Removing {existingCount} existing campaigns for company {companyId}");
                await _campaigns.DeleteManyAsync(filter);
            }
            
            // Now create new test data
            await CreateTestDataForCompanyAsync(companyId);
        }
        
        // Create test campaign data for any company ID
        public async Task CreateTestDataForCompanyAsync(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                Console.WriteLine("Cannot create test data for null or empty company ID");
                return;
            }
            
            var filter = Builders<Campaign>.Filter.Eq(c => c.CompanyId, companyId);
            var count = await _campaigns.CountDocumentsAsync(filter);
            
            if (count == 0)
            {
                Console.WriteLine($"Creating test campaign data for company {companyId}");
                
                // Use the last two characters of the company ID to add some variance to the test data
                var variance = companyId.Length >= 2 ? int.Parse(companyId.Substring(companyId.Length - 2).Replace("C", "1")) : 10;
                
                var testCampaigns = new List<Campaign>
                {
                    new Campaign
                    {
                        CamId = $"CMP{companyId}1",
                        Platform = "TikTok",
                        Description = $"TikTok Engagement Boost - {companyId}",
                        ClickThroughRate = 0.13,
                        Cpc = 12.12 + (variance * 0.1),
                        SpentAmount = 845123.12 + (variance * 100),
                        NoOfVisitors = 287000 + (variance * 1000),
                        NoOfCustomers = 37310 + (variance * 100),
                        Date = DateTime.UtcNow,
                        CompanyId = companyId
                    },
                    new Campaign
                    {
                        CamId = $"CMP{companyId}2",
                        Platform = "Facebook",
                        Description = $"Facebook Promo Campaign - {companyId}",
                        ClickThroughRate = 0.08,
                        Cpc = 5.45 + (variance * 0.05),
                        SpentAmount = 65432.21 + (variance * 200),
                        NoOfVisitors = 156000 + (variance * 1200),
                        NoOfCustomers = 12480 + (variance * 120),
                        Date = DateTime.UtcNow,
                        CompanyId = companyId
                    },
                    new Campaign
                    {
                        CamId = $"CMP{companyId}3",
                        Platform = "Google",
                        Description = $"Google Search Ads - {companyId}",
                        ClickThroughRate = 0.15,
                        Cpc = 7.89 + (variance * 0.08),
                        SpentAmount = 120876.43 + (variance * 150),
                        NoOfVisitors = 412000 + (variance * 1500),
                        NoOfCustomers = 61800 + (variance * 180),
                        Date = DateTime.UtcNow,
                        CompanyId = companyId
                    },
                    new Campaign
                    {
                        CamId = $"CMP{companyId}4",
                        Platform = "Instagram",
                        Description = $"Instagram Stories Campaign - {companyId}",
                        ClickThroughRate = 0.11,
                        Cpc = 8.23 + (variance * 0.06),
                        SpentAmount = 89543.67 + (variance * 180),
                        NoOfVisitors = 198000 + (variance * 800),
                        NoOfCustomers = 21780 + (variance * 90),
                        Date = DateTime.UtcNow,
                        CompanyId = companyId
                    },
                    new Campaign
                    {
                        CamId = $"CMP{companyId}5",
                        Platform = "LinkedIn",
                        Description = $"LinkedIn B2B Campaign - {companyId}",
                        ClickThroughRate = 0.09,
                        Cpc = 15.67 + (variance * 0.12),
                        SpentAmount = 156789.34 + (variance * 250),
                        NoOfVisitors = 98000 + (variance * 600),
                        NoOfCustomers = 8820 + (variance * 60),
                        Date = DateTime.UtcNow,
                        CompanyId = companyId
                    },
                    new Campaign
                    {
                        CamId = $"CMP{companyId}6",
                        Platform = "Twitter",
                        Description = $"Twitter Promoted Tweets - {companyId}",
                        ClickThroughRate = 0.07,
                        Cpc = 6.78 + (variance * 0.04),
                        SpentAmount = 42156.89 + (variance * 120),
                        NoOfVisitors = 234000 + (variance * 900),
                        NoOfCustomers = 16380 + (variance * 75),
                        Date = DateTime.UtcNow,
                        CompanyId = companyId
                    }
                };
                
                await _campaigns.InsertManyAsync(testCampaigns);
                Console.WriteLine($"Created {testCampaigns.Count} test campaigns for company {companyId}");
            }
            else
            {
                Console.WriteLine($"Found {count} existing campaigns for company {companyId}, no need to create test data");
            }
        }
    }
}
