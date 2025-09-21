using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Backend.Models.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDashboardController : ControllerBase
    {
        private readonly SalesService _salesService;
        private readonly MongoDBService _mongoDBService;

        public SalesDashboardController(SalesService salesService, MongoDBService mongoDBService)
        {
            _salesService = salesService;
            _mongoDBService = mongoDBService;
        }

        // Get all sales, orders, and inventory data
       [HttpGet]
public async Task<IActionResult> GetDashboardData()
{
    var sales = await _mongoDBService.GetAllSalesAsync() ?? new List<Sale>();
    var orders = await _mongoDBService.GetAllOrdersAsync() ?? new List<Order>();
    var inventory = await _mongoDBService.GetAllInventoryAsync() ?? new List<Inventory>();

    var totalRevenue = sales.Sum(s => s.Amount);
    var totalItems = orders.Sum(o => (o.OrderDetails ?? new List<OrderDetail>()).Sum(od => od.Quantity));
    var totalOrders = orders.Count;

            var viewModel = new SalesViewModel
            {
                Sales = sales,
                RelatedOrders = orders,
                RelatedInventory = inventory,
                TotalRevenue = totalRevenue,
                TotalItems = totalItems,
                TotalOrders = totalOrders
            };

    return Ok(viewModel);
}


        // Get dashboard data within a date range
        [HttpGet("date-range")]
        public async Task<IActionResult> GetDashboardDataByDateRange([FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                // Parse the input dates
                DateTime startDateTime = DateTime.Parse(startDate);
                DateTime endDateTime = DateTime.Parse(endDate);

                // Get sales data within the given date range
                var sales = await _salesService.GetSalesByDateRangeAsync(startDate, endDate);

                // Calculate total revenue, total items, and total orders
                var totalRevenue = sales.Sum(s => s.Amount);
                var totalItems = sales.Sum(s => s.OrderIds.Count); // Assuming OrderIds holds the count of orders
                var totalOrders = sales.Count;

                // Return the summary in a structured response
                var summary = new
                {
                    TotalRevenue = totalRevenue,
                    TotalItems = totalItems,
                    TotalOrders = totalOrders
                };

                return Ok(summary);
            }
            catch (FormatException ex)
            {
                return BadRequest($"Invalid date format: {ex.Message}");
            }
        }

        // Get sales, orders, and inventory data filtered by saleId
        [HttpGet("sale/{saleId}")]
        public async Task<IActionResult> GetDashboardDataBySaleId(string saleId)
        {
            var sales = await _mongoDBService.GetAllSalesAsync();
            var targetSale = sales.FirstOrDefault(s => s.SaleId == saleId);

            if (targetSale == null)
                return NotFound($"Sale with SaleId {saleId} not found.");

            var allOrders = await _mongoDBService.GetAllOrdersAsync();

            var relatedOrders = allOrders
                .Where(o => targetSale.OrderIds.Contains(o.OrderId))
                .ToList();

            var productIds = relatedOrders
                .SelectMany(o => o.OrderDetails.Select(od => od.ProductId))
                .Distinct()
                .ToList();

            var inventory = await _mongoDBService.GetAllInventoryAsync();
            var relatedInventory = inventory
                .Where(i => productIds.Contains(i.ProductId))
                .ToList();

            var totalRevenue = targetSale.Amount;
            var totalItems = relatedOrders.Sum(o => o.OrderDetails.Sum(od => od.Quantity));
            var totalOrders = relatedOrders.Count;

            var viewModel = new SalesViewModel
            {
                Sales = new List<Sale> { targetSale },
                RelatedOrders = relatedOrders,
                RelatedInventory = relatedInventory,
                TotalRevenue = totalRevenue,
                TotalItems = totalItems,
                TotalOrders = totalOrders
            };

            return Ok(viewModel);
        }

        [HttpGet("company-sales-comparison")]
        public async Task<IActionResult> GetCompanySalesComparison([FromQuery] string month)
        {
            // Parse month (format: YYYY-MM)
            if (!DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthStart))
                return BadRequest("Invalid month format. Use YYYY-MM.");
            var monthEnd = monthStart.AddMonths(1);

            // Get all companies with salesAccessValue == 1
            var salesAccessService = HttpContext.RequestServices.GetService(typeof(SalesAccessService)) as SalesAccessService;
            if (salesAccessService == null) return StatusCode(500, "SalesAccessService not available");
            var accessList = await salesAccessService.GetAllAsync();
            var allowedCompanies = accessList.Where(a => a.SalesAccessValue == 1).Select(a => a.CompanyId).ToList();

            // Get all sales
            var allSales = await _mongoDBService.GetAllSalesAsync();
            var result = allowedCompanies.Select(companyId => new {
                companyId,
                totalSales = allSales
                    .Where(s => s.CompanyId == companyId && s.SaleDate >= monthStart && s.SaleDate < monthEnd)
                    .Sum(s => s.Amount)
            }).ToList();

            return Ok(result);
        }

        // Aggregated sales, orders, and inventory for a date range and company
        [HttpGet("aggregated-sales")]
        public async Task<IActionResult> GetAggregatedSales([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest("companyId is required.");
            }
            if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
            {
                return BadRequest("Invalid date format.");
            }
            var data = await _salesService.GetAggregatedSalesDataAsync(start, end, companyId);
            return Ok(data);
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetDashboardDataByCompanyId(string companyId, [FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            DateTime? start = null, end = null;
            if (!string.IsNullOrEmpty(startDate))
                start = DateTime.Parse(startDate);
            if (!string.IsNullOrEmpty(endDate))
                end = DateTime.Parse(endDate);

            var sales = (await _mongoDBService.GetAllSalesAsync() ?? new List<Sale>())
                .Where(s => s.CompanyId == companyId &&
                    (!start.HasValue || s.SaleDate >= start.Value) &&
                    (!end.HasValue || s.SaleDate <= end.Value))
                .ToList();
            var orders = (await _mongoDBService.GetAllOrdersAsync() ?? new List<Order>())
                .Where(o => o.CompanyId == companyId &&
                    (!start.HasValue || o.OrderDate >= start.Value) &&
                    (!end.HasValue || o.OrderDate <= end.Value))
                .ToList();
            var inventory = (await _mongoDBService.GetAllInventoryAsync() ?? new List<Inventory>())
                .Where(i => i.CompanyId == companyId)
                .ToList();

            var totalRevenue = sales.Sum(s => s.Amount);
            var totalItems = orders.Sum(o => (o.OrderDetails ?? new List<OrderDetail>()).Sum(od => od.Quantity));
            var totalOrders = orders.Count;

            var viewModel = new SalesViewModel
            {
                Sales = sales,
                RelatedOrders = orders,
                RelatedInventory = inventory,
                TotalRevenue = totalRevenue,
                TotalItems = totalItems,
                TotalOrders = totalOrders
            };

            return Ok(viewModel);
        }
    }
}
