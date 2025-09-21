using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    [BsonIgnoreExtraElements] 
    public class Customerr
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null!;   

        [BsonElement("CompanyId")]
public string? CompanyId { get; set; }
  

        [BsonElement("customer_id")]
        public string? Customer_id { get; set; }
        
        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("active_date")]
        public string?  Active_date { get; set; }

        [BsonElement("estimate_date")]
        public string? Estimate_date { get; set; }

        [BsonElement("location")]
        public string? Location { get; set; }

        [BsonElement("status")]
        public string? Status { get; set; }
    }
}
