using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
//using AuthAPI.Settings;
using Backend.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend.Services
{
    public class HoneycombService
    {
        private readonly IMongoCollection<HoneycombUser> _honeycombCollection;

        public HoneycombService(IOptions<Backend.Models.MongoDBSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.AdminDatabaseName); 
            _honeycombCollection = mongoDatabase.GetCollection<HoneycombUser>(mongoDbSettings.Value.HoneyCombCollectionName);
        }

        public async Task<HoneycombUser> GetByHoneycombIdAsync(string honeycombId)
        {
            return await _honeycombCollection
                .Find(u => u.HoneyCombId == honeycombId)
                .FirstOrDefaultAsync();
        }

        public async Task<HoneycombUser> GetByEmailAsync(string email)
        {
            return await _honeycombCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ValidateHoneycombRegistration(string honeycombId, string email)
        {
            var honeycombUser = await GetByHoneycombIdAsync(honeycombId);
            
            if (honeycombUser == null)
                return false;
                
            // Verify email matches the honeycomb ID record
            return honeycombUser.Email.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        
        public async Task<string> GetUserRole(string honeycombId)
        {
            var honeycombUser = await GetByHoneycombIdAsync(honeycombId);
            return honeycombUser?.Roles ?? "user";
        }
    }

    // Model for the honeycomb collection
    public class HoneycombUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Email { get; set; }
        
        public string CompanyId { get; set; }
        
        public string HoneyCombId { get; set; }
        
        public string Roles { get; set; }
    }
}