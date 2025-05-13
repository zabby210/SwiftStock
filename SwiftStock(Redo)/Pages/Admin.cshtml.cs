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

        public string SelectedPeriod { get; set; } = "daily"; // Add this property

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

                // Filter transactions based on selected period
                var filteredTransactions = SelectedPeriod switch
                {
                    "daily" => transactions.Where(t => t.Transaction_Date.Date == today),
                    "weekly" => transactions.Where(t => t.Transaction_Date >= today.AddDays(-7)),
                    "monthly" => transactions.Where(t => t.Transaction_Date >= today.AddMonths(-1)),
                    "annually" => transactions.Where(t => t.Transaction_Date >= today.AddYears(-1)),
                    _ => transactions.Where(t => t.Transaction_Date.Date == today)
                };

                // Calculate metrics based on filtered transactions
                TotalSales = filteredTransactions.Sum(t => t.Total);
                TotalTransactions = filteredTransactions.Count();
                AverageTransaction = filteredTransactions.Any() ? filteredTransactions.Average(t => t.Total) : 0m;

                // Get top selling products for the selected period
                var productSales = filteredTransactions
                    .SelectMany(t => t.TransactionItems)
                    .GroupBy(ti => ti.Product.Product_Name)
                    .Select(g => new
                    {
                        Product = g.Key,
                        TotalQuantity = g.Sum(ti => ti.Quantity),
                        TotalRevenue = g.Sum(ti => ti.Subtotal)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(5)
                    .ToList();

                // Update pie chart data
                ProductSalesData.Labels = productSales.Select(x => x.Product).ToList();
                ProductSalesData.Values = productSales.Select(x => x.TotalRevenue).ToList();

                // Set top selling product
                if (productSales.Any())
                {
                    var topProduct = productSales.First();
                    TopSellingProduct = topProduct.Product;
                    TopSellingQuantity = topProduct.TotalQuantity;
                }

                // Update revenue data for line chart
                var dateRange = SelectedPeriod switch
                {
                    "daily" => Enumerable.Range(0, 24).Select(h => today.AddHours(h)),
                    "weekly" => Enumerable.Range(0, 7).Select(d => today.AddDays(-d)).Reverse(),
                    "monthly" => Enumerable.Range(0, 30).Select(d => today.AddDays(-d)).Reverse(),
                    "annually" => Enumerable.Range(0, 12).Select(m => today.AddMonths(-m)).Reverse(),
                    _ => Enumerable.Range(0, 24).Select(h => today.AddHours(h))
                };

                var groupedRevenue = filteredTransactions
                    .GroupBy<Transaction, object>(t => SelectedPeriod switch
                    {
                        "daily" => t.Transaction_Date.Hour,
                        "weekly" => t.Transaction_Date.Date,
                        "monthly" => t.Transaction_Date.Date,
                        "annually" => new DateTime(t.Transaction_Date.Year, t.Transaction_Date.Month, 1),
                        _ => t.Transaction_Date.Hour
                    })
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Total));

                RevenueData.Labels = SelectedPeriod switch
                {
                    "daily" => dateRange.Select(d => d.ToString("HH:mm")).ToList(),
                    "weekly" => dateRange.Select(d => d.ToString("ddd")).ToList(),
                    "monthly" => dateRange.Select(d => d.ToString("MMM dd")).ToList(),
                    "annually" => dateRange.Select(d => d.ToString("MMM yyyy")).ToList(),
                    _ => dateRange.Select(d => d.ToString("HH:mm")).ToList()
                };

                RevenueData.Values = dateRange.Select(d => 
                    groupedRevenue.TryGetValue(SelectedPeriod switch
                    {
                        "daily" => d.Hour,
                        "weekly" => d.Date,
                        "monthly" => d.Date,
                        "annually" => new DateTime(d.Year, d.Month, 1),
                        _ => d.Hour
                    }, out var value) ? value : 0m).ToList();

                // Get recent transactions
                RecentTransactions = transactions
                    .OrderByDescending(t => t.Transaction_Date)
                    .Take(10)
                    .ToList();
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
