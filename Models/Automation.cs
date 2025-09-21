using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;


namespace Backend.Models
{
    public class Automation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("reportType")]
        public string ReportType { get; set; } = string.Empty;

        [BsonElement("frequency")]
        public string Frequency { get; set; } = string.Empty;

        [BsonElement("time")]
        public string Time { get; set; } = string.Empty;

        [BsonElement("format")]
        public string Format { get; set; } = "PDF";

        [BsonElement("dayOfWeek")]
        public string? DayOfWeek { get; set; }

        [BsonElement("dayOfMonth")]
        public int? DayOfMonth { get; set; }

        [BsonElement("emails")]
        public string Emails { get; set; } = string.Empty;

        [BsonElement("subject")]
        public string Subject { get; set; } = string.Empty;

        [BsonElement("message")]
        public string? Message { get; set; }

        [BsonElement("notifyOnSuccess")]
        public bool NotifyOnSuccess { get; set; }

        [BsonElement("notifyOnFailure")]
        public bool NotifyOnFailure { get; set; }

        [BsonElement("CompanyId")]
        public string? CompanyId { get; set; }


    public string HoneyCombId { get; set; }
    }
}
