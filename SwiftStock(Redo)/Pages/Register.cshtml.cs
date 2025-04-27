using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;
using System.ComponentModel.DataAnnotations;

namespace AlfaMart.Pages
{

    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context; // Use swiftstockdb as your DbContext

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

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

        public string ErrorMessage { get; set; } // Define ErrorMessage property

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Check if the email already exists
            var existingEmail = await _context.users.FirstOrDefaultAsync(u => u.Email == Email);
            if (existingEmail != null)
            {
                ErrorMessage = "Email is already registered.";
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

            //bcrypt encryption :)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

            // Create new user without hashing the password
            var user = new User
            {
                Username = this.Username,
                Password = hashedPassword,
                Email = this.Email,
                Name = this.Name,
                Role = "Customer"
            };

            _context.users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Login");
        }

        // Implement the validation methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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

