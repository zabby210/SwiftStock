using Microsoft.AspNetCore.Authorization; // For [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftStock.Data;
using SwiftStock.Models;

namespace SwiftStock.Pages.Account
{
    // Ensure that users are authorized to access the settings page
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SettingsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; }

        public bool UserNotFound { get; set; }

        public IActionResult OnGet()
        {
            // Access the logged-in user's username from HttpContext.User.Identity.Name
            var userName = HttpContext.User.Identity.Name; // This gives the logged-in user's username

            // If no username is found (meaning the user is not authenticated), redirect to login
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToPage("/Login"); // Redirect to login if the user is not authenticated
            }

            // Find the user in the database based on their username
            User = _context.users.FirstOrDefault(u => u.Username == userName);

            if (User == null)
            {
                UserNotFound = true;
                return Page(); // Stay on the same page and show an error if user is not found.
            }

            return Page(); // Return the page with the user's data.
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            // Find the user in the database by their Id
            var existingUser = _context.users.FirstOrDefault(u => u.Id == User.Id);

            if (existingUser != null)
            {
                existingUser.Name = User.Name;
                existingUser.Username = User.Username;
                existingUser.Email = User.Email;

                if (!string.IsNullOrEmpty(User.Password))
                {
                    existingUser.Password = User.Password; // Update the password (hash in production)
                }

                _context.SaveChanges();
            }

            return RedirectToPage("/Home");
        }
    }
}
