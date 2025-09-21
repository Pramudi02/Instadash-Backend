using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutomationController : ControllerBase
    {
        private readonly AutomationService _automationService;

        public AutomationController(AutomationService automationService)
        {
            _automationService = automationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Automation>>> Get()
        {
            var automations = await _automationService.GetAllAsync();
            return Ok(automations);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Automation>> Get(string id)
        {
            var automation = await _automationService.GetByIdAsync(id);
            if (automation == null)
                return NotFound();

            return Ok(automation);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Automation automation)
        {
            await _automationService.CreateAsync(automation);
            return CreatedAtAction(nameof(Get), new { id = automation.Id }, automation);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, [FromBody] Automation automation)
        {
            var existing = await _automationService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            automation.Id = id;
            await _automationService.UpdateAsync(id, automation);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var automation = await _automationService.GetByIdAsync(id);
            if (automation == null)
                return NotFound();

            await _automationService.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("company/{companyId}")]
public async Task<ActionResult<List<Automation>>> GetByCompanyId(string companyId)
{
    var automations = await _automationService.GetByCompanyIdAsync(companyId);
    return Ok(automations);
}

    }
}
