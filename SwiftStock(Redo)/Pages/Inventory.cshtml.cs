// File: Pages/Inventory.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    [Authorize(Roles = "Admin")]
    public class InventoryItemModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryItemModel> _logger;

        public InventoryItemModel(ApplicationDbContext context, ILogger<InventoryItemModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<InventoryItem> InventoryItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                InventoryItem = await _context.inventory
                    .OrderBy(i => i.Product_Name)
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory");
                TempData["ErrorMessage"] = "Error loading inventory. Please try again later.";
                return Page();
            }
        }
    }
}
