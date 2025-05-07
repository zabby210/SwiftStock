using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    public class CustomerAdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CustomerAdminModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MUser> Users { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _context._musers
                .Where(u => u.Role == "Customer")
                .ToListAsync();
        }
    }
}
