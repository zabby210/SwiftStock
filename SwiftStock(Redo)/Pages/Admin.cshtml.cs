using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Pages
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

                var transactions = _context.transaction.ToList();

                // Calculate total sales for today
                TotalSales = transactions
                    .Where(t => t.Transaction_Date.Date == today)
                    .Sum(t => t.Total);

                // Calculate total transactions for today
                TotalTransactions = transactions
                    .Count(t => t.Transaction_Date.Date == today);

                // Calculate average transaction value
                AverageTransaction = transactions.Any()
                    ? transactions.Average(t => t.Total)
                    : 0m;

                // Get top selling product
                var topProduct = transactions
                    .GroupBy(t => t.Products)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                TopSellingProduct = topProduct?.Key ?? "No sales yet";
                TopSellingQuantity = topProduct?.Count() ?? 0;

                // Get recent transactions
                RecentTransactions = transactions
                    .OrderByDescending(t => t.Transaction_Date)
                    .Take(10)
                    .ToList();



                // Prepare product sales data for pie chart
                var productSales = transactions
                    .SelectMany(t => t.Products.Split(", ")) // Split products into individual items
                    .GroupBy(product => product.Trim()) // Group by product name (trimmed to avoid whitespace issues)
                    .Select(g => new
                    {
                        Product = g.Key,
                        Count = g.Count() // Count how many times the product appears across all transactions
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                ProductSalesData = new ChartData
                {
                    Labels = productSales.Select(x => x.Product).ToList(),
                    Values = productSales.Select(x => (decimal)x.Count).ToList() // Cast Count to decimal
                };



                // Prepare revenue data for bar chart
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .Reverse()
                    .ToList();

                var dailyRevenue = transactions
                    .Where(t => t.Transaction_Date.Date >= last7Days.First())
                    .GroupBy(t => t.Transaction_Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Total));

                RevenueData = new ChartData
                {
                    Labels = last7Days.Select(d => d.ToString("ddd")).ToList(),
                    Values = last7Days.Select(d => dailyRevenue.ContainsKey(d) ? dailyRevenue[d] : 0m).ToList()
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
        public int TotalTransactions { get; set; }
        public decimal AverageTransaction { get; set; }
        public string TopSellingProduct { get; set; }
        public int TopSellingQuantity { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public ChartData ProductSalesData { get; set; }
        public ChartData RevenueData { get; set; }
    }

    public class ChartData
    {
        public List<string> Labels { get; set; }
        public List<decimal> Values { get; set; }
    }
}