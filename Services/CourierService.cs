using System.Collections.Generic; // Provides generic collection types like List<T>.
using System.Threading.Tasks; // Enables asynchronous programming.
using Backend.Models; // Imports the Courier model.
using MongoDB.Driver; // Provides MongoDB client interfaces.
using System.Linq; // Provides LINQ methods like Count and FirstOrDefault.
using System;

namespace Backend.Services
{
    public class CourierService
    {
        private readonly IMongoCollection<Courier> _courierCollection; // Reference to the "courier" MongoDB collection.

        public CourierService(IMongoDatabase database)
        {
            _courierCollection = database.GetCollection<Courier>("courier"); // Initializes the MongoDB collection.
        }

        public async Task<List<Courier>> GetAllAsync()
        {
            return await _courierCollection.Find(_ => true).ToListAsync(); // Returns all courier documents.
        }

        public async Task<List<Courier>> GetAllByCompanyIdAsync(string companyId)
        {
            var filter = Builders<Courier>.Filter.Eq("CompanyId", companyId);
            return await _courierCollection.Find(filter).ToListAsync(); // Returns all couriers for a specific company.
        }

        public async Task<Courier> GetByIdAsync(string id)
        {
            return await _courierCollection.Find(c => c.Id == id).FirstOrDefaultAsync(); // Finds courier by ID or returns null.
        }

        public async Task CreateAsync(Courier courier)
        {
            await _courierCollection.InsertOneAsync(courier); // Inserts a new courier document into the collection.
        }

        public async Task UpdateAsync(string id, Courier updatedCourier)
        {
            await _courierCollection.ReplaceOneAsync(c => c.Id == id, updatedCourier); // Replaces the document with the given ID.
        }

        public async Task DeleteAsync(string id)
        {
            await _courierCollection.DeleteOneAsync(c => c.Id == id); // Deletes the courier document with the specified ID.
        }

        public async Task<List<Courier>> GetRecentDeliveriesAsync(int count)
        {
            return await _courierCollection
                .Find(_ => true)
                .SortByDescending(c => c.Date)
                .Limit(count)
                .ToListAsync(); // Returns most recent deliveries limited by count.
        }

        public async Task<List<Courier>> GetRecentByCompanyIdAsync(int count, string companyId)
        {
            var filter = Builders<Courier>.Filter.Eq("CompanyId", companyId);
            return await _courierCollection
                .Find(filter)
                .SortByDescending(c => c.Date)
                .Limit(count)
                .ToListAsync(); // Returns most recent deliveries for a specific company, limited by count.
        }

        public async Task<CourierSummaryDto> GetSummaryAsync(DateTime from, DateTime to, string? companyId = null)
        {
            var filterBuilder = Builders<Courier>.Filter;
            var filter = filterBuilder.Gte(c => c.Date, from) & filterBuilder.Lte(c => c.Date, to);
            if (!string.IsNullOrEmpty(companyId))
                filter &= filterBuilder.Eq(c => c.CompanyId, companyId);

            var courier = await _courierCollection.Find(filter).ToListAsync(); // Retrieves couriers within date range.
            return new CourierSummaryDto
            {
                Total = courier.Count, // Total couriers in the range.
                Pending = courier.Count(c => c.Status == "pending"), // Count of couriers with "pending" status.
                Completed = courier.Count(c => c.Status == "completed"), // Count of completed deliveries.
                Rejected = courier.Count(c => c.Status == "rejected") // Count of rejected or failed deliveries.
            };
        }

        public async Task<List<object>> GetTopCountriesAsync(string companyId)
        {
            var filter = Builders<Courier>.Filter.Eq("CompanyId", companyId);
            var couriers = await _courierCollection.Find(filter).ToListAsync();
            
            // Group by destination (assuming destination contains country info)
            var countryGroups = couriers
                .Where(c => !string.IsNullOrEmpty(c.Destination))
                .GroupBy(c => ExtractCountry(c.Destination!))
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(3)
                .ToList();

            // Calculate percentages
            int total = countryGroups.Sum(g => g.Count);
            
            return countryGroups
                .Select(x => new { 
                    name = x.Country,
                    percentage = total > 0 ? Math.Round((x.Count * 100.0) / total, 2) : 0
                })
                .ToList<object>();
        }

