using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class Consumer
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Username { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(64)")] // Ensure enough space for SHA256 hash
        public string Password { get; set; } = string.Empty;
    }
}