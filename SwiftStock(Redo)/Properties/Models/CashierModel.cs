using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using Microsoft.AspNetCore.Authorization;

namespace SwiftStock.Pages
{
    // Allow both Admin and Cashier roles to access this page
    [Authorize(Roles = "Personnel")]
    public class CashierModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CashierModel> _logger;

        public CashierModel(ApplicationDbContext context, ILogger<CashierModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<InventoryItem> InventoryItems { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                InventoryItems = await _context.inventory
                    .OrderBy(i => i.Product_Name)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cashier page");
                return Page();
            }
        }
    }
}