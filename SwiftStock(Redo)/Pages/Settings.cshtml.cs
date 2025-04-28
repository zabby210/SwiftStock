using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using System.ComponentModel.DataAnnotations;
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

        // Properties to bind user input
        [BindProperty]
        public User User { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; }

        [BindProperty]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Flags for validation messages
        public string StatusMessage { get; set; }
        public bool IsError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            var user = await _context.users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return RedirectToPage("/Error");
            }

            User = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                IsError = true;
                StatusMessage = "Please fix the errors and try again.";
                return Page();
            }

            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return RedirectToPage("/Error");
            }

            // Check if username exists (except for current user)
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Username == User.Username && u.Id != user.Id);
            if (existingUser != null)
            {
                IsError = true;
                StatusMessage = "Username already exists.";
                return Page();
            }

            bool passwordChanged = false;

            // Handle password change logic
            if (!string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                // Require current password when changing password
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    IsError = true;
                    StatusMessage = "Current password is required to set a new password.";
                    return Page();
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.Password))
                {
                    IsError = true;
                    StatusMessage = "Current password is incorrect.";
                    return Page();
                }

                // Confirmation check (redundant with attribute validation but keeping as a safety)
                if (NewPassword != ConfirmPassword)
                {
                    IsError = true;
                    StatusMessage = "New password and confirmation do not match.";
                    return Page();
                }

                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                passwordChanged = true;
            }

            // Update basic user details
            user.Username = User.Username;
            user.Name = User.Name;
            user.Email = User.Email;

            _context.users.Update(user);
            await _context.SaveChangesAsync();

            // Update cookie claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }
            );

            // Set success message
            StatusMessage = passwordChanged
                ? "Your profile and password have been updated successfully."
                : "Your profile has been updated successfully.";

            return RedirectToPage("/Account/Settings", new { statusMessage = StatusMessage });
        }

        public void OnGetAsync(string statusMessage = null)
        {
            StatusMessage = statusMessage;
        }
    }
}