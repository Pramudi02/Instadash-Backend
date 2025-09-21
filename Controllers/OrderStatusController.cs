using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStatusController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderStatusController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetOrderStatusSummary()
        {
            var statuses = new List<string> { "completed", "pending" ,"new"};
            var result = await _orderService.GetOrderCountsByStatusesAsync(statuses);
            return Ok(result);
        }

        [HttpGet("summary/company/{companyId}")]
public async Task<IActionResult> GetOrderStatusSummaryByCompany(string companyId)
{
    var statuses = new List<string> { "completed", "pending", "new" };
    var result = await _orderService.GetOrderCountsByStatusesAndCompanyAsync(statuses, companyId);
    return Ok(result);
}

    }
}
