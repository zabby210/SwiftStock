using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventory(string? search = null, string? sort = null)
        {
            try
            {
                var query = _context.inventory.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(i => i.Product_Name.Contains(search));
                }

                query = sort?.ToLower() switch
                {
                    "name" => query.OrderBy(i => i.Product_Name),
                    "stock" => query.OrderBy(i => i.Stock),
                    "price" => query.OrderBy(i => i.Price),
                    _ => query.OrderBy(i => i.Id)
                };

                var items = await query.ToListAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory items");
                return StatusCode(500, "Error retrieving inventory items");
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddItem([FromBody] InventoryItem item)
        {
            if (item == null)
                return BadRequest("Invalid item");

            try
            {
                _context.inventory.Add(item);
                await _context.SaveChangesAsync();
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product: {ProductName}", item.Product_Name);
                return StatusCode(500, "Error adding product to inventory");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProduct([FromBody] InventoryItem item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingItem = await _context.inventory.FindAsync(item.Id);
                if (existingItem == null)
                {
                    return NotFound($"Product with ID {item.Id} not found");
                }

                existingItem.Product_Name = item.Product_Name;
                existingItem.Price = item.Price;
                existingItem.Stock = item.Stock;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated product: {item.Product_Name}");
                return Ok(existingItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, "Error updating product");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var item = await _context.inventory.FindAsync(id);
                if (item == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                _context.inventory.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted product with ID: {id}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, "Error deleting product");
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                var items = await _context.inventory.OrderBy(i => i.Product_Name).ToListAsync();

                // Create CSV content
                var csv = "ID,Product Name,Price,Stock\n";
                foreach (var item in items)
                {
                    csv += $"{item.Id},{item.Product_Name},{item.Price},{item.Stock}\n";
                }

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"inventory_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting inventory");
                return StatusCode(500, "Error exporting inventory");
            }
        }
    }
}