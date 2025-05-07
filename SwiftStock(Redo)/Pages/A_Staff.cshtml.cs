using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    public class A_StaffModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public A_StaffModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MUser> Staff { get; set; }

        public async Task OnGetAsync()
        {
            Staff = await _context._musers
                .Where(u => u.Role == "Personnel" || u.Role == "Admin")
                .ToListAsync();
        }
    }
}
