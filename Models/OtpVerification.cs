using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class OtpVerification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("otpCode")]
        public string OtpCode { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("isUsed")]
        public bool IsUsed { get; set; } = false;

        [BsonElement("attempts")]
        public int Attempts { get; set; } = 0;

        [BsonElement("maxAttempts")]
        public int MaxAttempts { get; set; } = 3;

        [BsonElement("purpose")]
        public string Purpose { get; set; } = "SIGNUP"; // SIGNUP, PASSWORD_RESET, etc.

        [BsonElement("tempUserData")]
        public string TempUserData { get; set; } = string.Empty; // Store temporary user data as JSON

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired && Attempts < MaxAttempts;
    }

    public class TempUserData
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string HoneyCombId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
    }

    public class OtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = "SIGNUP";
    }

    public class OtpVerifyDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = "SIGNUP";
    }
}
