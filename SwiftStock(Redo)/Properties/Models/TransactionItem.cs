using SwiftStock.Properties.Models;

namespace SwiftStock.Properties.Models
{
    public class TransactionItem
    {
        public int Id { get; set; }
        public int Transaction_ID { get; set; }
        public int Product_ID { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        
        public virtual Transaction Transaction { get; set; } = null!;
        public virtual InventoryItem Product { get; set; } = null!;
    }
}