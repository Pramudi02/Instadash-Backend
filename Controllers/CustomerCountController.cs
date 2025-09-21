using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerCountController : ControllerBase
    {
        private readonly CustomerCountService _customerService;

        public CustomerCountController(CustomerCountService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCustomerCount([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("Company ID is required.");

            var count = await _customerService.GetCustomerCountByCompanyAsync(companyId);
            return Ok(count);
        }

        [HttpGet("total-customers")]
        public async Task<IActionResult> GetTotalCustomers([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"CustomerCountController - GetTotalCustomers - Received companyId: {companyId ?? "null"}");
            
            var count = await _customerService.GetTotalCustomersAsync(companyId);
            Console.WriteLine($"CustomerCountController - GetTotalCustomers - Found {count} total customers for companyId: {companyId ?? "all companies"}");
            
            return Ok(count);
        }

        [HttpGet("last-month-customers")]
        public async Task<IActionResult> GetLastMonthCustomers([FromQuery] string? companyId = null)
        {
            Console.WriteLine($"CustomerCountController - GetLastMonthCustomers - Received companyId: {companyId ?? "null"}");
            
            var count = await _customerService.GetLastMonthCustomersAsync(companyId);
            Console.WriteLine($"CustomerCountController - GetLastMonthCustomers - Found {count} customers from last month for companyId: {companyId ?? "all companies"}");
            
            return Ok(count);
        }

    }
}
