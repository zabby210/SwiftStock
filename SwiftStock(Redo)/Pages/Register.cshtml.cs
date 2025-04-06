using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public string Email { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Check if the username or email already exists
            if (_context.consumer.Any(c => c.Username == Username || c.Email == Email))
            {
                ErrorMessage = "Username or Email already exists.";
                return Page();
            }


            {


                // Create a new consumer
                var consumer = new Consumer
                {
                    Email = Email,
                    Name = Name,
                    Username = Username,
                    Password = Password // Store the hashed password
                };

                _context.consumer.Add(consumer);
                _context.SaveChanges();
            }


            return RedirectToPage("/Login");
        }
    }
}

