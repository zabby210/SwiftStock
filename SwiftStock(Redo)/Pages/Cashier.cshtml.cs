using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    [Authorize(Roles = "Personnel")]
    public class CashierPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CashierPageModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public string CurrentUserName { get; private set; }

        public List<InventoryItem> InventoryItems { get; set; } = new();

        public List<CartItem> Cart { get; set; } = new();
        public decimal Subtotal { get; private set; }
        public decimal Tax { get; private set; }
        public decimal Total { get; private set; }
        public decimal Change { get; private set; }

        public async Task OnGetAsync()
        {
            InventoryItems = await _context.inventory
                .Where(item => item.Stock > 0)
                .OrderBy(item => item.Product_Name)
                .ToListAsync();

            // Debug: Log the fetched items
            Console.WriteLine($"Fetched {InventoryItems.Count} inventory items.");
            foreach (var item in InventoryItems)
            {
                Console.WriteLine($"Item: {item.Product_Name}, Price: {item.Price}, Stock: {item.Stock}");
            }
        }

        public void AddToCart(int productId, string productName, decimal price)
        {
            var existingProduct = Cart.FirstOrDefault(c => c.ProductId == productId);
            if (existingProduct != null)
            {
                existingProduct.Quantity++;
            }
            else
            {
                Cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = 1
                });
            }

            UpdateCartTotals();
        }

        public void RemoveFromCart(int productId)
        {
            var product = Cart.FirstOrDefault(c => c.ProductId == productId);
            if (product != null)
            {
                Cart.Remove(product);
            }

            UpdateCartTotals();
        }

        public void IncrementQuantity(int productId)
        {
            var product = Cart.FirstOrDefault(c => c.ProductId == productId);
            if (product != null)
            {
                product.Quantity++;
            }

            UpdateCartTotals();
        }

        public void DecrementQuantity(int productId)
        {
            var product = Cart.FirstOrDefault(c => c.ProductId == productId);
            if (product != null)
            {
                if (product.Quantity > 1)
                {
                    product.Quantity--;
                }
                else
                {
                    Cart.Remove(product);
                }
            }

            UpdateCartTotals();
        }

        public void ClearCart()
        {
            Cart.Clear();
            UpdateCartTotals();
        }

        public void ProceedPayment(decimal cashPayment)
        {
            if (cashPayment < Total)
            {
                throw new InvalidOperationException("Insufficient cash payment.");
            }

            Change = cashPayment - Total;

            // Save transaction logic can be added here
            SaveTransaction();

            ClearCart();
        }

        private void UpdateCartTotals()
        {
            Subtotal = Cart.Sum(c => c.Price * c.Quantity);
            Tax = Subtotal * 0.12m; // 12% tax
            Total = Subtotal + Tax;
        }

        private void SaveTransaction()
        {
        }


        public async Task SaveTransactionAsync(string userName, List<CartItem> cart, decimal total)
        {
            // Prepare product and quantity lists
            var productList = string.Join(", ", cart.Select(item => item.ProductName));
            var totalQuantity = cart.Sum(item => item.Quantity);

            // Create a new transaction
            var transaction = new Transaction
            {
                Name = userName,
                Products = productList,
                Quantity = totalQuantity,
                Total = total,
                Transaction_Date = DateTime.Now
            };

            // Save the transaction to the database
            _context.transaction.Add(transaction);

            // Update inventory stock
            foreach (var cartItem in cart)
            {
                var inventoryItem = await _context.inventory.FirstOrDefaultAsync(i => i.Id == cartItem.ProductId);
                if (inventoryItem != null)
                {
                    inventoryItem.Stock -= cartItem.Quantity;

                    // Ensure stock doesn't go below zero
                    if (inventoryItem.Stock < 0)
                    {
                        inventoryItem.Stock = 0;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }



        public List<InventoryItem> FilterProducts(string searchInput)
        {
            if (string.IsNullOrWhiteSpace(searchInput))
            {
                return new List<InventoryItem>();
            }

            return InventoryItems
                .Where(item => item.Product_Name.Contains(searchInput, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}


