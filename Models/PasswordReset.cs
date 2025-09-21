
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class PasswordReset
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;
        
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
        
        [BsonElement("resetToken")]
        public string ResetToken { get; set; } = string.Empty;
        
        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }
        
        [BsonElement("isUsed")]
        public bool IsUsed { get; set; } = false;
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("usedAt")]
        public DateTime? UsedAt { get; set; }
    }
}