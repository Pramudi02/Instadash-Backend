using System;
using System.Collections.Generic;

namespace Backend.Models.DTOs
{
    public class OrderCreateDTO
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;

        // Initialized to avoid nullability warning
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();

        public double TotalAmount { get; set; }

        public DateTime? OrderDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
