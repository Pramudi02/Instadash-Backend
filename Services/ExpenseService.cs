using Backend.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Backend.Services
{
    public class ExpenseService
    {
        private readonly MongoDBService _mongoDBService;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadDirectory;

        public ExpenseService(MongoDBService mongoDBService, IWebHostEnvironment environment)
        {
            _mongoDBService = mongoDBService;
            _environment = environment;
            
            // Create directory for file uploads if it doesn't exist
            _uploadDirectory = Path.Combine(_environment.ContentRootPath, "Uploads", "Receipts");
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            return await _mongoDBService.GetAllExpensesAsync();
        }

        public async Task<Expense> GetExpenseByIdAsync(string id)
        {
            return await _mongoDBService.GetExpenseByIdAsync(id);
        }

        public async Task<Expense> CreateExpenseAsync(Expense expense, IFormFile? receiptFile)
        {
            // Handle receipt file if provided
            if (receiptFile != null)
            {
                // Save file to disk
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(receiptFile.FileName)}";
                string filePath = Path.Combine(_uploadDirectory, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await receiptFile.CopyToAsync(stream);
                }

                // Set the receipt URL in the expense object
                expense.ReceiptUrl = $"/api/expenses/receipts/{uniqueFileName}";
            }

            // Create the expense in the database
            await _mongoDBService.CreateExpenseAsync(expense);
            return expense;
        }

                public async Task<string> GetReceiptPathAsync(string fileName)
            {
                return await Task.Run(() => Path.Combine(_uploadDirectory, fileName));
            }


        public async Task UpdateExpenseAsync(string id, Expense expense, IFormFile? receiptFile)
        {
            var existingExpense = await _mongoDBService.GetExpenseByIdAsync(id);
            
            if (existingExpense == null)
                throw new Exception("Expense not found");
                
            if (receiptFile != null)
            {
                // Delete old receipt if exists
                if (!string.IsNullOrEmpty(existingExpense.ReceiptUrl))
                {
                    string oldFileName = Path.GetFileName(existingExpense.ReceiptUrl);
                    string oldFilePath = Path.Combine(_uploadDirectory, oldFileName);
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }
                
                // Save new file
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(receiptFile.FileName)}";
                string filePath = Path.Combine(_uploadDirectory, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await receiptFile.CopyToAsync(stream);
                }
                
                expense.ReceiptUrl = $"/api/expenses/receipts/{uniqueFileName}";
            }
            else
            {
                // Keep old receipt URL if no new file uploaded
                expense.ReceiptUrl = existingExpense.ReceiptUrl;
            }
            
            await _mongoDBService.UpdateExpenseAsync(id, expense);
        }

        public async Task DeleteExpenseAsync(string id)
        {
            var expense = await _mongoDBService.GetExpenseByIdAsync(id);
            
            if (expense != null && !string.IsNullOrEmpty(expense.ReceiptUrl))
            {
                string fileName = Path.GetFileName(expense.ReceiptUrl);
                string filePath = Path.Combine(_uploadDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            
            await _mongoDBService.DeleteExpenseAsync(id);
        }

       public async Task<List<Expense>> GetExpensesByCompanyIdAsync(string companyId)
{
    return await _mongoDBService.GetExpensesByCompanyIdAsync(companyId);
}


    }
}