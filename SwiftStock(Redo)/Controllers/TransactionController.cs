using Microsoft.AspNetCore.Mvc;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("SaveTransaction")]
        public async Task<IActionResult> SaveTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
            {
                return BadRequest(new { success = false, error = "Invalid transaction data." });
            }

            try
            {
                // Log the incoming data
                Console.WriteLine($"Transaction Data: {transaction.Name}, {transaction.Products}, {transaction.Quantity}, {transaction.Total}, {transaction.Transaction_Date}");

                // Add the transaction to the database
                _context.transaction.Add(transaction);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

    }
}