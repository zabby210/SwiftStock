using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;
using System.Security.Claims;

namespace SwiftStock.Pages
{
    [Authorize(Roles = "Personnel")]
    public class CashierPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CashierPageModel(ApplicationDbContext context)
        {
            _context = context;
            InventoryItems = new List<InventoryItem>();
        }

        public List<InventoryItem> InventoryItems { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            InventoryItems = await _context.inventory.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostProcessTransactionAsync([FromBody] TransactionData data)
        {
            try
            {
                // First create the main transaction
                var transaction = new Transaction
                {
                    Personnel_ID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    Total = data.Total,
                    Transaction_Date = DateTime.Now
                };

                _context.transaction.Add(transaction);
                await _context.SaveChangesAsync(); // Save to get the transaction ID

                // Process each item in the cart
                foreach (var item in data.Cart)
                {
                    var inventoryItem = await _context.inventory.FindAsync(item.productId);
                    if (inventoryItem != null)
                    {
                        // Create transaction item
                        var transactionItem = new TransactionItem
                        {
                            Transaction_ID = transaction.Id,
                            Product_ID = item.productId,
                            Quantity = item.quantity,
                            Subtotal = item.price * item.quantity
                        };

                        _context.transaction_items.Add(transactionItem);

                        // Update inventory stock
                        inventoryItem.Stock -= item.quantity;
                        if (inventoryItem.Stock < 0)
                        {
                            inventoryItem.Stock = 0;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class TransactionData
        {
            public List<CartItem> Cart { get; set; } = new();
            public decimal Total { get; set; }
            public decimal CashPayment { get; set; }
        }

        public class CartItem
        {
            public int productId { get; set; }
            public int quantity { get; set; }
            public decimal price { get; set; }
        }
    }
}


