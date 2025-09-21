using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inventory = await _inventoryService.GetAllAsync();
            return Ok(inventory);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByProductId(string productId)
        {
            var item = await _inventoryService.GetByProductIdAsync(productId);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Inventory item)
        {
            await _inventoryService.CreateAsync(item);
            return CreatedAtAction(nameof(GetByProductId), new { productId = item.ProductId }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Inventory item)
        {
            var existing = await _inventoryService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            item.Id = id;
            await _inventoryService.UpdateAsync(id, item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _inventoryService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _inventoryService.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("bestselling")]
        public async Task<IActionResult> GetBestSellingProducts([FromQuery] int limit = 10)
        {
            var topProducts = await _inventoryService.GetBestSellingProductsAsync(limit);
            return Ok(topProducts);
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompanyId(string companyId)
        {
            var inventory = await _inventoryService.GetAllAsync();
            var filteredInventory = inventory.Where(i => i.CompanyId == companyId).ToList();
            return Ok(filteredInventory);
        }

        [HttpPatch("company/{companyId}/product/{productId}/stock")]
        public async Task<IActionResult> UpdateStockQuantity(string companyId, string productId, [FromBody] int newStockQuantity)
        {
            var inventory = await _inventoryService.GetAllAsync();
            var product = inventory.FirstOrDefault(i => i.ProductId == productId && i.CompanyId == companyId);
            if (product == null)
                return NotFound();

            product.StockQuantity = newStockQuantity;
            await _inventoryService.UpdateAsync(product.Id, product);
            return NoContent();
        }


    }
}
