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
            Console.WriteLine($"Username: {Username}, Password: {Password}, Email: {Email}");

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Email))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Validate email format
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Invalid email format.";
                return Page();
            }

            // Check if the email already exists in the users table
            var existingEmail = await _context.users.FirstOrDefaultAsync(u => u.Email == Email); // Ensure 'Users' matches your DbSet name
            if (existingEmail != null)
            {
                ErrorMessage = "Email is already registered.";
                return Page();
            }

            // Check if the username already exists in the users table
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Username == Username); // Ensure 'Users' matches your DbSet name
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

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

            // Create new user
            var user = new User
            {
                Email = Email,
                Name = Name,
                Username = Username,
                Password = hashedPassword,
                Role = "Customer"
            };

            _context.users.Add(user); // Ensure 'Users' matches your DbSet name
            await _context.SaveChangesAsync();

            // Redirect to the Login page
            return RedirectToPage("/Login"); // Redirect to the Login page
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

