using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<Customerr>> GetCustomerByCustomerId(string customerId)
        {
            var customer = await _customerService.GetByCustomerIdAsync(customerId);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }
    }
}
