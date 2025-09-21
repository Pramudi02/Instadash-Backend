using Backend.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CustomerService
    {
        private readonly IMongoCollection<Customerr> _customers;

        public CustomerService(IMongoDatabase database)
        {
            _customers = database.GetCollection<Customerr>("customers");
        }

        public async Task<Customerr?> GetByCustomerIdAsync(string customerId)
        {
            return await _customers.Find(c => c.Customer_id == customerId).FirstOrDefaultAsync();
        }
    }
}
