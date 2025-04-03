using AlfaMart.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlfaMart.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Username { get; set; } // Keep as nullable
        [BindProperty]
        public string? Password { get; set; } // Keep as nullable
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == Username && u.Password == Password);

            if (user != null)
            {
                // Confirm role check and redirection
                if (user.Role == "Admin")
                {
                    return RedirectToPage("/Admin"); // Should redirect to Admin page
                }
                else if (user.Role == "Personnel")
                {
                    return RedirectToPage("/Cashier"); // Should redirect to Cashier page
                }
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}