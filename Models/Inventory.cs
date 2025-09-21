using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    [BsonIgnoreExtraElements]
    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; } 

        [BsonElement("product_id")]
        public required string ProductId { get; set; } 

        [BsonElement("name")]
        public required string Name { get; set; } 

        [BsonElement("category")]
        public required string Category { get; set; } 

        [BsonElement("price")]
        public required double Price { get; set; }

        [BsonElement("stockQuantity")]
        public required int StockQuantity { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; } = string.Empty;

        [BsonElement("CompanyId")]
        public string? CompanyId { get; set; }
    }
}
