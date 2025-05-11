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
            => (_context, _logger) = (context, logger);

        [HttpPost("SaveTransaction")]
        public async Task<IActionResult> SaveTransaction([FromBody] TransactionRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Transaction data is null");
                return BadRequest(new { success = false, error = "Invalid transaction data." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create and save the main transaction first
                var transactionEntity = new Transaction
                {
                    Personnel_ID = request.PersonnelId,
                    Total = request.Total,
                    Transaction_Date = DateTime.Now
                };

                _context.transaction.Add(transactionEntity);
                await _context.SaveChangesAsync();

                // Create and save transaction items
                foreach (var item in request.Items)
                {
                    var product = await _context.inventory.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Product with ID {item.ProductId} not found");
                    }

                    // Create transaction item following the exact table structure
                    var transactionItem = new TransactionItem
                    {
                        Transaction_ID = transactionEntity.Id,  // Matches the table's Transaction_ID
                        Product_ID = item.ProductId,           // Matches the table's Product_ID
                        Quantity = item.Quantity,              // Matches the table's Quantity
                        Subtotal = item.Subtotal              // Matches the table's Subtotal
                    };

                    _context.Set<TransactionItem>().Add(transactionItem);

                    // Update inventory
                    product.Stock -= item.Quantity;
                    if (product.Stock < 0) product.Stock = 0;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Transaction saved successfully. ID: {TransactionId}", transactionEntity.Id);
                return Ok(new { success = true, id = transactionEntity.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving transaction");
                return StatusCode(500, new { success = false, error = $"Error saving transaction: {ex.Message}" });
            }
        }
    }

    public class TransactionRequest
    {
        public int PersonnelId { get; set; }
        public decimal Total { get; set; }
        public List<TransactionItemRequest> Items { get; set; } = [];
    }

    public class TransactionItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}