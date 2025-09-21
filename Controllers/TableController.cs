using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services; 

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TableController : ControllerBase
    {
        private readonly Auditservice _auditservice;

        public TableController(Auditservice auditservice)
        {
            _auditservice = auditservice;
        }

        // GET: api/table
        // Get all table
        [HttpGet("table")]
        public async Task<ActionResult<List<Table>>> GetFilteredAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var records = await _auditservice.GetFilteredAsync(from, to);
            return Ok(records ?? new List<Table>());
        }


        [HttpGet("totals")]
        public async Task<IActionResult> GetTotalSums(string from = "", string to = "")
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                from = DateTime.MinValue.ToString("yyyy-MM-dd");
                to = DateTime.Now.ToString("yyyy-MM-dd");
            }

            var totals = await _auditservice.GetTotalSumsAsync(from, to);
            return Ok(totals);
        }

        
        [HttpGet("tax-sum/last-3-years")]
        public async Task<IActionResult> Getlastthreetax()
        {
            var summary = await _auditservice.GetlastthreetaxAsync();
            return Ok(summary);
        }

        [HttpGet("audit-stats")]
        public async Task<IActionResult> GetAuditStatistics()
        {
            var stats = await _auditservice.GetAuditStatisticsByYearAsync();
            return Ok(stats);
        }
    }
}        