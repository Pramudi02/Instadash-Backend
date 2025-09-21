
using AuthAPI.Models;
//using AuthAPI.Settings;
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Services;
public class UserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(IOptions<Backend.Models.MongoDBSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.AdminDatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
    }

    public async Task<bool> GetAnyUser()
    {
        return await _usersCollection.Find(_ => true).AnyAsync();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        try
        {
            Console.WriteLine($"🔍 Searching for user with email: {email}");
            
            //  try to get the raw document to see what fields exist
            var filter = Builders<BsonDocument>.Filter.Eq("email", email);
            var collection = _usersCollection.Database.GetCollection<BsonDocument>(_usersCollection.CollectionNamespace.CollectionName);
            var rawDoc = await collection.Find(filter).FirstOrDefaultAsync();
            
            if (rawDoc != null)
            {
                Console.WriteLine($" Raw document fields: {string.Join(", ", rawDoc.Names)}");
                
                // Try to manually create User object from BsonDocument
                var user = new User
                {
                    Id = rawDoc.GetValue("_id", "").ToString(),
                    Username = rawDoc.GetValue("username", "").ToString(),
                    Email = rawDoc.GetValue("email", "").ToString(),
                    PasswordHash = rawDoc.GetValue("passwordHash", "").ToString(),
                    Salt = rawDoc.GetValue("salt", "").ToString(),
                    HoneyCombId = rawDoc.GetValue("honeyCombId", "").ToString(),
                    Roles = rawDoc.GetValue("roles", "user").ToString(),
                    CompanyId = rawDoc.GetValue("companyId", "").ToString(),
                    CreatedAt = rawDoc.GetValue("createdAt", DateTime.UtcNow).ToUniversalTime()
                };
                
                Console.WriteLine($"✅ User found: {user.Username}");
                return user;
            }
            else
            {
                Console.WriteLine($" No user found with email: {email}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in GetByEmailAsync: {ex.Message}");
            
            // Fallback: try without deserializing CreatedAt
            try
            {
                var projection = Builders<User>.Projection
                    .Include(u => u.Id)
                    .Include(u => u.Username)
                    .Include(u => u.Email)
                    .Include(u => u.PasswordHash)
                    .Include(u => u.Salt)
                    .Include(u => u.HoneyCombId)
                    .Include(u => u.Roles)
                    .Include(u => u.CompanyId)
                    .Exclude(u => u.CreatedAt);
                
                var user = await _usersCollection.Find(u => u.Email == email)
                    .Project<User>(projection)
                    .FirstOrDefaultAsync();
                
                if (user != null)
                {
                    user.CreatedAt = DateTime.UtcNow; // Set default value
                    Console.WriteLine($"✅ User found (fallback method): {user.Username}");
                }
                
                return user;
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($" Fallback method also failed: {fallbackEx.Message}");
                throw;
            }
        }
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        try
        {
            return await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in GetByUsernameAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<User> GetByRolesAsync(string role)
    {
        try
        {
            return await _usersCollection.Find(u => u.Roles == role).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in GetByRolesAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<User> GetByHoneyCombIdAsync(string honeycombId)
    {
        try
        {
            return await _usersCollection.Find(u => u.HoneyCombId == honeycombId).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in GetByHoneyCombIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            await _usersCollection.InsertOneAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in CreateAsync: {ex.Message}");
            throw;
        }
    }

    public (string passwordHash, string salt) HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var passwordHash = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        
        return (passwordHash, salt);
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        using var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt));
        var computedHash = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        
        return computedHash == storedHash;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in GetAllUsersAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<User> GetByIdAsync(string id)
    {
        try
        {
            return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in GetByIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error in UpdateUserAsync: {ex.Message}");
            throw;
        }
    }
}
