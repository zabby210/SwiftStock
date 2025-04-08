using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftStock.Data;
using SwiftStock.Models;
using System.Security.Cryptography;
using System.Text;

namespace AlfaMart.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private static readonly string Key = "b14ca5898a4e4133bbce2ea2315a1916"; // 32 chars = 256 bits

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

        // Helper method to encrypt password using AES
        private string EncryptPassword(string password)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(password);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Name) || 
                string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
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

            try
            {
                // Create a new consumer with encrypted password
                var consumer = new Consumer
                {
                    Email = Email,
                    Name = Name,
                    Username = Username,
                    Password = EncryptPassword(Password) // Encrypt password before storing
                };

                _context.consumer.Add(consumer);
                _context.SaveChanges();

                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during registration. Please try again.";
                return Page();
            }
        }
    }
}

