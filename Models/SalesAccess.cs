using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class SalesAccess
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("CompanyId")]
        public string CompanyId { get; set; } = null!;

        [BsonElement("salesAccess")]
        public int SalesAccessValue { get; set; }
    }
} 