using AuthAPI.Models;
//using AuthAPI.Settings;
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class UserManagementService
{
    private readonly IMongoCollection<User> _users;

    public UserManagementService(IOptions<Backend.Models.MongoDBSettings> mongoSettings)
    {
        var settings = mongoSettings.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.AdminDatabaseName);
        _users = database.GetCollection<User>(settings.UsersCollectionName);
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _users.Find(user => true).ToListAsync();
    }

    public async Task<bool> DeleteUserByEmailAsync(string username)
    {
        var result = await _users.DeleteOneAsync(user => user.Username == username);
        return result.DeletedCount > 0;
    }

    

    public async Task<User> CreateUserAsync(User user)
    {
        // Check if user with this email already exists
        var existingUser = await _users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return null; // Email already in use
        }

        await _users.InsertOneAsync(user);
        return user;
    }



}
