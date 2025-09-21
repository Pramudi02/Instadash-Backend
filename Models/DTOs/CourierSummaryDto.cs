using System.Text.Json.Serialization; // Provides attributes to control JSON serialization behavior.

namespace Backend.Models.DTOs
{
    public class CourierSummaryDto
    {
        [JsonPropertyName("total")] // Maps this property to the "total" key in JSON.
        public int Total { get; set; } // Total number of courier entries.

        [JsonPropertyName("pending")] // Maps this property to the "pending" key in JSON.
        public int Pending { get; set; } // Number of couriers that are pending.

        [JsonPropertyName("completed")] // Maps this property to the "completed" key in JSON.
        public int Completed { get; set; } // Number of couriers that are completed.

        [JsonPropertyName("rejected")] // Maps this property to the "rejected" key in JSON.
        public int Rejected { get; set; } // Number of couriers that are rejected or failed.
    }
}
// This Dto is used to summarize courier data, providing a structured response for API consumers.