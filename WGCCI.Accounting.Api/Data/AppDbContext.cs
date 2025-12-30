using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WGCCI.Accounting.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    /*
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<AppUser> Users { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<JournalLine> JournalLines { get; set; } = null!;
    public DbSet<Vendor> Vendors { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<Forecast> Forecasts { get; set; } = null!;
    public DbSet<CurrencyRate> CurrencyRates { get; set; } = null!;
    public DbSet<RevaluationRun> RevaluationRuns { get; set; } = null!;
    public DbSet<TaxCode> TaxCodes { get; set; } = null!;
    public DbSet<BankTransaction> BankTransactions { get; set; } = null!;
    public DbSet<BankRule> BankRules { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    */
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Forecast> Forecasts => Set<Forecast>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<RevaluationRun> RevaluationRuns => Set<RevaluationRun>();
    public DbSet<TaxCode> TaxCodes => Set<TaxCode>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<BankRule> BankRules => Set<BankRule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Account>()
            .HasIndex(a => new { a.OrgId, a.Code })
            .IsUnique();

        mb.Entity<JournalLine>()
            .HasOne(l => l.Entry)
            .WithMany(e => e.Lines)
            .HasForeignKey(l => l.EntryId);

        // ✅ Prevent SQL decimal truncation
        mb.Entity<BankTransaction>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        mb.Entity<Budget>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        mb.Entity<Forecast>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        mb.Entity<JournalLine>()
            .Property(x => x.Debit)
            .HasPrecision(18, 2);

        mb.Entity<JournalLine>()
            .Property(x => x.Credit)
            .HasPrecision(18, 2);

        mb.Entity<CurrencyRate>()
            .Property(x => x.Rate)
            .HasPrecision(18, 6);

        mb.Entity<TaxCode>()
            .Property(x => x.Rate)
            .HasPrecision(18, 6);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added
                     || e.State == EntityState.Modified
                     || e.State == EntityState.Deleted)
            .ToList();

        foreach (var e in entries)
        {
            // Avoid auditing the audit table itself
            if (e.Entity is AuditLog) continue;

            var name = e.Entity.GetType().Name;

            var before = e.State == EntityState.Added
                ? null
                : JsonSerializer.Serialize(e.OriginalValues.ToObject());

            var after = e.State == EntityState.Deleted
                ? null
                : JsonSerializer.Serialize(e.CurrentValues.ToObject());

            int orgId = 0;
            if (e.CurrentValues.Properties.Any(p => p.Name == "OrgId"))
            {
                var value = e.CurrentValues["OrgId"];
                if (value is int i) orgId = i;
                else if (value is int ni) orgId = ni;
            }

            string? entityId = null;
            if (e.CurrentValues.Properties.Any(p => p.Name == "Id"))
            {
                entityId = e.CurrentValues["Id"]?.ToString();
            }

            AuditLogs.Add(new AuditLog
            {
                OrgId = orgId,
                Action = e.State.ToString(),
                Entity = name,
                EntityId = entityId,
                Before = before,
                After = after
            });
        }

        return base.SaveChanges();
    }
}
