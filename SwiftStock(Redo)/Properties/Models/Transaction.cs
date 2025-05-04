namespace SwiftStock.Properties.Models
{
    public class Transaction
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; } // Name of the cashier
        public string Products { get; set; } // JSON or string representation of products
        public int Quantity { get; set; } // Total quantity of items
        public decimal Total { get; set; } // Total amount
        public DateTime Transaction_Date { get; set; } // Date of transaction
    }
}
