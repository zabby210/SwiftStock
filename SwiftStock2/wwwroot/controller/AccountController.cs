using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly string connectionString = "server=localhost;database=swiftstockdb;user=root;password=;";

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            return BadRequest(new { success = false, message = "Username and Password are required!" });
        }

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT password, role FROM users WHERE username = @username";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", model.Username);

            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
            {
                return Unauthorized(new { success = false, message = "Invalid username or password." });
            }

            string storedPassword = reader["password"].ToString();
            string role = reader["role"].ToString();

            if (!VerifyPassword(model.Password, storedPassword))
            {
                return Unauthorized(new { success = false, message = "Invalid username or password." });
            }

            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("Username", model.Username);

            Response.Cookies.Append("AuthToken", GenerateToken(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { success = true, role, message = "Login Successful!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred.", error = ex.Message });
        }
    }

    [HttpGet("GetUserRole")]
    public IActionResult GetUserRole()
    {
        var role = HttpContext.Session.GetString("UserRole");
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { success = false, message = "User not logged in." });
        }

        return Ok(new { success = true, username, role });
    }

    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        try
        {
            HttpContext.Session.Clear();

            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies.Delete("AuthToken");
            }

            return Ok(new { success = true, message = "Logged out successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Logout failed.", error = ex.Message });
        }
    }

    [HttpGet("HealthCheck")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "Running" });
    }

    private static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
    {
        byte[] hashBytes = Convert.FromBase64String(storedHashedPassword);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        using var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(20);

        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash[i]) return false;
        }
        return true;
    }

    private static string GenerateToken()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] tokenData = new byte[32];
        rng.GetBytes(tokenData);
        return Convert.ToBase64String(tokenData);
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
