using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;


namespace Backend.Models
{
    public class Expense
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("employeeName")]
    [Required] // Can keep this if it's required
    public string EmployeeName { get; set; }

    [BsonElement("position")]
    [Required] // Can keep this if it's required
    public string Position { get; set; }

    [BsonElement("expenseType")]
    [Required] // Can keep this if it's required
    public string ExpenseType { get; set; }

    [BsonElement("amount")]
    [Required] // Can keep this if it's required
    public decimal Amount { get; set; }

    [BsonElement("date")]
    [Required] // Can keep this if it's required
    public DateTime Date { get; set; }

    [BsonElement("paymentMethod")]
    [Required] // Can keep this if it's required
    public string PaymentMethod { get; set; }

    // Make description optional
    [BsonElement("description")]
    public string? Description { get; set; }

    // Make receiptUrl optional
    [BsonElement("receiptUrl")]
    public string? ReceiptUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string CompanyId { get; set; }
    public string HoneyCombId { get; set; }
}
}