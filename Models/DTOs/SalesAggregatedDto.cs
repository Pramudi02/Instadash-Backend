using System;
   namespace Backend.Models.DTOs
   {
       public class SalesAggregatedDto
       {
           public string SaleId { get; set; }
           public DateTime SalesDate { get; set; }
           public string OrderId { get; set; }
           public string ProductId { get; set; }
           public string ProductName { get; set; }
           public string Category { get; set; }
           public int Quantity { get; set; }
           public double Price { get; set; }
           public string CompanyId { get; set; }
       }
   }