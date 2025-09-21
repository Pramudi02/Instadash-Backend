using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesAccessController : ControllerBase
    {
        private readonly SalesAccessService _service;

        public SalesAccessController(SalesAccessService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalesAccess>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<SalesAccess>> GetByCompanyId(string companyId)
        {
            var result = await _service.GetByCompanyIdAsync(companyId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SalesAccess access)
        {
            await _service.CreateAsync(access);
            return CreatedAtAction(nameof(GetByCompanyId), new { companyId = access.CompanyId }, access);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SalesAccess access)
        {
            await _service.UpdateAsync(id, access);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPatch("company/{companyId}")]
        public async Task<IActionResult> PatchSalesAccessValue(string companyId, [FromBody] int salesAccessValue)
        {
            var access = await _service.GetByCompanyIdAsync(companyId);
            if (access == null) return NotFound();
            access.SalesAccessValue = salesAccessValue;
            await _service.UpdateAsync(access.Id!, access);
            return NoContent();
        }
    }
} 