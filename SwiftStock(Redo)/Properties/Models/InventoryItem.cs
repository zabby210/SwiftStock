// File: Models/InventoryItem.cs
namespace AlfaMart.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Product_Name { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}
