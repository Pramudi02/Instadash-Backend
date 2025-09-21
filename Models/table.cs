using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    [BsonIgnoreExtraElements] 
    public class Table
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("date")]
        public string?  Date { get; set; }

        [BsonElement("audit_id")]
        public string? AuditId { get; set; }

        [BsonElement("sales_id")]
        [JsonPropertyName("salesId")]
        public string? SalesId { get; set; }

        [BsonElement("names")]
        public string? Name { get; set; }

        [BsonElement("value")]
        public double? Value { get; set; }  

        [BsonElement("tax")]
        public double? Tax { get; set; }

        [BsonElement("netvalue")]
        public double? NetValue { get; set; }
    }
}
