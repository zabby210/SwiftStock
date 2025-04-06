using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Models;

namespace AlfaMart.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminModel> _logger;

        public AdminModel(ApplicationDbContext context, ILogger<AdminModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return RedirectToPage("/Login", new { returnUrl = "/Admin" });
            }

            try
            {
                LoadDashboardData();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard data. Please try again later.";
                return Page();
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                // Calculate total sales amount
                TotalSales = _context.sales
                    .Include(s => s.InventoryItem)
                    .Sum(s => s.Quantity * s.InventoryItem.Price);

                TotalOrders = _context.sales.Count();
                ActiveCustomers = _context.consumer.Count();
                LowStockItems = _context.inventory.Count(i => i.Stock < 10);

                // Get recent orders with related data
                RecentOrders = _context.sales
                    .Include(s => s.InventoryItem)
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .ToList();

                // Get top products by stock level
                TopProducts = _context.inventory
                    .OrderByDescending(i => i.Stock)
                    .Take(5)
                    .ToList();

                // Prepare sales chart data
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Today.AddDays(-i))
                    .Reverse()
                    .ToList();

                SalesData = new
                {
                    labels = last7Days.Select(d => d.ToString("ddd")),
                    values = last7Days.Select(d =>
                        _context.sales
                            .Where(s => s.Date.Date == d)
                            .Include(s => s.InventoryItem)
                            .Sum(s => s.Quantity * s.InventoryItem.Price))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "Error loading dashboard data. Please try again later.";
            }
        }

        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Login");
        }

        // Dashboard data properties
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveCustomers { get; set; }
        public int LowStockItems { get; set; }
        public List<Sale> RecentOrders { get; set; }
        public List<InventoryItem> TopProducts { get; set; }
        public object SalesData { get; set; }
    }
}