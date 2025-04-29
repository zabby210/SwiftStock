using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    public class SalesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SalesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Sale> Sales { get; set; } = new List<Sale>();

        public async Task OnGetAsync()
        {
            Sales = await _context.sales
                .Include(s => s.InventoryItem)
                .ToListAsync();
        }
    }
}
