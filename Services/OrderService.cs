using MongoDB.Driver;
using Backend.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _ordersCollection;

        public OrderService(IMongoDatabase database)
        {
            _ordersCollection = database.GetCollection<Order>("orders");
        }

        // Get all orders
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _ordersCollection.Find(order => true).ToListAsync();
        }

        // Get an order by OrderId
        public async Task<Order> GetOrderByOrderIdAsync(string orderId)
        {
            return await _ordersCollection
                .Find(order => order.OrderId == orderId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetByCompanyIdAsync(string companyId)
        {
        var filter = Builders<Order>.Filter.Eq(o => o.CompanyId, companyId);
        return await _ordersCollection.Find(filter).ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetOrderCountsByStatusesAsync(List<string> statuses)
        {
            var filter = Builders<Order>.Filter.In(o => o.Status, statuses);

            var group = await _ordersCollection
                .Aggregate()
                .Match(filter)
                .Group(
                    o => o.Status,
                    g => new { Status = g.Key, Count = g.Count() }
                )
                .ToListAsync();

            return group.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<double> GetTodayOrdersAsync(string? companyId = null)
        {
            var bsonOrders = await _ordersCollection.Find(_ => true).ToListAsync();

            // Parse manually from raw BSON to get the correct orderDate string
            var today = DateTime.Today;
            int count = 0;

            foreach (var bsonOrder in bsonOrders)
            {
                var rawDoc = bsonOrder.ToBsonDocument();
                var orderDateValue = rawDoc.GetValue("orderDate", null);
                
                // Check company ID if provided
                bool companyMatches = true;
                if (!string.IsNullOrEmpty(companyId))
                {
                    var orderCompanyId = rawDoc.GetValue("companyId", null)?.ToString();
                    companyMatches = orderCompanyId == companyId;
                }

                if (companyMatches && orderDateValue != null && DateTime.TryParse(orderDateValue.ToString(), out var parsedDate))
                {
                    if (parsedDate.Date == today)
                        count++;
                }
            }

            Console.WriteLine($"GetTodayOrdersAsync - Found {count} orders for today {today.ToString("yyyy-MM-dd")} " + 
                             (companyId != null ? $"for company ID {companyId}" : "across all companies"));

            return count;
        }

        public async Task<double> GetYesterdayOrdersAsync(string? companyId = null)
        {
            var bsonOrders = await _ordersCollection.Find(_ => true).ToListAsync();

            // Parse manually from raw BSON to get the correct orderDate string
            var yesterday = DateTime.Today.AddDays(-1);
            int count = 0;

            foreach (var bsonOrder in bsonOrders)
            {
                var rawDoc = bsonOrder.ToBsonDocument();
                var orderDateValue = rawDoc.GetValue("orderDate", null);
                
                // Check company ID if provided
                bool companyMatches = true;
                if (!string.IsNullOrEmpty(companyId))
                {
                    var orderCompanyId = rawDoc.GetValue("companyId", null)?.ToString();
                    companyMatches = orderCompanyId == companyId;
                }

                if (companyMatches && orderDateValue != null && DateTime.TryParse(orderDateValue.ToString(), out var parsedDate))
                {
                    if (parsedDate.Date == yesterday)
                        count++;
                }
            }

            Console.WriteLine($"GetYesterdayOrdersAsync - Found {count} orders for yesterday {yesterday.ToString("yyyy-MM-dd")} " + 
                             (companyId != null ? $"for company ID {companyId}" : "across all companies"));

            return count;
        }


public async Task<Dictionary<string, int>> GetOrderCountsByStatusesAndCompanyAsync(List<string> statuses, string companyId)
{
    var orders = await _ordersCollection
        .Find(o => o.CompanyId == companyId && statuses.Contains(o.Status.ToLower()))
        .ToListAsync();

    var summary = orders
        .GroupBy(o => o.Status.ToLower())
        .ToDictionary(g => g.Key, g => g.Count());

    return summary;
}




        // Create a new order
        public async Task CreateOrderAsync(Order newOrder)
        {
            await _ordersCollection.InsertOneAsync(newOrder);
        }

        // Update an existing order
        public async Task UpdateOrderAsync(string orderId, Order updatedOrder)
        {
            await _ordersCollection.ReplaceOneAsync(
                order => order.OrderId == orderId,
                updatedOrder
            );
        }

        // Delete an order by OrderId
        public async Task DeleteOrderAsync(string orderId)
        {
            await _ordersCollection.DeleteOneAsync(order => order.OrderId == orderId);
        }
    }
}
