using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminModel(ApplicationDbContext context, ILogger<AdminModel> logger) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<AdminModel> _logger = logger;

        // Dashboard data properties
        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransaction { get; set; }
        public string TopSellingProduct { get; set; } = "No sales yet";
        public int TopSellingQuantity { get; set; }
        public List<Transaction> RecentTransactions { get; set; } = [];
        public ChartData ProductSalesData { get; set; } = new();
        public ChartData RevenueData { get; set; } = new();

        public IActionResult OnGet()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
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
                var today = DateTime.Today;

                // Add transaction items to the query
                var transactions = _context.transaction
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .ToList();

                // Get recent transactions with their items
                RecentTransactions = _context.transaction
                    .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Product)
                    .OrderByDescending(t => t.Transaction_Date)
                    .Take(10)
                    .ToList();

                // Calculate total sales for today
                TotalSales = transactions
                    .Where(t => t.Transaction_Date.Date == today)
                    .Sum(t => t.Total);

                // Calculate total transactions for today
                TotalTransactions = transactions
                    .Count(t => t.Transaction_Date.Date == today);

                // Calculate average transaction value
                AverageTransaction = transactions.Count > 0
                    ? transactions.Average(t => t.Total)
                    : 0m;

                // Get top selling product
                var topProduct = transactions
                    .SelectMany(t => t.TransactionItems)
                    .GroupBy(ti => ti.Product.Product_Name)
                    .Select(g => new { Name = g.Key, Quantity = g.Sum(ti => ti.Quantity) })
                    .OrderByDescending(x => x.Quantity)
                    .FirstOrDefault();

                if (topProduct != null)
                {
                    TopSellingProduct = topProduct.Name;
                    TopSellingQuantity = topProduct.Quantity;
                }

                // Get recent transactions
                RecentTransactions = transactions
                    .OrderByDescending(t => t.Transaction_Date)
                    .Take(10)
                    .ToList();

                // Prepare product sales data for pie chart
                // Update product sales calculation to use TransactionItems
                var productSales = transactions
                    .SelectMany(t => t.TransactionItems)
                    .GroupBy(ti => ti.Product.Product_Name)
                    .Select(g => new
                    {
                        Product = g.Key,
                        TotalQuantity = g.Sum(ti => ti.Quantity)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToList();

                ProductSalesData.Labels = productSales.Select(x => x.Product).ToList();
                ProductSalesData.Values = productSales.Select(x => (decimal)x.TotalQuantity).ToList();

                // Prepare revenue data for bar chart
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .Reverse()
                    .ToList();

                var dailyRevenue = transactions
                    .Where(t => t.Transaction_Date.Date >= last7Days.First())
                    .GroupBy(t => t.Transaction_Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Total));

                RevenueData.Labels = last7Days.Select(d => d.ToString("ddd")).ToList();
                RevenueData.Values = last7Days.Select(d =>
                    dailyRevenue.TryGetValue(d.Date, out var value) ? value : 0m).ToList();
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
    }

    public class ChartData
    {
        public List<string> Labels { get; set; } = [];
        public List<decimal> Values { get; set; } = [];
    }
}
