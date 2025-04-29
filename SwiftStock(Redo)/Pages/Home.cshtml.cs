using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data; // For ApplicationDbContext
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HomeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // This property is what the Razor page needs
        public List<InventoryItem> InventoryItem { get; set; } = new List<InventoryItem>();

        public async Task OnGetAsync()
        {
            InventoryItem = await _context.inventory.ToListAsync();
        }
    }
}
