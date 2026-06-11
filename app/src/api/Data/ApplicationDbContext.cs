using Microsoft.EntityFrameworkCore;
using FinancialApp.API.Repositories;

namespace FinancialApp.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.AccountName).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.SourceAccountId).IsRequired();
            entity.Property(e => e.TargetAccountId).IsRequired();
            entity.Property(e => e.Amount).IsRequired().HasPrecision(19, 2);
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
