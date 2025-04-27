using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data; // Ensure this matches your ApplicationDbContext namespace
using System.Security.Claims;
using System.ComponentModel.DataAnnotations; // Add this!

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
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // No need to check ModelState unless you add [Required]
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

            if (!BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                ErrorMessage = "Invalid login attempt.";
                return Page();
            }

            // Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

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
