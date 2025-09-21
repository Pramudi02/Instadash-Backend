namespace Backend.Models.DTOs
{
    public class OrderDetailDTO
    {
        public string ProductId { get; set; } = string.Empty;  // Initialize with default value
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
