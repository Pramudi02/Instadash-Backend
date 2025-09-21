using MongoDB.Driver;
using Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class FinanceService
    {
        private readonly IMongoCollection<Finance> _financeCollection;

        public FinanceService(IMongoDatabase database)
        {
            _financeCollection = database.GetCollection<Finance>("finance");
        }

        // Get all finances
        public async Task<List<Finance>> GetAsync()
        {
            return await _financeCollection.Find(f => true).ToListAsync();
        }

        // Get a finance by id
        public async Task<Finance> GetByIdAsync(string id)
        {
            return await _financeCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
        }

        // Get finance data by companyId
public async Task<List<Finance>> GetByCompanyIdAsync(string companyId)
{
    return await _financeCollection.Find(f => f.CompanyId == companyId).ToListAsync();
}



        // Create a new finance entry
        public async Task CreateAsync(Finance finance)
        {
            await _financeCollection.InsertOneAsync(finance);
        }

        // Update an existing finance entry
        public async Task UpdateAsync(string id, Finance finance)
        {
            await _financeCollection.ReplaceOneAsync(f => f.Id == id, finance);
        }

        // Delete a finance entry by id
        public async Task DeleteAsync(string id)
        {
            await _financeCollection.DeleteOneAsync(f => f.Id == id);
        }
    }
}
