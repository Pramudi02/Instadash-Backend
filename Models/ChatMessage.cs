using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("text")]
        public string Text { get; set; } = string.Empty;

        [BsonElement("sender")]
        public string Sender { get; set; } = string.Empty;

        [BsonElement("receiver")]
        public string Receiver { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false; // // not yet confirmed by receiver
    }
}
