using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly FinanceService _financeService;

        public FinanceController(FinanceService financeService)
        {
            _financeService = financeService;
        }

        // Get all finances
        [HttpGet]
        public async Task<ActionResult<List<Finance>>> Get()
        {
            var finances = await _financeService.GetAsync();
            return Ok(finances);
        }

        // Get a finance by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Finance>> GetById(string id)
        {
            var finance = await _financeService.GetByIdAsync(id);

            if (finance == null)
                return NotFound();

            return Ok(finance);
        }

        // GET finance data by CompanyId
            [HttpGet("company/{companyId}")]
            public async Task<ActionResult<List<Finance>>> GetByCompanyId(string companyId)
            {
                var finances = await _financeService.GetByCompanyIdAsync(companyId);
                return Ok(finances);
            }


        // Create a new finance entry
        [HttpPost]
        public async Task<ActionResult> Create(Finance finance)
        {
            await _financeService.CreateAsync(finance);
            return CreatedAtAction(nameof(GetById), new { id = finance.Id }, finance);
        }

        // Update an existing finance entry
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, Finance finance)
        {
            var existingFinance = await _financeService.GetByIdAsync(id);
            if (existingFinance == null)
                return NotFound();

            finance.Id = id;
            await _financeService.UpdateAsync(id, finance);
            return NoContent();
        }

        // Delete a finance entry by id
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var existingFinance = await _financeService.GetByIdAsync(id);
            if (existingFinance == null)
                return NotFound();

            await _financeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
