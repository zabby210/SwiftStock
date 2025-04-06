using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace AlfaMart.Controllers
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
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventory(string search = "", string sort = "name")
        {
            try
            {
                var query = _context.inventory.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(i => i.Product_Name.Contains(search));
                }

                // Apply sorting
                query = sort.ToLower() switch
                {
                    "stock" => query.OrderBy(i => i.Stock),
                    "price" => query.OrderBy(i => i.Price),
                    _ => query.OrderBy(i => i.Product_Name)
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
        public async Task<ActionResult<InventoryItem>> AddProduct(InventoryItem item)
        {
            try
            {
                _context.inventory.Add(item);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetInventory), new { id = item.Id }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory item");
                return StatusCode(500, "Error adding inventory item");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProduct(InventoryItem item)
        {
            try
            {
                _context.Entry(item).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory item");
                return StatusCode(500, "Error updating inventory item");
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
                    return NotFound();
                }

                _context.inventory.Remove(item);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item");
                return StatusCode(500, "Error deleting inventory item");
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