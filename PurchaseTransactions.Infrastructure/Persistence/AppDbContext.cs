using Microsoft.EntityFrameworkCore;

namespace PurchaseTransactions.Infrastructure.Persistence
{
	public class AppDbContext : DbContext
    {
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Entities.PurchaseTransaction> PurchaseTransactions { get; set; } = default!;   
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Entities.PurchaseTransaction>(entity =>
            {
                entity.ToTable("PurchaseTransactions");
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.TransactionId).ValueGeneratedOnAdd();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionDate).IsRequired();
              
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.HasIndex(e => new { e.TransactionDate, e.Amount }).HasDatabaseName("IX_Index");
            });
        }
    }   
}
