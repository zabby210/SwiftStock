using Microsoft.AspNetCore.Mvc.RazorPages;
using SwiftStock.Data;

namespace SwiftStock.Pages
{
    public class LogEntry
    {
        public string LogType { get; set; }
        public string action_type { get; set; }
        public int? product_id { get; set; }
        public int? user_id { get; set; }
        public DateTime log_time { get; set; }
    }

    public class A_HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<LogEntry> CombinedLogs { get; set; }

        public A_HistoryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            try
            {
                var inventoryLogs = _context.logs_inventory
                    .AsEnumerable()
                    .Select(l => new LogEntry
                    {
                        LogType = "Inventory",
                        action_type = l.action_type,
                        product_id = l.product_id,
                        log_time = l.log_time
                    });

                var userLogs = _context.logs_users
                    .AsEnumerable()
                    .Select(l => new LogEntry
                    {
                        LogType = "User",
                        action_type = l.action_type,
                        user_id = l.user_id,
                        log_time = l.log_time
                    });

                CombinedLogs = inventoryLogs.Union(userLogs)
                                           .OrderByDescending(l => l.log_time)
                                           .ToList();
            }
            catch (Exception ex)
            {
                // Initialize empty list if there's an error
                CombinedLogs = new List<LogEntry>();
                // You might want to log the error or handle it appropriately
            }
        }
    }
}
