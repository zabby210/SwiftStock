using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Properties.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Products { get; set; } = string.Empty;
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal Total { get; set; }
        
        [Required]
        public DateTime Transaction_Date { get; set; } = DateTime.Now;
    }
}
