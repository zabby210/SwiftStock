using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace AlfaMart.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Email { get; set; }
        [BindProperty]
        public string? Name { get; set; }
        [BindProperty]
        public string? Username { get; set; }
        [BindProperty]
        public string? Password { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Check for required fields
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
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
            var existingEmail = await _context.users.FirstOrDefaultAsync(u => u.Email == Email);
            if (existingEmail != null)
            {
                ErrorMessage = "Email is already registered.";
                return Page();
            }

            // Check if the username already exists in the users table
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Username == Username);
            if (existingUser != null)
            {
                ErrorMessage = "Username is already taken.";
                return Page();
            }

            // Validate username and name
            if (!IsValidUsername(Username) || !IsValidName(Name))
            {
                ErrorMessage = "Username and Name must not contain special characters.";
                return Page();
            }

            // Hash the password using BCrypt
            var hashedPassword = HashPassword(Password);

            // Create a new user
            var newUser = new User
            {
                Email = Email,
                Name = Name,
                Username = Username,
                Password = hashedPassword,
                Role = "Customer" // Set default role to Customer
            };

            _context.users.Add(newUser); // Add to users table
            Console.WriteLine("Attempting to save user to database.");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user: {ex.Message}");
                ErrorMessage = "An error occurred while creating the account.";
                return Page();
            }
            Console.WriteLine("User saved successfully.");

            Console.WriteLine($"Email: {Email}, Name: {Name}, Username: {Username}, Password: {Password}");

          return RedirectToPage("/Login");
        }

        // Helper methods for validation
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

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-' || c == '\'');
        }

        private bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}

