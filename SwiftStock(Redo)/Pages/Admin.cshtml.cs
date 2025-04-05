using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlfaMart.Pages
{
    [Authorize(Roles = "Admin")] // Ensure only admins can access this page
    public class AdminModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return new JsonResult(new
                {
                    accessDenied = true,
                    message = "Access denied. Please log in with an admin account."
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Login");
        }
    }
}