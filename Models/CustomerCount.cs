namespace Backend.Models
{
    public class Customer
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? CompanyId { get; set; }
    }
}
