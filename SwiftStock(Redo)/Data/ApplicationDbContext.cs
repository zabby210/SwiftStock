using Microsoft.EntityFrameworkCore;
using SwiftStock.Properties.Models;

namespace SwiftStock.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryItem> inventory { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Sale> sales { get; set; }
        public DbSet<Transaction> Transactions { get; set; } // Add this line

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure InventoryItem entity
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.ToTable("inventory");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Product_Name).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
            });

            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sales");
                entity.HasKey(e => e.Id);
                entity.HasOne(s => s.InventoryItem)
                      .WithMany()
                      .HasForeignKey(s => s.InventoryItemId);
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transaction"); // Map to "transactions" table
                entity.HasKey(e => e.Id); // Primary key
                entity.Property(e => e.Name).IsRequired(); // Name of the personnel
                entity.Property(e => e.Products).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();// List of products
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)"); // Total amount
                entity.Property(e => e.Transaction_Date).IsRequired(); // Transaction date
            });
        }
    }
}
