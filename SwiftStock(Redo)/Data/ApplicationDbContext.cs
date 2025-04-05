using AlfaMart.Models; // Adjust the namespace as needed
using Microsoft.EntityFrameworkCore;

namespace AlfaMart.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Consumer> consumer { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<InventoryItem> inventory { get; set; }
        public DbSet<Sale> sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Consumer entity
            modelBuilder.Entity<Consumer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            });

            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            });

            // Configure the InventoryItem entity
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Product_Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Stock).IsRequired();
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
            });

            // Configure the Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.HasOne(e => e.InventoryItem)
                      .WithMany()
                      .HasForeignKey(e => e.InventoryItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
