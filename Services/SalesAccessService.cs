using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class SalesAccessService
    {
        private readonly IMongoCollection<SalesAccess> _salesAccessCollection;

        public SalesAccessService(IOptions<Backend.Models.MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _salesAccessCollection = database.GetCollection<SalesAccess>("SalesAccess");
        }

        public async Task<List<SalesAccess>> GetAllAsync() =>
            await _salesAccessCollection.Find(_ => true).ToListAsync();

        public async Task<SalesAccess?> GetByCompanyIdAsync(string companyId) =>
            await _salesAccessCollection.Find(x => x.CompanyId == companyId).FirstOrDefaultAsync();

        public async Task CreateAsync(SalesAccess access) =>
            await _salesAccessCollection.InsertOneAsync(access);

        public async Task UpdateAsync(string id, SalesAccess access) =>
            await _salesAccessCollection.ReplaceOneAsync(x => x.Id == id, access);

        public async Task DeleteAsync(string id) =>
            await _salesAccessCollection.DeleteOneAsync(x => x.Id == id);
    }
} 