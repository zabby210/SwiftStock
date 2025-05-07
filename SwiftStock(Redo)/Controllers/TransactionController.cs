using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ApplicationDbContext context, ILogger<TransactionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("SaveTransaction")]
        public async Task<IActionResult> SaveTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
            {
                _logger.LogError("Transaction data is null");
                return BadRequest(new { success = false, error = "Invalid transaction data." });
            }

            try
            {
                // Log the incoming data
                _logger.LogInformation($"Saving transaction: Name={transaction.Name}, Products={transaction.Products}, Quantity={transaction.Quantity}, Total={transaction.Total}, Date={transaction.Transaction_Date}");

                // Validate the data
                if (string.IsNullOrEmpty(transaction.Name))
                {
                    return BadRequest(new { success = false, error = "Name is required." });
                }

                if (string.IsNullOrEmpty(transaction.Products))
                {
                    return BadRequest(new { success = false, error = "Products are required." });
                }

                if (transaction.Quantity <= 0)
                {
                    return BadRequest(new { success = false, error = "Quantity must be greater than 0." });
                }

                if (transaction.Total <= 0)
                {
                    return BadRequest(new { success = false, error = "Total must be greater than 0." });
                }

                // Ensure transaction date is set
                if (transaction.Transaction_Date == default)
                {
                    transaction.Transaction_Date = DateTime.Now;
                }

                // Log the entity state before saving
                var entry = _context.Entry(transaction);
                _logger.LogInformation($"Entity State: {entry.State}");
                _logger.LogInformation($"Entity Properties: {string.Join(", ", entry.Properties.Select(p => $"{p.Metadata.Name}: {p.CurrentValue}"))}");

                // Add the transaction to the database
                _context.transaction.Add(transaction);
                
                try
                {
                    // Log the SQL that will be executed
                    var sql = _context.transaction.ToQueryString();
                    _logger.LogInformation($"SQL Query: {sql}");

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Transaction saved successfully with ID: {transaction.Id}");
                    return Ok(new { success = true, id = transaction.Id });
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database update error: {Message}", dbEx.Message);
                    if (dbEx.InnerException != null)
                    {
                        _logger.LogError(dbEx.InnerException, "Inner exception: {Message}", dbEx.InnerException.Message);
                        _logger.LogError(dbEx.InnerException, "Stack trace: {StackTrace}", dbEx.InnerException.StackTrace);
                    }
                    return StatusCode(500, new { success = false, error = $"Database error: {dbEx.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transaction: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                    _logger.LogError(ex.InnerException, "Stack trace: {StackTrace}", ex.InnerException.StackTrace);
                }
                return StatusCode(500, new { success = false, error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}