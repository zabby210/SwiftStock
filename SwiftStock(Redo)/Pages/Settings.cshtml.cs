using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using System.Security.Claims;

namespace SwiftStock.Pages.Account
{
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SettingsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Properties to bind user data
        public User User { get; set; }
        public bool UsernameExists { get; set; } // To check if the username already exists

        // OnGet method to retrieve user information
        public async Task OnGetAsync()
        {
            // Get user ID from the authenticated user (ClaimsPrincipal)
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);  // Access ClaimsPrincipal via HttpContext

            // Retrieve the user from the database by Id
            User = await _context.users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            // If the user does not exist, handle the error
            if (User == null)
            {
                RedirectToPage("/Error");
            }
        }

        // OnPost method to handle profile updates
        public async Task<IActionResult> OnPostAsync(string username, string name, string email, string password)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if the username already exists
            var existingUser = await _context.users
                .FirstOrDefaultAsync(u => u.Username == username && u.Id != User.Id);

            if (existingUser != null)
            {
                // If username exists, set the flag to show an error
                UsernameExists = true;
                return Page();
            }

            // Update user's information (except for password unless provided)
            User.Username = username;
            User.Name = name;
            User.Email = email;

            if (!string.IsNullOrEmpty(password)) // Only update password if provided
            {
                // Ideally, you should hash the password here before saving it
                User.Password = password;
            }

            // Save changes to the database
            _context.users.Update(User);
            await _context.SaveChangesAsync();

            // Redirect to a confirmation page or back to the settings page
            return RedirectToPage("/Account/Settings");
        }
    }
}
