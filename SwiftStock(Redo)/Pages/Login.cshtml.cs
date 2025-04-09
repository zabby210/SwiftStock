using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using System.Security.Claims;

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
        public string? Username { get; set; }
        [BindProperty]
        public string? Password { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            try
            {
                Console.WriteLine($"Attempting to log in with username: {Username}");

                // Check in the users table for both users and personnel
                var user = await _context.users.FirstOrDefaultAsync(u => u.Username == Username);
                if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.Password))
                {
                    // Successful login for users and personnel
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role) // Retain role for users and personnel
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Redirect based on user role
                    return user.Role switch
                    {
                        "Admin" => RedirectToPage("/Admin"),
                        "Personnel" => RedirectToPage("/Cashier"),
                        "Customer" => RedirectToPage("/Home"),
                        _ => RedirectToPage("/Home")
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                ErrorMessage = "An error occurred while logging in.";
                return Page();
            }

            // If we reach here, login failed
            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}





