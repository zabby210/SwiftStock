using AlfaMart.Models; // Ensure this matches your Models namespace
using Microsoft.EntityFrameworkCore;

namespace AlfaMart.Data // Ensure this matches your project namespace
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } // Ensure User model is defined
    }
}