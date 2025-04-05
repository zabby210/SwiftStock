using AlfaMart.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

            // Check in the users table
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.Username == Username && u.Password == Password);

            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) // Important for [Authorize(Roles = "Admin")]
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (user.Role == "Admin")
                {
                    return RedirectToPage("/Admin");
                }
                else if (user.Role == "Personnel")
                {
                    return RedirectToPage("/Cashier");
                }
            }


            {

                var consumer = await _context.consumer
                    .FirstOrDefaultAsync(c => c.Username == Username && c.Password == Password);

                if (consumer != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, consumer.Username),

            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToPage("/Home");
                }

                ErrorMessage = "Invalid username or password.";
                return Page();
            }
        }
    }
}



