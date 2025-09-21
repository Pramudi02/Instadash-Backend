namespace Backend.Models
{
    public class SalesViewModel
    {
        public required List<Sale> Sales { get; set; }
        public required List<Order> RelatedOrders { get; set; }
        public required List<Inventory> RelatedInventory { get; set; }

        public double TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public int TotalOrders { get; set; }
    }
}
