// Services/PasswordResetService.cs
using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
//using AuthAPI.Settings;

namespace Backend.Services
{
    public interface IPasswordResetService
    {
        Task<string> GenerateResetTokenAsync(string userId, string email);
        Task<PasswordReset?> ValidateResetTokenAsync(string token, string email);
        Task MarkTokenAsUsedAsync(string token);
        Task CleanupExpiredTokensAsync();
    }

    public class PasswordResetService : IPasswordResetService
    {
        private readonly IMongoCollection<PasswordReset> _passwordResetCollection;

        public PasswordResetService(IOptions<Backend.Models.MongoDBSettings> mongoDbSettings)
        {
            var settings = mongoDbSettings.Value;
            var mongoClient = new MongoClient(settings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.UserDetailsdatabase);
            _passwordResetCollection = mongoDatabase.GetCollection<PasswordReset>(settings.PasswordResetCollectionName);
        }

        public async Task<string> GenerateResetTokenAsync(string userId, string email)
        {
            // Invalidate any existing tokens for this user
            await _passwordResetCollection.UpdateManyAsync(
                pr => pr.UserId == userId && !pr.IsUsed,
                Builders<PasswordReset>.Update.Set(pr => pr.IsUsed, true)
            );

            // Generate secure token
            var token = GenerateSecureToken();

            var passwordReset = new PasswordReset
            {
                UserId = userId,
                Email = email,
                ResetToken = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetCollection.InsertOneAsync(passwordReset);
            return token;
        }

        public async Task<PasswordReset?> ValidateResetTokenAsync(string token, string email)
        {
            var passwordReset = await _passwordResetCollection
                .Find(pr => pr.ResetToken == token && 
                           pr.Email == email && 
                           !pr.IsUsed && 
                           pr.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return passwordReset;
        }

        public async Task MarkTokenAsUsedAsync(string token)
        {
            await _passwordResetCollection.UpdateOneAsync(
                pr => pr.ResetToken == token,
                Builders<PasswordReset>.Update
                    .Set(pr => pr.IsUsed, true)
                    .Set(pr => pr.UsedAt, DateTime.UtcNow)
            );
        }

        public async Task CleanupExpiredTokensAsync()
        {
            await _passwordResetCollection.DeleteManyAsync(
                pr => pr.ExpiresAt < DateTime.UtcNow || pr.IsUsed
            );
        }

        private static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}