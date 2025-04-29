using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;

namespace SwiftStock.Properties.Models
{
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HomeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InventoryItem> InventoryItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            InventoryItems = await _context.inventory
                .Where(i => i.Stock > 0) // Only show items with stock
                .OrderBy(i => i.Product_Name)
                .ToListAsync();
        }
    }
}