using Backend.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class AutomationService
    {
        private readonly IMongoCollection<Automation> _automationCollection;

        public AutomationService(IOptions<Backend.Models.MongoDBSettings> mongoSettings)
        {
            var settings = mongoSettings.Value;

            var mongoClient = new MongoClient(settings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName); // or AdminDatabaseName if you want

            _automationCollection = mongoDatabase.GetCollection<Automation>("automation"); // Replace with correct name if needed
        }

        public async Task<List<Automation>> GetAllAsync()
        {
            return await _automationCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Automation?> GetByIdAsync(string id)
        {
            return await _automationCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Automation automation)
        {
            await _automationCollection.InsertOneAsync(automation);
        }

        public async Task UpdateAsync(string id, Automation automation)
        {
            await _automationCollection.ReplaceOneAsync(a => a.Id == id, automation);
        }

        public async Task DeleteAsync(string id)
        {
            await _automationCollection.DeleteOneAsync(a => a.Id == id);
        }
        public async Task<List<Automation>> GetByCompanyIdAsync(string companyId)
{
    return await _automationCollection.Find(a => a.CompanyId == companyId).ToListAsync();
}

    }
}
