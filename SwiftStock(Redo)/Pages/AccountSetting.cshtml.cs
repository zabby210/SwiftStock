using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SwiftStock.Pages
{
    public class AccountSettingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AccountSettingModel(ApplicationDbContext context)
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

        public User CurrentUser { get; set; }
        public bool UsernameExists { get; set; } = false;
        public bool InvalidCurrentPassword { get; set; } = false;
        public bool PasswordsDoNotMatch { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToPage("/Login");
            }

            CurrentUser = await _context.users.FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (CurrentUser == null)
            {
                TempData["ErrorMessage"] = "User data could not be found. Please try again later.";
                return Page();
            }

            Name = CurrentUser.Name;
            Username = CurrentUser.Username;
            Email = CurrentUser.Email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToPage("/Login");
            }

            CurrentUser = await _context.users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (CurrentUser == null)
            {
                TempData["ErrorMessage"] = "User data could not be found.";
                return Page();
            }

            if (Username != CurrentUser.Username)
            {
                var usernameTaken = await _context.users.AnyAsync(u => u.Username == Username && u.Id != currentUserId);
                if (usernameTaken)
                {
                    UsernameExists = true;
                    return Page();
                }
            }

            if (!string.IsNullOrEmpty(Password))
            {
                if (string.IsNullOrEmpty(CurrentPassword) || !BCrypt.Net.BCrypt.Verify(CurrentPassword, CurrentUser.Password))
                {
                    InvalidCurrentPassword = true;
                    return Page();
                }

                if (Password != ConfirmPassword)
                {
                    PasswordsDoNotMatch = true;
                    return Page();
                }

                CurrentUser.Password = BCrypt.Net.BCrypt.HashPassword(Password);
            }

            CurrentUser.Name = Name;
            CurrentUser.Username = Username;
            CurrentUser.Email = Email;

            _context.users.Update(CurrentUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your account settings have been updated successfully!";
            return RedirectToPage("/AccountSetting");
        }
    }
}
