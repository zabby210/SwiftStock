using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(64)")] // Ensure enough space for SHA256 hash
        public string Role { get; set; }
    }
}