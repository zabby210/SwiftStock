// // Controllers/InventoryController.cs
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using SwiftStock.Data;
// using SwiftStock.Models;

// namespace SwiftStock.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class InventoryController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly ILogger<InventoryController> _logger;

//         public InventoryController(ApplicationDbContext context, ILogger<InventoryController> logger)
//         {
//             _context = context;
//             _logger = logger;
//         }

//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventory()
//         {
//             try
//             {
//                 // Make sure we're using the lowercase 'inventory' table
//                 var items = await _context.inventory
//                     .OrderBy(i => i.Product_Name)
//                     .ToListAsync();

//                 _logger.LogInformation($"Retrieved {items.Count} inventory items");
//                 return Ok(items);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error retrieving inventory items");
//                 return StatusCode(500, "Error retrieving inventory items");
//             }
//         }
//     }
// }




/// CAUSES THE ADMIN INVETORY PAGE TO NOT LOAD. 
/// SO I COMMENTED IT OUT.
/// Microsoft.AspNetCore.Routing.Matching.AmbiguousMatchException
/// DUPLICATE NAME PLEASE USE A DIFFERENT
