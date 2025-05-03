namespace SwiftStock.Properties.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Name { get; set; } // Name of the personnel
        public string Products { get; set; } // List of products and quantities
        public decimal Total { get; set; } // Total amount
        public DateTime Transaction_Date { get; set; } // Transaction date
    }
}
