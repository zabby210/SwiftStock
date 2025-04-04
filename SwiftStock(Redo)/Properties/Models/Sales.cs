namespace AlfaMart.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }

        public InventoryItem InventoryItem { get; set; }
    }
}