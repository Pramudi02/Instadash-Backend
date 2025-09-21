using Backend.Models;
using MongoDB.Driver;

namespace Backend.Services
{
    public class CustomerCountService
    {
        private readonly IMongoCollection<Customer> _customers;

        public CustomerCountService(IConfiguration config)
        {
            var mongoDbSettings = config.GetSection("MongoDBSettings").Get<MongoDBSettings>();
            var client = new MongoClient(mongoDbSettings.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.DatabaseName);
            _customers = database.GetCollection<Customer>("customers");
        }

        public async Task<long> GetCustomerCountAsync()
        {
            return await _customers.CountDocumentsAsync(FilterDefinition<Customer>.Empty);
        }

        public async Task<long> GetCustomerCountByCompanyAsync(string companyId)
        {
            var filter = Builders<Customer>.Filter.Eq(c => c.CompanyId, companyId);
            return await _customers.CountDocumentsAsync(filter);
        }

        public async Task<long> GetTotalCustomersAsync(string? companyId = null)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return await _customers.CountDocumentsAsync(FilterDefinition<Customer>.Empty);
            }
            else
            {
                var filter = Builders<Customer>.Filter.Eq(c => c.CompanyId, companyId);
                return await _customers.CountDocumentsAsync(filter);
            }
        }

        public async Task<long> GetLastMonthCustomersAsync(string? companyId = null)
        {
            // Since Customer model doesn't have a DateCreated field,
            // we'll return the total customer count for now
            // In a real application, you would have a proper date field to filter by
            
            Console.WriteLine("GetLastMonthCustomersAsync - Note: Customer model doesn't have DateCreated field, returning total count");
            
            if (string.IsNullOrEmpty(companyId))
            {
                return await _customers.CountDocumentsAsync(FilterDefinition<Customer>.Empty);
            }
            else
            {
                var filter = Builders<Customer>.Filter.Eq(c => c.CompanyId, companyId);
                return await _customers.CountDocumentsAsync(filter);
            }
        }
        }
}
