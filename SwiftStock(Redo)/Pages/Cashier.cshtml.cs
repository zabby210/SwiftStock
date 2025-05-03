// Pages/Cashier.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    // Allow both Admin and Cashier roles to access this page
    [Authorize(Roles = "Personnel")]
    public class CashierPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CashierPageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InventoryItem> InventoryItems { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                InventoryItems = await _context.inventory
                    .Where(i => i.Stock > 0) // Only show items with stock
                    .OrderBy(i => i.Product_Name)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                // Handle the error (e.g., log it)
                return Page();
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostSaveTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest("Invalid transaction data.");
            }

            try
            {
                // Add the transaction to the database
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                // Return an error response if something goes wrong
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}