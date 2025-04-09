using Microsoft.EntityFrameworkCore;
using SwiftStock.Models;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.ToTable("inventory");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Product_Name).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
            });


            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sales");
                entity.HasKey(e => e.Id);
                entity.HasOne(s => s.InventoryItem)
                      .WithMany()
                      .HasForeignKey(s => s.InventoryItemId);
            });
        }
    }
}
