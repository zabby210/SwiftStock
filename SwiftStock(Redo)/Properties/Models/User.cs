using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Properties.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; } = string.Empty;
        [Column(TypeName = "nvarchar(128)")]
        public string Role { get; set; }
    }
}