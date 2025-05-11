using Microsoft.EntityFrameworkCore;
using SwiftStock.Properties.Models;

namespace SwiftStock.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<InventoryItem> inventory { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Sale> sales { get; set; }
        public DbSet<Transaction> transaction { get; set; }
        public DbSet<TransactionItem> transaction_items { get; set; }
        public DbSet<MUser> _musers { get; set; }

         
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                _logger.LogWarning("DbContext is not configured. Using default configuration.");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
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
                    entity.ToTable("transaction");
                    entity.HasKey(e => e.Id);
                    entity.HasMany(e => e.TransactionItems)
                          .WithOne(e => e.Transaction)
                          .HasForeignKey(e => e.Transaction_ID);
                });

                modelBuilder.Entity<TransactionItem>(entity =>
                {
                    entity.ToTable("transaction_items");
                    entity.HasKey(e => e.Id);
                    entity.HasOne(e => e.Product)
                          .WithMany()
                          .HasForeignKey(e => e.Product_ID);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while configuring database model.");
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred while saving changes to the database.");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                }
                throw;
            }
        }
    }
}
