using System;

namespace SwiftStock.Properties.Models
{
    public class LogsInventory
    {
        public int id { get; set; }
        public int? product_id { get; set; }
        public string? action_type { get; set; }
        public DateTime log_time { get; set; }
    }
}
