using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly SalesService _salesService;
        private readonly MongoDBService _mongoDBService;

        public SalesController(SalesService salesService, MongoDBService mongoDBService)
        {
            _salesService = salesService;
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _salesService.GetAllSalesAsync();
            return Ok(sales);
        }

        [HttpGet("{saleId}")]
        public async Task<IActionResult> Get(string saleId)
        {
            var sale = await _salesService.GetSaleByIdAsync(saleId);
            if (sale == null)
                return NotFound();
            return Ok(sale);
        }

        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"SalesController - GetTotalRevenue - Received companyId: {companyId ?? "null"}");
            
            var sales = await _salesService.GetAllSalesAsync();
            Console.WriteLine($"SalesController - GetTotalRevenue - Total number of sales records: {sales.Count}");
            
            var totalRevenue = await _salesService.GetTotalSalesCostAsync(companyId);
            Console.WriteLine($"SalesController - GetTotalRevenue - Calculated total revenue: {totalRevenue} for companyId: {companyId ?? "null"}");
            
            if(totalRevenue == 0)
                return Ok(0);
            return Ok(totalRevenue);
        }

        [HttpGet("today-cost")]
        public async Task<IActionResult> GetTodayRevenue([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"SalesController - GetTodayRevenue - Received companyId: {companyId ?? "null"}");
            Console.WriteLine($"SalesController - GetTodayRevenue - Looking for sales on: {DateTime.Today.ToString("yyyy-MM-dd")}");
            
            var todayRevenue = await _salesService.GetTodaySalesRevenueAsync(companyId);
            Console.WriteLine($"SalesController - GetTodayRevenue - Calculated today revenue: {todayRevenue} for companyId: {companyId ?? "null"}");
            
            // Return the exact amount, even if it's 0
            return Ok(todayRevenue);
        }

        [HttpGet("last-month-revenue")]
        public async Task<IActionResult> GetLastMonthRevenue([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"SalesController - GetLastMonthRevenue - Received companyId: {companyId ?? "null"}");
            
            var lastMonthRevenue = await _salesService.GetLastMonthSalesRevenueAsync(companyId);
            Console.WriteLine($"SalesController - GetLastMonthRevenue - Calculated last month revenue: {lastMonthRevenue} for companyId: {companyId ?? "null"}");
            
            return Ok(lastMonthRevenue);
        }

        [HttpGet("yesterday-revenue")]
        public async Task<IActionResult> GetYesterdayRevenue([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"SalesController - GetYesterdayRevenue - Received companyId: {companyId ?? "null"}");
            
            var yesterdayRevenue = await _salesService.GetYesterdaySalesRevenueAsync(companyId);
            Console.WriteLine($"SalesController - GetYesterdayRevenue - Calculated yesterday revenue: {yesterdayRevenue} for companyId: {companyId ?? "null"}");
            
            return Ok(yesterdayRevenue);
        }

        [HttpGet("average-order-amount")]
        public async Task<IActionResult> GetAverageOrderAmount([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"SalesController - GetAverageOrderAmount - Received companyId: {companyId ?? "null"}");
            
            var averageAmount = await _salesService.GetAverageOrderAmountAsync(companyId);
            Console.WriteLine($"SalesController - GetAverageOrderAmount - Calculated average order amount: {averageAmount} for companyId: {companyId ?? "null"}");
            
            return Ok(averageAmount);
        }

        // Removed vendorId-based filter since it's no longer part of the data

        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] string startDate, [FromQuery] string endDate)
        {
            var sales = await _salesService.GetSalesByDateRangeAsync(startDate, endDate);
            return Ok(sales);
        }
        
        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var sales = await _salesService.GetSalesByYearAsync(year);
            return Ok(sales);
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompanyId(string companyId)
        {
            var sales = await _salesService.GetAllSalesAsync();
            var filteredSales = sales.Where(s => s.CompanyId == companyId).ToList();
            return Ok(filteredSales);
        }


        // TotalSales, TotalOrders, TotalItemsSold summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _salesService.GetSalesSummaryAsync();
            return Ok(summary);
        }

         [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlySales()
        {
            try
            {
                var monthlySales = await _salesService.GetMonthlySalesAsync();
                return Ok(monthlySales);
            }
            catch (System.Exception)
            {
                // Log exception if needed
                return StatusCode(500, "An error occurred while retrieving monthly sales data.");
            }
        }

        [HttpGet("year-data")]
        public async Task<IActionResult> GetYearlySales()
        {
            try
            {
                var monthlySales = await _salesService.GetYearlySalesAsync(DateTime.Now.Year);
                return Ok(monthlySales);
            }
            catch (System.Exception)
            {
                // Log exception if needed
                return StatusCode(500, "An error occurred while retrieving monthly sales data.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Sale sale)
        {
            await _salesService.CreateSaleAsync(sale);
            return CreatedAtAction(nameof(Get), new { saleId = sale.SaleId }, sale);
        }

        [HttpPut("{saleId}")]
        public async Task<IActionResult> Update(string saleId, [FromBody] Sale sale)
        {
            var existingSale = await _salesService.GetSaleByIdAsync(saleId);
            if (existingSale == null)
                return NotFound();

            sale.Id = saleId;
            await _salesService.UpdateSaleAsync(saleId, sale);
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            if (sale == null)
                return NotFound();

            await _salesService.DeleteSaleAsync(id);
            return NoContent();
        }

        [HttpPost("create-today-test-data")]
        public async Task<IActionResult> CreateTodayTestData([FromQuery] string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
                return BadRequest("CompanyId is required");

            await _salesService.CreateTodayTestSalesAsync(companyId);
            return Ok(new { message = $"Today's test sales data created for company {companyId}" });
        }

        [HttpGet("period-data")]
        public async Task<IActionResult> GetSalesDataByPeriod([FromQuery] string period, [FromQuery] string? companyId = null)
        {
            Console.WriteLine($"SalesController - GetSalesDataByPeriod - Period: {period}, CompanyId: {companyId ?? "null"}");
            
            var salesData = await _salesService.GetSalesDataByPeriodAsync(period, companyId);
            Console.WriteLine($"SalesController - GetSalesDataByPeriod - Returning {salesData.Count} data points");
            
            return Ok(salesData);
        }

        [HttpPost("create-historical-test-data")]
        public async Task<IActionResult> CreateHistoricalTestData([FromQuery] string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
                return BadRequest("CompanyId is required");

            await _salesService.CreateHistoricalTestSalesAsync(companyId);
            return Ok(new { message = $"Historical test sales data created for company {companyId}" });
        }
    }
}
