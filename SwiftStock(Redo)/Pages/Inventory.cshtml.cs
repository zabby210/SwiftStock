// File: Pages/Inventory.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages
{
    [Authorize(Roles = "Admin")]
    public class InventoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryModel> _logger;

        public InventoryModel(ApplicationDbContext context, ILogger<InventoryModel> logger)
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
                _logger.LogError(ex, "Error loading inventory");
                TempData["ErrorMessage"] = "Error loading inventory. Please try again later.";
                return Page();
            }
        }
    }
}
