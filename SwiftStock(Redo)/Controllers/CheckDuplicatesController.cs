using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;

namespace AlfaMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckDuplicatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CheckDuplicatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CheckDuplicatesRequest request)
        {
            var emailExists = await _context.users.AnyAsync(u => u.Email == request.Email);
            var usernameExists = await _context.users.AnyAsync(u => u.Username == request.Username);

            return Ok(new { usernameExists, emailExists });
        }
    }

    public class CheckDuplicatesRequest
    {
        public string? Name { get; set; }
        public string? Username { get; set; }
        public required string Email { get; set; }
    }
}