        // Helper method to extract country from address
        private string ExtractCountry(string destination)
        {
            // This is a simplified implementation - in a real app you would have more
            // sophisticated parsing logic based on your data format
            
            // Assuming destination might contain country at the end after a comma
            if (string.IsNullOrEmpty(destination)) return string.Empty;
            
            var parts = destination.Split(',');
            if (parts.Length > 0)
            {
                // Try to get the last part as country and trim whitespace
                return parts[parts.Length - 1].Trim();
            }
            
            return destination.Trim();
        }

        // Create test data for a company (for testing purposes)
        public async Task CreateTestDataForCompanyAsync(string companyId)
        {
            // Check if company already has courier data
            var existingData = await GetAllByCompanyIdAsync(companyId);
            if (existingData.Count > 0)
            {
                Console.WriteLine($"Company {companyId} already has {existingData.Count} courier records");
                return;
            }

            var testCouriers = new List<Courier>
            {
                new Courier
                {
                    OrderId = "ORD001",
                    CourierId = "COR001",
                    Destination = "123 Main St, Colombo, Sri Lanka",
                    Date = DateTime.UtcNow.AddDays(-5),
                    EstimateDate = DateTime.UtcNow.AddDays(-2),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD002",
                    CourierId = "COR002",
                    Destination = "456 Broadway, New York, United States",
                    Date = DateTime.UtcNow.AddDays(-4),
                    EstimateDate = DateTime.UtcNow.AddDays(-1),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD003",
                    CourierId = "COR003",
                    Destination = "789 Queen St, Sydney, Australia",
                    Date = DateTime.UtcNow.AddDays(-3),
                    EstimateDate = DateTime.UtcNow.AddDays(1),
                    Status = "pending",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD004",
                    CourierId = "COR004",
                    Destination = "321 King St, Toronto, Canada",
                    Date = DateTime.UtcNow.AddDays(-2),
                    EstimateDate = DateTime.UtcNow.AddDays(2),
                    Status = "pending",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD005",
                    CourierId = "COR005",
                    Destination = "654 High St, London, United Kingdom",
                    Date = DateTime.UtcNow.AddDays(-1),
                    EstimateDate = DateTime.UtcNow.AddDays(3),
                    Status = "pending",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD006",
                    CourierId = "COR006",
                    Destination = "987 Central Ave, Kandy, Sri Lanka",
                    Date = DateTime.UtcNow,
                    EstimateDate = DateTime.UtcNow.AddDays(4),
                    Status = "pending",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD007",
                    CourierId = "COR007",
                    Destination = "147 Beach Rd, Perth, Australia",
                    Date = DateTime.UtcNow.AddDays(-6),
                    EstimateDate = DateTime.UtcNow.AddDays(-3),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD008",
                    CourierId = "COR008",
                    Destination = "258 State St, Chicago, United States",
                    Date = DateTime.UtcNow.AddDays(-7),
                    EstimateDate = DateTime.UtcNow.AddDays(-4),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD009",
                    CourierId = "COR009",
                    Destination = "101 Marina Bay, Singapore",
                    Date = DateTime.UtcNow.AddDays(-8),
                    EstimateDate = DateTime.UtcNow.AddDays(-5),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD010",
                    CourierId = "COR010",
                    Destination = "222 Shibuya, Tokyo, Japan",
                    Date = DateTime.UtcNow.AddDays(-9),
                    EstimateDate = DateTime.UtcNow.AddDays(-6),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD011",
                    CourierId = "COR011",
                    Destination = "333 Connaught Place, New Delhi, India",
                    Date = DateTime.UtcNow.AddDays(-10),
                    EstimateDate = DateTime.UtcNow.AddDays(-7),
                    Status = "completed",
                    CompanyId = companyId
                },
                new Courier
                {
                    OrderId = "ORD012",
                    CourierId = "COR012",
                    Destination = "444 Champs-Élysées, Paris, France",
                    Date = DateTime.UtcNow.AddDays(-11),
                    EstimateDate = DateTime.UtcNow.AddDays(-8),
                    Status = "completed",
                    CompanyId = companyId
                }
            };

            await _courierCollection.InsertManyAsync(testCouriers);
            Console.WriteLine($"Created {testCouriers.Count} test courier records for company {companyId}");
        }

        // Recreate test data for a company (deletes existing and creates new)
        public async Task RecreateTestDataForCompanyAsync(string companyId)
        {
            // Delete existing data for the company
            var filter = Builders<Courier>.Filter.Eq("CompanyId", companyId);
            await _courierCollection.DeleteManyAsync(filter);
            Console.WriteLine($"Deleted existing courier records for company {companyId}");

            // Create new test data
            await CreateTestDataForCompanyAsync(companyId);
        }
    }
}
