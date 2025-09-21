using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class DeviceCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("deviceType")]
        public string DeviceType { get; set; } = string.Empty; // Mobile, Desktop, Tablet

        [BsonElement("sessionCount")]
        public int SessionCount { get; set; }

        [BsonElement("orderCount")]
        public int OrderCount { get; set; }

        [BsonElement("revenue")]
        public decimal Revenue { get; set; }

        [BsonElement("companyId")]
        public string? CompanyId { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("userAgent")]
        public string? UserAgent { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DeviceCategoryStats
    {
        public string DeviceType { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public decimal Percentage { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
