using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Backend.Models
{
    public class Campaign
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }  // MongoDB ObjectId

        [BsonElement("camId")]
        public string? CamId { get; set; }

        [BsonElement("platform")]
        public string? Platform { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("clickThroughRate")]
        public double ClickThroughRate { get; set; }

        [BsonElement("cpc")]
        public double Cpc { get; set; }

        [BsonElement("spentAmount")]
        public double SpentAmount { get; set; }

        [BsonElement("noOfVisitors")]
        public int NoOfVisitors { get; set; }

        [BsonElement("noOfCustomers")]
        public int NoOfCustomers { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("CompanyId")]
        public string? CompanyId { get; set; }
    }
}
