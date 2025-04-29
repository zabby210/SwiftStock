using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using System.ComponentModel.DataAnnotations;

namespace SwiftStock.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SettingsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        public User? User { get; set; }

        public bool UsernameExists { get; set; } = false;
        public bool InvalidCurrentPassword { get; set; } = false;
        public bool PasswordsDoNotMatch { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            // Simulate logged-in user; you can replace this with session-based logic
            var currentUserId = 1;

            User = await _context.users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (User == null)
                return NotFound();

            Name = User.Name;
            Username = User.Username;
            Email = User.Email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUserId = 1; // replace with real user ID logic

            User = await _context.users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (User == null)
                return NotFound();

            // Username check
            if (Username != User.Username)
            {
                var usernameTaken = await _context.users.AnyAsync(u => u.Username == Username);
                if (usernameTaken)
                {
                    UsernameExists = true;
                    return Page();
                }
            }

            // Password check and update
            if (!string.IsNullOrEmpty(Password))
            {
                if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, User.Password))
                {
                    InvalidCurrentPassword = true;
                    return Page();
                }

                if (Password != ConfirmPassword)
                {
                    PasswordsDoNotMatch = true;
                    return Page();
                }

                User.Password = BCrypt.Net.BCrypt.HashPassword(Password);
            }

            // Update other info
            User.Name = Name;
            User.Username = Username;
            User.Email = Email;

            _context.users.Update(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard");
        }
    }
}
