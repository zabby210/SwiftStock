// File: Pages/Inventory.cshtml.cs
using AlfaMart.Data;
using AlfaMart.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlfaMart.Pages
{
    public class InventoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InventoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<InventoryItem> InventoryItems { get; set; }


        public async Task OnGetAsync()
        {
            InventoryItems = await _context.inventory.ToListAsync(); // Updated to match the DbSet property name
        }
    }

}
