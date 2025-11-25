using BenyFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BenyFinance.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CreditCard> CreditCards { get; set; }
    public DbSet<RecurringTemplate> RecurringTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Or Restrict, depending on requirements. Cascade is easier for now.

        modelBuilder.Entity<User>()
            .HasMany(u => u.Categories)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.CreditCards)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.RecurringTemplates)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Transactions)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete transactions if category is deleted? Or maybe SetNull? 
                                                // Usually we want to prevent deleting category if used, or move to 'Uncategorized'.
                                                // For simplicity, Restrict.

        modelBuilder.Entity<Category>()
            .HasMany(c => c.RecurringTemplates)
            .WithOne(r => r.Category)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // CreditCard
        modelBuilder.Entity<CreditCard>()
            .HasMany(c => c.Transactions)
            .WithOne(t => t.Card)
            .HasForeignKey(t => t.CardId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Transaction
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<CreditCard>()
            .Property(c => c.Limit)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<RecurringTemplate>()
            .Property(r => r.Amount)
            .HasColumnType("decimal(18,2)");
    }
}
