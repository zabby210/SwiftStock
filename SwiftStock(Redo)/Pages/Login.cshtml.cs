using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AlfaMart.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private static readonly string Key = "b14ca5898a4e4133bbce2ea2315a1916"; // Keep this key

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string? Username { get; set; }
        [BindProperty]
        public string? Password { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        private string EncryptPassword(string password)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

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

        private string DecryptPassword(string encryptedPassword)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(encryptedPassword);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            // Check in the users table (admin/personnel) - plain text
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.Username == Username && u.Password == Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (user.Role == "Admin")
                {
                    return RedirectToPage("/Admin");
                }
                else if (user.Role == "Personnel")
                {
                    return RedirectToPage("/Cashier");
                }
            }

            // First find the consumer by username only
            var consumer = await _context.consumer
                .FirstOrDefaultAsync(c => c.Username == Username);

            if (consumer != null)
            {
                try
                {
                    // Get the stored encrypted password and decrypt it
                    string decryptedStoredPassword = DecryptPassword(consumer.Password);

                    // Compare the decrypted password with input password
                    if (decryptedStoredPassword == Password)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, consumer.Username),
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return RedirectToPage("/Home");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error if needed
                    Console.WriteLine($"Decryption error: {ex.Message}");
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}


