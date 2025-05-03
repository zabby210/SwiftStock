namespace SwiftStock.Properties.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Name { get; set; } // Name of the logged-in user
        public string Products { get; set; } // List of products in the transaction
        public int Quantity { get; set; } // Total quantity of products
        public decimal Total { get; set; } // Total price of the transaction
        public DateTime Transaction_Date { get; set; } // Date and time of the transaction
    }
}
