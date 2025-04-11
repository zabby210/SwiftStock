using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data; // Ensure this matches your ApplicationDbContext namespace
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
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _context.users.FirstOrDefaultAsync(u => u.Username == Username);
            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            // Verify the entered password against the stored plain text password
            if (Password != user.Password) // Direct comparison
            {
                ErrorMessage = "Invalid login attempt.";
                return Page();
            }

            // Create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) // Assuming you have a Role property
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Set authentication cookie
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Set to true if you want the cookie to persist across sessions
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Set cookie expiration
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // Redirect based on role
            return user.Role switch
            {
                "Admin" => RedirectToPage("/Admin"),
                "Personnel" => RedirectToPage("/Cashier"),
                "Customer" => RedirectToPage("/Home"),
                _ => RedirectToPage("/Home"),
            };
        }
    }
}