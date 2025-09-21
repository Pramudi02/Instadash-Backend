using Backend.Models;
// using Backend.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Backend.Services
{
    public interface IUserDetailsService
    {
        Task<UserDetails?> GetByUserIdAsync(string userId);
        Task<UserDetails> CreateAsync(UserDetails userDetails);
        Task<UserDetails> UpdateAsync(string userId, UserDetails userDetails);
        Task<bool> DeleteAsync(string userId);
    }

    public class UserDetailsService : IUserDetailsService
    {
        private readonly IMongoCollection<UserDetails> _userDetailsCollection;

        public UserDetailsService(IOptions<Backend.Models.MongoDBSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.UserDetailsdatabase);
            _userDetailsCollection = mongoDatabase.GetCollection<UserDetails>(mongoDbSettings.Value.UserDetailsCollectionName);
        }

        public async Task<UserDetails?> GetByUserIdAsync(string userId)
        {
            return await _userDetailsCollection
                .Find(x => x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<UserDetails> CreateAsync(UserDetails userDetails)
        {
            userDetails.CreatedAt = DateTime.UtcNow;
            userDetails.UpdatedAt = DateTime.UtcNow;
            
            await _userDetailsCollection.InsertOneAsync(userDetails);
            return userDetails;
        }

        public async Task<UserDetails> UpdateAsync(string userId, UserDetails userDetails)
        {
            userDetails.UpdatedAt = DateTime.UtcNow;
            
            await _userDetailsCollection.ReplaceOneAsync(
                x => x.UserId == userId, 
                userDetails
            );
            
            return userDetails;
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var result = await _userDetailsCollection.DeleteOneAsync(x => x.UserId == userId);
            return result.DeletedCount > 0;
        }
    }
}