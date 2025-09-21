using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class UserDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        [BsonElement("userId")]
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [BsonElement("firstName")]
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [BsonElement("lastName")]
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [BsonElement("email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [BsonElement("userName")]
        [Required]
        public string UserName { get; set; } = string.Empty;
        
        [BsonElement("phoneCountryCode")]
        public string PhoneCountryCode { get; set; } = "+94";
        
        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [BsonElement("dateOfBirth")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DateOfBirth { get; set; }
        
        [BsonElement("country")]
        public string Country { get; set; } = string.Empty;
        
        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;
        
        [BsonElement("city")]
        public string City { get; set; } = string.Empty;
        
        [BsonElement("profileImage")]
        public string? ProfileImage { get; set; }
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProfileImageUrlRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}