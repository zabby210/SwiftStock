using System;

namespace SwiftStock.Properties.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int Personnel_ID { get; set; }
        public decimal Total { get; set; }
        public DateTime Transaction_Date { get; set; }
        
        public virtual List<TransactionItem> TransactionItems { get; set; } = [];
    }
}

