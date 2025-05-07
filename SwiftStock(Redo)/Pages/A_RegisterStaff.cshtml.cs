using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;
using System.ComponentModel.DataAnnotations;

namespace SwiftStock.Pages
{
    public class A_RegisterStaffModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public A_RegisterStaffModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Role))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Check if the username already exists
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Username == Username);
            if (existingUser != null)
            {
                ErrorMessage = "Username is already taken.";
                return Page();
            }

            if (!IsValidUsername(Username) || !IsValidName(Name))
            {
                ErrorMessage = "Username and Name must not contain special characters.";
                return Page();
            }

            // Validate role
            if (Role != "Admin" && Role != "Personnel")
            {
                ErrorMessage = "Invalid role selected.";
                return Page();
            }

            //bcrypt encryption
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

            // Create new staff user with default email
            var user = new User
            {
                Username = Username,
                Password = hashedPassword,
                Name = Name,
                Role = Role,
                Email = $"{Username}@swiftstock.com" // Default email based on username
            };

            _context.users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToPage("/A_Staff");
        }

        private bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-' || c == '\'');
        }
    }
}
