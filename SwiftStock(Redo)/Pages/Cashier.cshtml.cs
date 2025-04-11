// Pages/Cashier.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public CashierModel CashierData { get; set; } = new CashierModel();

        // Define the InventoryItems property
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
    }
}