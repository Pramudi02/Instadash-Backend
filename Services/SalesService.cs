using Backend.Models;
using Backend.Models.DTOs;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;   
using Backend.Models.DTOs;

namespace Backend.Services
{
    public class SalesService
    {
        private readonly MongoDBService _mongoDBService;

        public SalesService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        // Get all sales
        public async Task<List<Sale>> GetAllSalesAsync()
        {
            return await _mongoDBService.GetAllSalesAsync();
        }

        // Get sale by ID
        public async Task<Sale> GetSaleByIdAsync(string id)
        {
            return await _mongoDBService.GetSaleByIdAsync(id);
        }

        // Get sales within a date range
        public async Task<List<Sale>> GetSalesByDateRangeAsync(string startDate, string endDate)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();

            DateTime startDateTime = DateTime.Parse(startDate);
            DateTime endDateTime = DateTime.Parse(endDate);

            return sales
                .Where(s => s.SaleDate >= startDateTime && s.SaleDate <= endDateTime)
                .ToList();
        }

        // NEW: Get sales by year
        public async Task<List<Sale>> GetSalesByYearAsync(int year)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();

            return sales
                .Where(s =>
                {
                    if (DateTime.TryParse(s.SaleDate.ToString(), out DateTime saleDate))
                    {
                        return saleDate.Year == year;
                    }
                    return false;
                })
                .ToList();
        }

        // Get summary of sales
        public async Task<object> GetSalesSummaryAsync()
        {
            var sales = await _mongoDBService.GetAllSalesAsync();

            return new
            {
                TotalSales = sales.Sum(s => s.Amount),
                TotalOrders = sales.Sum(s => s.OrderIds.Count),
                TotalItemsSold = sales.Sum(s => s.OrderIds.Count) // Adjust logic if needed
            };
        }

        // Get total sales Revenue for a company
        public async Task<double> GetTotalSalesCostAsync(string? companyId = null)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            
            // Debug the sales data
            Console.WriteLine($"GetTotalSalesCostAsync - Total sales records: {sales.Count}");
            Console.WriteLine($"GetTotalSalesCostAsync - Looking for company ID: {companyId ?? "null"}");
            
            // Show some sample company IDs from the data
            var sampleCompanyIds = sales.Take(5).Select(s => s.CompanyId).Distinct();
            Console.WriteLine($"GetTotalSalesCostAsync - Sample company IDs in data: {string.Join(", ", sampleCompanyIds)}");
            
            // Filter by company ID if provided
            if (!string.IsNullOrEmpty(companyId))
            {
                var beforeCount = sales.Count;
                sales = sales.Where(s => s.CompanyId == companyId).ToList();
                var afterCount = sales.Count;
                
                Console.WriteLine($"GetTotalSalesCostAsync - Filtered from {beforeCount} to {afterCount} records for company ID {companyId}");
            }
            
            // Calculate sum
            var sum = sales.Sum(s => s.Amount);
            Console.WriteLine($"GetTotalSalesCostAsync - Total sum: {sum}");
            
            return sum;
        }

        // Get today sales Revenue for a company
        public async Task<double> GetTodaySalesRevenueAsync(string? companyId = null)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            
            Console.WriteLine($"GetTodaySalesRevenueAsync - Total sales records: {sales.Count}");
            Console.WriteLine($"GetTodaySalesRevenueAsync - Today's date: {DateTime.Today.ToString("yyyy-MM-dd")}");
            
            // Filter by today's date - ONLY today's date, no fallback to other dates
            var todaySales = sales.Where(s => s.SaleDate.Date == DateTime.Today).ToList();
            Console.WriteLine($"GetTodaySalesRevenueAsync - Sales from today: {todaySales.Count}");
            
            // Check for debug purposes if we actually have data
            if (todaySales.Count == 0)
            {
                Console.WriteLine("GetTodaySalesRevenueAsync - WARNING: No sales found for today's date.");
                // Return 0 if there are no sales today
                // This is intentional - we want the exact revenue of today's sales
            }
            
            // Filter by company ID if provided
            if (!string.IsNullOrEmpty(companyId))
            {
                var beforeCount = todaySales.Count;
                todaySales = todaySales.Where(s => s.CompanyId == companyId).ToList();
                var afterCount = todaySales.Count;
                
                Console.WriteLine($"GetTodaySalesRevenueAsync - Filtered from {beforeCount} to {afterCount} records for company ID {companyId}");
            }
            
            // Calculate sum
            var sum = todaySales.Sum(s => s.Amount);
            Console.WriteLine($"GetTodaySalesRevenueAsync - Total sum: {sum}");
            
            return sum;
        }

        // Get last month sales Revenue for a company
        public async Task<double> GetLastMonthSalesRevenueAsync(string? companyId = null)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            
            // Calculate last month's date range
            var today = DateTime.Today;
            var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
            var lastDayOfLastMonth = firstDayOfThisMonth.AddDays(-1);
            
            Console.WriteLine($"GetLastMonthSalesRevenueAsync - Total sales records: {sales.Count}");
            Console.WriteLine($"GetLastMonthSalesRevenueAsync - Last month range: {firstDayOfLastMonth.ToString("yyyy-MM-dd")} to {lastDayOfLastMonth.ToString("yyyy-MM-dd")}");
            
            // Filter by last month's date range
            var lastMonthSales = sales.Where(s => 
                s.SaleDate.Date >= firstDayOfLastMonth && 
                s.SaleDate.Date <= lastDayOfLastMonth).ToList();
            Console.WriteLine($"GetLastMonthSalesRevenueAsync - Sales from last month: {lastMonthSales.Count}");
            
            // Filter by company ID if provided
            if (!string.IsNullOrEmpty(companyId))
            {
                var beforeCount = lastMonthSales.Count;
                lastMonthSales = lastMonthSales.Where(s => s.CompanyId == companyId).ToList();
                var afterCount = lastMonthSales.Count;
                
                Console.WriteLine($"GetLastMonthSalesRevenueAsync - Filtered from {beforeCount} to {afterCount} records for company ID {companyId}");
            }
            
            // Calculate sum
            var sum = lastMonthSales.Sum(s => s.Amount);
            Console.WriteLine($"GetLastMonthSalesRevenueAsync - Total sum: {sum}");
            
            return sum;
        }

        // Get yesterday's sales revenue for a company
        public async Task<double> GetYesterdaySalesRevenueAsync(string? companyId = null)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            var yesterday = DateTime.Today.AddDays(-1);
            
            Console.WriteLine($"GetYesterdaySalesRevenueAsync - Total sales records: {sales.Count}");
            Console.WriteLine($"GetYesterdaySalesRevenueAsync - Yesterday's date: {yesterday.ToString("yyyy-MM-dd")}");
            
            // Filter sales from yesterday
            var yesterdaySales = sales.Where(s => s.SaleDate.Date == yesterday.Date).ToList();
            Console.WriteLine($"GetYesterdaySalesRevenueAsync - Sales from yesterday: {yesterdaySales.Count}");
            
            // Filter by company ID if provided
            if (!string.IsNullOrEmpty(companyId))
            {
                var beforeCount = yesterdaySales.Count;
                yesterdaySales = yesterdaySales.Where(s => s.CompanyId == companyId).ToList();
                var afterCount = yesterdaySales.Count;
                
                Console.WriteLine($"GetYesterdaySalesRevenueAsync - Filtered from {beforeCount} to {afterCount} records for company ID {companyId}");
            }
            
            // Calculate sum
            var sum = yesterdaySales.Sum(s => s.Amount);
            Console.WriteLine($"GetYesterdaySalesRevenueAsync - Total sum: {sum}");
            
            return sum;
        }

        // Get average order amount for a company
        public async Task<double> GetAverageOrderAmountAsync(string? companyId = null)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            
            Console.WriteLine($"GetAverageOrderAmountAsync - Total sales records: {sales.Count}");
            Console.WriteLine($"GetAverageOrderAmountAsync - Looking for company ID: {companyId ?? "null"}");
            
            // Filter by company ID if provided
            if (!string.IsNullOrEmpty(companyId))
            {
                var beforeCount = sales.Count;
                sales = sales.Where(s => s.CompanyId == companyId).ToList();
                var afterCount = sales.Count;
                
                Console.WriteLine($"GetAverageOrderAmountAsync - Filtered from {beforeCount} to {afterCount} records for company ID {companyId}");
            }
            
            // Calculate average
            if (sales.Count == 0)
            {
                Console.WriteLine("GetAverageOrderAmountAsync - No sales found, returning 0");
                return 0;
            }
            
            var average = sales.Average(s => s.Amount);
            Console.WriteLine($"GetAverageOrderAmountAsync - Average amount: {average}");
            
            return average;
        }

        public async Task<List<double>> GetMonthlySalesAsync()
        {
            var sales = await _mongoDBService.GetAllSalesAsync();

            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Filter sales for current month and year
            var filteredSales = sales.Where(s =>
                s.SaleDate.Month == currentMonth &&
                s.SaleDate.Year == currentYear
            );

            // Group sales by day and sum the Amount
            var grouped = filteredSales
                .GroupBy(s => s.SaleDate.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    Total = g.Sum(x => x.Amount)  // double sum here
                })
                .ToList();

            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            var dailySales = new List<double>(new double[daysInMonth]);  // initialize with zeros

            // Fill the daily sales list with the sums
            foreach (var item in grouped)
            {
                dailySales[item.Day - 1] = item.Total;
            }

            return dailySales;
        }

        public async Task<List<double>> GetYearlySalesAsync(int year)
        {
            // Load all sales (you can optimize later if needed)
            var sales = await _mongoDBService.GetAllSalesAsync();

            // Filter sales only in the given year
            var yearlySales = sales.Where(s => s.SaleDate.Year == year);

            // Initialize list for 12 months (Jan to Dec) with zeros
            var monthlyTotals = new double[12];

            // Sum amounts grouped by month
            foreach (var sale in yearlySales)
            {
                int monthIndex = sale.SaleDate.Month - 1;
                monthlyTotals[monthIndex] += sale.Amount;
            }

            return monthlyTotals.ToList();
        }

        // Aggregate sales, orders, and inventory for a date range and company
        public async Task<List<SalesAggregatedDto>> GetAggregatedSalesDataAsync(DateTime startDate, DateTime endDate, string companyId)
        {
            var sales = (await _mongoDBService.GetAllSalesAsync()).Where(s => s.CompanyId == companyId).ToList();
            var orders = (await _mongoDBService.GetAllOrdersAsync()).Where(o => o.CompanyId == companyId).ToList();
            var inventory = (await _mongoDBService.GetAllInventoryAsync()).Where(p => p.CompanyId == companyId).ToList();

            var filteredSales = sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate).ToList();

            var result = new List<SalesAggregatedDto>();

            foreach (var sale in filteredSales)
            {
                foreach (var orderId in sale.OrderIds)
                {
                    var order = orders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order != null && order.OrderDetails != null)
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            var prod = inventory.FirstOrDefault(p => p.ProductId == detail.ProductId);
                            result.Add(new SalesAggregatedDto
                            {
                                SaleId = sale.SaleId,
                                SalesDate = sale.SaleDate,
                                OrderId = order.OrderId,
                                ProductId = detail.ProductId,
                                ProductName = prod?.Name ?? string.Empty,
                                Category = prod?.Category ?? string.Empty,
                                Quantity = detail.Quantity,
                                Price = detail.Price,
                                CompanyId = sale.CompanyId ?? string.Empty
                            });
                        }
                    }
                }
            }
            return result;
        }


        // Create a sale
        public async Task CreateSaleAsync(Sale sale)
        {
            await _mongoDBService.CreateSaleAsync(sale);
        }

        // Update a sale
        public async Task UpdateSaleAsync(string id, Sale sale)
        {
            await _mongoDBService.UpdateSaleAsync(id, sale);
        }

        // Delete a sale
        public async Task DeleteSaleAsync(string id)
        {
            await _mongoDBService.DeleteSaleAsync(id);
        }

        // Create test sales data for today (for testing purposes)
        public async Task CreateTodayTestSalesAsync(string companyId)
        {
            var today = DateTime.Today;
            var existingSalesToday = (await _mongoDBService.GetAllSalesAsync())
                .Where(s => s.SaleDate.Date == today && s.CompanyId == companyId)
                .ToList();

            if (existingSalesToday.Count > 0)
            {
                Console.WriteLine($"Company {companyId} already has {existingSalesToday.Count} sales for today");
                return;
            }

            var testSales = new List<Sale>
            {
                new Sale
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    SaleId = $"SALE_{companyId}_{today:yyyyMMdd}_001",
                    OrderIds = new List<string> { $"ORD_{companyId}_{today:yyyyMMdd}_001" },
                    Amount = 150.75,
                    SaleDate = today.AddHours(9).AddMinutes(30), // 9:30 AM today
                    CompanyId = companyId
                },
                new Sale
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    SaleId = $"SALE_{companyId}_{today:yyyyMMdd}_002",
                    OrderIds = new List<string> { $"ORD_{companyId}_{today:yyyyMMdd}_002" },
                    Amount = 89.99,
                    SaleDate = today.AddHours(11).AddMinutes(15), // 11:15 AM today
                    CompanyId = companyId
                },
                new Sale
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    SaleId = $"SALE_{companyId}_{today:yyyyMMdd}_003",
                    OrderIds = new List<string> { $"ORD_{companyId}_{today:yyyyMMdd}_003" },
                    Amount = 225.50,
                    SaleDate = today.AddHours(14).AddMinutes(45), // 2:45 PM today
                    CompanyId = companyId
                },
                new Sale
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    SaleId = $"SALE_{companyId}_{today:yyyyMMdd}_004",
                    OrderIds = new List<string> { $"ORD_{companyId}_{today:yyyyMMdd}_004" },
                    Amount = 75.25,
                    SaleDate = today.AddHours(16).AddMinutes(20), // 4:20 PM today
                    CompanyId = companyId
                }
            };

            foreach (var sale in testSales)
            {
                await _mongoDBService.CreateSaleAsync(sale);
            }

            Console.WriteLine($"Created {testSales.Count} test sales for today for company {companyId}");
            Console.WriteLine($"Total test sales amount: ${testSales.Sum(s => s.Amount):F2}");
        }

        // Get sales data by period for a specific company
        public async Task<List<object>> GetSalesDataByPeriodAsync(string period, string? companyId = null)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            Console.WriteLine($"GetSalesDataByPeriodAsync - Total sales: {sales.Count}, Period: {period}, CompanyId: {companyId ?? "null"}");

            // Filter by company ID if provided
            if (!string.IsNullOrEmpty(companyId))
            {
                var beforeCount = sales.Count;
                sales = sales.Where(s => s.CompanyId == companyId).ToList();
                Console.WriteLine($"GetSalesDataByPeriodAsync - Filtered from {beforeCount} to {sales.Count} records for company {companyId}");
            }

            var now = DateTime.UtcNow;
            var result = new List<object>();

            switch (period.ToLower())
            {
                case "month":
                    // Current month - daily data (up to 31 days)
                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                    var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                    
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        var targetDate = new DateTime(now.Year, now.Month, day);
                        var dailySales = sales.Where(s => s.SaleDate.Date == targetDate.Date).Sum(s => s.Amount);
                        result.Add(new { 
                            label = $"Day {day}", 
                            date = targetDate.ToString("MMM dd"),
                            value = dailySales 
                        });
                    }
                    break;

                case "threemonths":
                    // Last 3 months - weekly data (12 weeks)
                    var threeMonthsAgo = now.AddMonths(-3);
                    for (int week = 0; week < 12; week++)
                    {
                        var weekStart = threeMonthsAgo.AddDays(week * 7);
                        var weekEnd = weekStart.AddDays(6);
                        var weeklySales = sales.Where(s => s.SaleDate.Date >= weekStart.Date && s.SaleDate.Date <= weekEnd.Date).Sum(s => s.Amount);
                        result.Add(new { 
                            label = $"Week {week + 1}", 
                            date = $"{weekStart:MMM dd} - {weekEnd:MMM dd}",
                            value = weeklySales 
                        });
                    }
                    break;

                case "sixmonths":
                    // Last 6 months - monthly data
                    for (int month = 5; month >= 0; month--)
                    {
                        var targetMonth = now.AddMonths(-month);
                        var startOfTargetMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                        var endOfTargetMonth = startOfTargetMonth.AddMonths(1).AddDays(-1);
                        var monthlySales = sales.Where(s => s.SaleDate.Date >= startOfTargetMonth.Date && s.SaleDate.Date <= endOfTargetMonth.Date).Sum(s => s.Amount);
                        result.Add(new { 
                            label = targetMonth.ToString("MMM yyyy"), 
                            date = targetMonth.ToString("MMM yyyy"),
                            value = monthlySales 
                        });
                    }
                    break;

                case "year":
                    // Current year - monthly data (12 months)
                    for (int month = 1; month <= 12; month++)
                    {
                        var targetMonth = new DateTime(now.Year, month, 1);
                        var monthStart = targetMonth;
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var monthlySales = sales.Where(s => s.SaleDate.Date >= monthStart.Date && s.SaleDate.Date <= monthEnd.Date).Sum(s => s.Amount);
                        result.Add(new { 
                            label = targetMonth.ToString("MMM"), 
                            date = targetMonth.ToString("MMM yyyy"),
                            value = monthlySales 
                        });
                    }
                    break;

                default:
                    // Default to current month
                    var defaultStartOfMonth = new DateTime(now.Year, now.Month, 1);
                    var defaultDaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                    
                    for (int day = 1; day <= defaultDaysInMonth; day++)
                    {
                        var targetDate = new DateTime(now.Year, now.Month, day);
                        var dailySales = sales.Where(s => s.SaleDate.Date == targetDate.Date).Sum(s => s.Amount);
                        result.Add(new { 
                            label = $"Day {day}", 
                            date = targetDate.ToString("MMM dd"),
                            value = dailySales 
                        });
                    }
                    break;
            }

            Console.WriteLine($"GetSalesDataByPeriodAsync - Returning {result.Count} data points for period {period}");
            return result;
        }

        // Create historical test sales data for a company (for testing chart functionality)
        public async Task CreateHistoricalTestSalesAsync(string companyId)
        {
            var now = DateTime.UtcNow;
            var existingHistoricalSales = (await _mongoDBService.GetAllSalesAsync())
                .Where(s => s.CompanyId == companyId && s.SaleDate.Date < now.Date)
                .ToList();

            if (existingHistoricalSales.Count > 10) // If we already have some historical data
            {
                Console.WriteLine($"Company {companyId} already has {existingHistoricalSales.Count} historical sales");
                return;
            }

            var historicalSales = new List<Sale>();

            // Create sales data for the past 6 months
            for (int month = 6; month >= 1; month--)
            {
                var targetMonth = now.AddMonths(-month);
                var daysInMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
                
                // Create 3-8 random sales per month
                var salesPerMonth = new Random().Next(3, 9);
                
                for (int sale = 0; sale < salesPerMonth; sale++)
                {
                    var randomDay = new Random().Next(1, daysInMonth + 1);
                    var saleDate = new DateTime(targetMonth.Year, targetMonth.Month, randomDay)
                        .AddHours(new Random().Next(9, 18)) // Business hours
                        .AddMinutes(new Random().Next(0, 60));
                    
                    var amount = new Random().NextDouble() * 800 + 100; // $100 - $900
                    
                    historicalSales.Add(new Sale
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        SaleId = $"SALE_{companyId}_{saleDate:yyyyMMdd}_{sale:D3}",
                        OrderIds = new List<string> { $"ORD_{companyId}_{saleDate:yyyyMMdd}_{sale:D3}" },
                        Amount = Math.Round(amount, 2),
                        SaleDate = saleDate,
                        CompanyId = companyId
                    });
                }
            }

            // Create some sales for the current month (excluding today which is handled separately)
            var currentMonth = now;
            var currentDaysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
            var salesThisMonth = new Random().Next(5, 12);
            
            for (int sale = 0; sale < salesThisMonth; sale++)
            {
                var randomDay = new Random().Next(1, now.Day); // Only past days this month
                if (randomDay == now.Day) continue; // Skip today
                
                var saleDate = new DateTime(currentMonth.Year, currentMonth.Month, randomDay)
                    .AddHours(new Random().Next(9, 18))
                    .AddMinutes(new Random().Next(0, 60));
                
                var amount = new Random().NextDouble() * 600 + 150; // $150 - $750
                
                historicalSales.Add(new Sale
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    SaleId = $"SALE_{companyId}_{saleDate:yyyyMMdd}_{sale:D3}",
                    OrderIds = new List<string> { $"ORD_{companyId}_{saleDate:yyyyMMdd}_{sale:D3}" },
                    Amount = Math.Round(amount, 2),
                    SaleDate = saleDate,
                    CompanyId = companyId
                });
            }

            foreach (var sale in historicalSales)
            {
                await _mongoDBService.CreateSaleAsync(sale);
            }

            Console.WriteLine($"Created {historicalSales.Count} historical test sales for company {companyId}");
            Console.WriteLine($"Date range: {historicalSales.Min(s => s.SaleDate):yyyy-MM-dd} to {historicalSales.Max(s => s.SaleDate):yyyy-MM-dd}");
        }
    }
}
