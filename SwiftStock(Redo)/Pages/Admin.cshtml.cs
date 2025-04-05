using AlfaMart.Data; // Ensure this namespace is correct
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlfaMart.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return new JsonResult(new { accessDenied = true, message = "You must be logged in to access this page." });
            }

            var user = _context.users.FirstOrDefault(u => u.Username == username);
            if (user == null || user.Role != "Admin")
            {
                return new JsonResult(new { accessDenied = true, message = "You do not have permission to access this page." });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }
    }
}

