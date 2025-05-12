using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Properties.Models
{
    [Table("logs_inventory")]
    public class LogInventory
    {
        [Key]
        public int id { get; set; }
        public int? product_id { get; set; }
        public string action_type { get; set; }
        public DateTime log_time { get; set; }
    }
}