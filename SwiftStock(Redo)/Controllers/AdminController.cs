using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftStock.Data;
using SwiftStock.Properties.Models;

namespace SwiftStock.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("ProductSales")]
        public async Task<IActionResult> GetProductSales(string period)
        {
            try
            {
                var transactions = await _context.transaction.ToListAsync();
                var startDate = GetStartDate(period);

                var productSales = transactions
                    .Where(t => t.Transaction_Date >= startDate)
                    .GroupBy(t => t.Products)
                    .Select(g => new { Product = g.Key, Count = (decimal)g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(7)
                    .ToList();

                return Ok(new
                {
                    labels = productSales.Select(x => x.Product).ToList(),
                    values = productSales.Select(x => x.Count).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product sales data");
                return StatusCode(500, "Error getting product sales data");
            }
        }

        [HttpGet("Revenue")]
        public async Task<IActionResult> GetRevenue(string period)
        {
            try
            {
                var transactions = await _context.transaction.ToListAsync();
                var startDate = GetStartDate(period);
                var endDate = DateTime.Today;

                var dates = GetDateRange(startDate, endDate, period);
                var revenue = transactions
                    .Where(t => t.Transaction_Date >= startDate)
                    .GroupBy(t => GetDateKey(t.Transaction_Date, period))
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Total));

                return Ok(new
                {
                    labels = dates.Select(d => FormatDate(d, period)).ToList(),
                    values = dates.Select(d => revenue.ContainsKey(GetDateKey(d, period)) ? revenue[GetDateKey(d, period)] : 0m).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue data");
                return StatusCode(500, "Error getting revenue data");
            }
        }

        private DateTime GetStartDate(string period)
        {
            return period switch
            {
                "daily" => DateTime.Today,
                "weekly" => DateTime.Today.AddDays(-7),
                "monthly" => DateTime.Today.AddMonths(-1),
                "annually" => DateTime.Today.AddYears(-1),
                _ => DateTime.Today
            };
        }

        private List<DateTime> GetDateRange(DateTime startDate, DateTime endDate, string period)
        {
            var dates = new List<DateTime>();
            var current = startDate;

            while (current <= endDate)
            {
                dates.Add(current);
                current = period switch
                {
                    "daily" => current.AddDays(1),
                    "weekly" => current.AddDays(1),
                    "monthly" => current.AddDays(1),
                    "annually" => current.AddMonths(1),
                    _ => current.AddDays(1)
                };
            }

            return dates;
        }

        private string GetDateKey(DateTime date, string period)
        {
            return period switch
            {
                "daily" => date.Date.ToString("yyyy-MM-dd"),
                "weekly" => date.Date.ToString("yyyy-MM-dd"),
                "monthly" => date.Date.ToString("yyyy-MM-dd"),
                "annually" => date.ToString("yyyy-MM"),
                _ => date.Date.ToString("yyyy-MM-dd")
            };
        }

        private string FormatDate(DateTime date, string period)
        {
            return period switch
            {
                "daily" => date.ToString("HH:mm"),
                "weekly" => date.ToString("ddd"),
                "monthly" => date.ToString("dd"),
                "annually" => date.ToString("MMM"),
                _ => date.ToString("dd")
            };
        }
    }
} 