using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;

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
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Check if the user exists using only the username
            var user = await _context.users.FirstOrDefaultAsync(u => u.Username == Username);
            if (user == null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            // Log the retrieved password hash for debugging
            Console.WriteLine($"Retrieved password hash: {user.Password}");

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            // Redirect based on user role
            switch (user.Role)
            {
                case "Admin":
                    return RedirectToPage("/Admin"); // Redirect to admin page
                case "Personnel":
                    return RedirectToPage("/Cashier"); // Redirect to cashier page
                case "Customer":
                    return RedirectToPage("/Home"); // Redirect to home page
                default:
                    ErrorMessage = "User is not recognized.";
                    return Page(); // Handle unexpected roles
            }
        }
    }
}