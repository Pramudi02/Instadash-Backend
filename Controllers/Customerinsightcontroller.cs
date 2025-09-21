using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;


namespace Backend.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly MongoDbCustomerInsightService  _mongoDbCustomerinsightService;

        public CustomerController(MongoDbCustomerInsightService  mongoDbCustomerinsightService)
        {
            _mongoDbCustomerinsightService = mongoDbCustomerinsightService;
        }

        [HttpGet("top-customer")]
        public async Task<IActionResult> GetTopCustomer()
        {
            try
            {
                var (name, location) = await _mongoDbCustomerinsightService.GetTopCustomerByOrderCountAsync();

                // Return a default response with 200 OK status instead of 404
                if (name == "No orders" || name == "No customer data" || name == "Customer not found")
                {
                    return Ok(new { 
                        Name = "No customer data available", 
                        Location = "No location data" 
                    });
                }

                return Ok(new { 
                    Name = name, 
                    Location = location 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTopCustomer: {ex.Message}");
                return StatusCode(500, new { 
                    Name = "Error retrieving customer data", 
                    Location = "" 
                });
            }
        }


        [HttpGet("top-product")]
        public async Task<IActionResult> GetTopProduct()
        {
            var (ProductId, Name) = await _mongoDbCustomerinsightService.GetTopProductByOrderCountAsync();

            if (string.IsNullOrEmpty(Name))
                return NotFound("No product found.");

            return Ok(new { ProductId, Name });
        }


        [HttpGet("custable")]
        public async Task<ActionResult<List<Customerr>>> GettableAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var records = await _mongoDbCustomerinsightService.GettableAsync(from, to);
            return Ok(records ?? new List<Customerr>());
        }

        [HttpGet("location-count")]
        public async Task<ActionResult<List<LocationCustomerCount>>> GetCustomerCountByLocation()
        {
            var result = await _mongoDbCustomerinsightService.GetCustomerCountByLocationAsync();
            return Ok(result);
        }

        [HttpGet("product-stats/{productId}")]
        public async Task<IActionResult> GetProductStats(string productId)
        {
            var result = await _mongoDbCustomerinsightService.Getphurchaserate(productId);
            return Ok(result);
        }

        [HttpGet("active-and-inactive")]
        public async Task<IActionResult> Getactiveinactive()
        {
            var result = await _mongoDbCustomerinsightService.Getactiveinactive();
            return Ok(result);
        }

        [HttpPost("monthly-purchase-by-product")]
        public async Task<IActionResult> GetMonthlyPurchase([FromBody] ProductPurchaseRequest request)
        {
            if (string.IsNullOrEmpty(request.ProductId) || request.Year <= 0)
                return BadRequest("Invalid productId or year.");

            var data = await _mongoDbCustomerinsightService.GetMonthlyPurchaseByProductAsync(request.ProductId, request.Year);

            return Ok(data);
        }

        //just to operate with database 
        //not for application


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Customerr customer)
        {
            customer.Id = null;
            await _mongoDbCustomerinsightService.CreateAsync(customer);
            return CreatedAtAction(nameof(GetByCustomerId), new { customerId = customer.Customer_id }, customer);
        }


        [HttpGet("by-customer-id/{customerId}")]
        public async Task<ActionResult<Customerr>> GetByCustomerId(string customerId)
        {
            var customer = await _mongoDbCustomerinsightService.GetByCustomerIdAsync(customerId);
            if (customer == null)
                return NotFound();
            return Ok(customer);
        }

        [HttpPut("{customerId}")]
        public async Task<ActionResult> Update(string customerId, [FromBody] Customerr updatedCustomer)
        {
            var success = await _mongoDbCustomerinsightService.UpdateAsync(customerId, updatedCustomer);
            if (!success)
                return NotFound();
            return NoContent();
        }
    
        [HttpDelete("{customerId}")]
        public async Task<ActionResult> Delete(string customerId)
        {
            var success = await _mongoDbCustomerinsightService.DeleteAsync(customerId);
            if (!success)
                return NotFound();
            return NoContent();
        }


    }
}