using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Backend.Models
{
    [BsonIgnoreExtraElements]
    public class Sale
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("sales_id")] 
        public required string SaleId { get; set; }

        [BsonElement("orderIds")]
        public required List<string> OrderIds { get; set; }

        [BsonElement("saledate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime SaleDate { get; set; }

        [BsonElement("amount")]
        public required double Amount { get; set; }

        [BsonElement("CompanyId")]
        public string? CompanyId { get; set; }
    }
}
