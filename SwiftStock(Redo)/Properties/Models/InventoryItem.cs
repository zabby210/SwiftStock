namespace SwiftStock.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Product_Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
