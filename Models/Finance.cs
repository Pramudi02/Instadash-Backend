using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Backend.Models
{
    public class Finance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("finance_id")]
        public required string FinanceId { get; set; }

        [BsonElement("sales_id")]
        public required List<string> SalesIds { get; set; }

        [BsonElement("campal_id")]
        public required List<string> CampalIds { get; set; }

        [BsonElement("amount")]
        public double Amount { get; set; }

        [BsonElement("status")]
        public required string Status { get; set; }

        [BsonElement("date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OrderDate { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("CompanyId")]
        public string? CompanyId { get; set; }
    }
}
