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
                Console.Error.WriteLine("Transaction data is null.");
                return BadRequest(new { success = false, error = "Invalid transaction data." });
            }

            try
            {
                // Log transaction details for debugging
                Console.WriteLine($"Saving transaction: Name={transaction.Name}, Products={transaction.Products}, Quantity={transaction.Quantity}, Total={transaction.Total}, Date={transaction.Transaction_Date}");

                // Save the transaction to the database
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving transaction: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An unexpected error occurred while saving the transaction." });
            }
        }
    }
}
