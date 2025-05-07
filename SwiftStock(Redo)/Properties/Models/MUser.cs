using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftStock.Properties.Models
{
    public class MUser
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }
} 