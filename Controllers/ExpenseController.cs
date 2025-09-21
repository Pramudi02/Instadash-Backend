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
    public class ExpensesController : ControllerBase
    {
        private readonly ExpenseService _expenseService;

        public ExpensesController(ExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Expense>>> GetExpenses()
        {
            var expenses = await _expenseService.GetAllExpensesAsync();
            return Ok(expenses);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Expense>> GetExpense(string id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            
            if (expense == null)
                return NotFound();
                
            return Ok(expense);
        }

        [HttpPost]
        public async Task<ActionResult<Expense>> CreateExpense([FromForm] ExpenseDTO expenseDTO)
        {
            // Clear ModelState errors for optional fields
            ModelState.Remove("Description");
            ModelState.Remove("ReceiptFile");

            try
            {
                // Validate required input fields
                if (string.IsNullOrEmpty(expenseDTO.EmployeeName) || 
                    string.IsNullOrEmpty(expenseDTO.Position) ||
                    string.IsNullOrEmpty(expenseDTO.ExpenseType) || 
                    string.IsNullOrEmpty(expenseDTO.PaymentMethod) || 
                    string.IsNullOrEmpty(expenseDTO.Amount) || 
                    string.IsNullOrEmpty(expenseDTO.Date))
                {
                    return BadRequest("Required fields (EmployeeName, Position, ExpenseType, PaymentMethod, Amount, Date) cannot be empty.");
                }

                // Parse and validate Amount
                decimal amount;
                if (!decimal.TryParse(expenseDTO.Amount.Replace("Rs.", "").Trim(), out amount) || amount <= 0)
                {
                    return BadRequest("Invalid amount value.");
                }

                // Parse and validate Date
                DateTime date;
                if (!DateTime.TryParse(expenseDTO.Date, out date))
                {
                    return BadRequest("Invalid date format.");
                }

                // Create the expense object
                var expense = new Expense
                {
                    EmployeeName = expenseDTO.EmployeeName,
                    Position = expenseDTO.Position,
                    ExpenseType = expenseDTO.ExpenseType,
                    Amount = amount,
                    Date = date,
                    PaymentMethod = expenseDTO.PaymentMethod,
                    Description = expenseDTO.Description,
                    CompanyId = expenseDTO.CompanyId,
    HoneyCombId = expenseDTO.HoneyCombId
                };

                // Create the expense in the service
                var createdExpense = await _expenseService.CreateExpenseAsync(expense, expenseDTO.ReceiptFile);

                return CreatedAtAction(nameof(GetExpense), new { id = createdExpense.Id }, createdExpense);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateExpense(string id, [FromForm] ExpenseDTO expenseDTO)
        {
            // Clear ModelState errors for optional fields
            ModelState.Remove("Description");
            ModelState.Remove("ReceiptFile");

            try
            {
                // Validate required input fields
                if (string.IsNullOrEmpty(expenseDTO.EmployeeName) || 
                    string.IsNullOrEmpty(expenseDTO.Position) ||
                    string.IsNullOrEmpty(expenseDTO.ExpenseType) || 
                    string.IsNullOrEmpty(expenseDTO.PaymentMethod) || 
                    string.IsNullOrEmpty(expenseDTO.Amount) || 
                    string.IsNullOrEmpty(expenseDTO.Date))
                {
                    return BadRequest("Required fields (EmployeeName, Position, ExpenseType, PaymentMethod, Amount, Date) cannot be empty.");
                }

                // Parse and validate Amount
                decimal amount;
                if (!decimal.TryParse(expenseDTO.Amount.Replace("Rs.", "").Trim(), out amount) || amount <= 0)
                {
                    return BadRequest("Invalid amount value.");
                }

                // Parse and validate Date
                DateTime date;
                if (!DateTime.TryParse(expenseDTO.Date, out date))
                {
                    return BadRequest("Invalid date format.");
                }

                // Create the expense object
                var expense = new Expense
                {
                    Id = id,
                    EmployeeName = expenseDTO.EmployeeName,
                    Position = expenseDTO.Position,
                    ExpenseType = expenseDTO.ExpenseType,
                    Amount = amount,
                    Date = date,
                    PaymentMethod = expenseDTO.PaymentMethod,
                    Description = expenseDTO.Description
                };

                // Update the expense in the service
                await _expenseService.UpdateExpenseAsync(id, expense, expenseDTO.ReceiptFile);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteExpense(string id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            
            if (expense == null)
                return NotFound();
                
            await _expenseService.DeleteExpenseAsync(id);
            
            return NoContent();
        }

        [HttpGet("company/{companyId}")]
public async Task<ActionResult<List<Expense>>> GetByCompanyId(string companyId)
{
    var expenses = await _expenseService.GetExpensesByCompanyIdAsync(companyId);
    return Ok(expenses);
}


       [HttpGet("receipts/{fileName}")]
public async Task<IActionResult> GetReceipt(string fileName)
{
    // Decode URL-encoded fileName to get the correct file path
    var decodedFileName = Uri.UnescapeDataString(fileName);

    var filePath = await _expenseService.GetReceiptPathAsync(decodedFileName);

    if (!System.IO.File.Exists(filePath))
        return NotFound();

    var fileBytes = System.IO.File.ReadAllBytes(filePath);
    return File(fileBytes, "application/pdf", decodedFileName);
}
    }


    public class ExpenseDTO
    {
        public string EmployeeName { get; set; }
        public string Position { get; set; }
        public string ExpenseType { get; set; }
        public string Amount { get; set; }
        public string Date { get; set; }
        public string PaymentMethod { get; set; }
        public string? Description { get; set; }
        public IFormFile? ReceiptFile { get; set; }
        public string CompanyId { get; set; }
public string HoneyCombId { get; set; }

    }
}