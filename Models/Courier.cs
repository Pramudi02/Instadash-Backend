using System; // Provides base data types like DateTime.
using MongoDB.Bson; // Contains BSON data types used by MongoDB.
using MongoDB.Bson.Serialization.Attributes; // Provides attributes to map C# classes to BSON.

namespace Backend.Models
{
    public class Courier
    {
        [BsonId] // Specifies this property is the document's primary key.
        [BsonRepresentation(BsonType.ObjectId)] // Tells MongoDB to treat this string as an ObjectId.
        public string? Id { get; set; } // Unique identifier for the courier record.

        [BsonElement("order_id")] // Maps this property to the "order_id" field in MongoDB.
        public string? OrderId { get; set; } // The ID of the order associated with this courier.

        [BsonElement("courier_id")] // Maps this property to the "courier_id" field in MongoDB.
        public string? CourierId { get; set; } // Optional ID of the courier service/provider.

        [BsonElement("destination")] // Maps this property to the "destination" field in MongoDB.
        public string? Destination { get; set; } // Optional destination address or location.

        [BsonElement("date")] // Maps this property to the "date" field in MongoDB.
        public DateTime Date { get; set; } // The date the courier process was initiated.

        [BsonElement("estimate_date")] // Maps this property to the "estimate_date" field in MongoDB.
        public DateTime EstimateDate { get; set; } // Estimated delivery or arrival date.

        [BsonElement("status")] // Maps this property to the "status" field in MongoDB.
        public string? Status { get; set; } // The current status of the courier (e.g., "shipped", "delivered").

        [BsonElement("CompanyId")] // Maps this property to the "CompanyId" field in MongoDB.
        public string? CompanyId { get; set; } // Optional company ID for filtering.
    }
}
