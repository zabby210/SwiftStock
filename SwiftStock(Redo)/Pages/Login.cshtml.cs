using AlfaMart.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        public string? Username { get; set; } // For both user and consumer tables
        [BindProperty]
        public string? Password { get; set; } // Keep as nullable
        public string ErrorMessage { get; set; } = string.Empty; // Initialize ErrorMessage

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            // Hash the input password
            using (var sha256 = SHA256.Create())
            {
                var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(Password)));

                // Check in the users table
                var user = await _context.users
                    .FirstOrDefaultAsync(u => u.Username == Username && u.Password == hashedPassword);

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

                // Check in the consumer table
                var consumer = await _context.consumer
                    .FirstOrDefaultAsync(c => c.Username == Username && c.Password == hashedPassword);

                if (consumer != null)
                {
                    // Redirect to the home page for consumers
                    return RedirectToPage("/Home");
                }

                ErrorMessage = "Invalid username or password.";
                return Page();
            }
        }
    }
}


