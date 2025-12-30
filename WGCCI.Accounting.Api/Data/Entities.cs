using System.ComponentModel.DataAnnotations;

namespace WGCCI.Accounting.Api.Data;

public class Organization
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string BaseCurrency { get; set; } = "USD";
}

public class AppUser
{
    public int Id { get; set; }
    public int OrgId { get; set; }
    public Organization? Org { get; set; }

    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}

public class UserRole
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public AppUser? User { get; set; }

    public Role Role { get; set; }
}

public class Account
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    [Required]
    public string Code { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    public AccountType Type { get; set; }

    public string? Currency { get; set; }
}

public class JournalEntry
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public DateOnly Date { get; set; }

    public string? Memo { get; set; }

    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}

public class JournalLine
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public int EntryId { get; set; }
    public JournalEntry? Entry { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class Vendor
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    [Required]
    public string Name { get; set; } = "";
}

public class Customer
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    [Required]
    public string Name { get; set; } = "";
}

public class Budget
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public int AccountId { get; set; }

    [Required]
    public string Period { get; set; } = "";

    public decimal Amount { get; set; }
}

public class Forecast
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public int AccountId { get; set; }

    [Required]
    public string Period { get; set; } = "";

    public decimal Amount { get; set; }

    public string Method { get; set; } = "manual";
}

public class CurrencyRate
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public DateOnly Date { get; set; }

    [Required]
    public string FromCurrency { get; set; } = "USD";

    [Required]
    public string ToCurrency { get; set; } = "USD";

    public decimal Rate { get; set; }
}

public class RevaluationRun
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public DateOnly RunDate { get; set; }

    public string? Note { get; set; }
}

public class TaxCode
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    [Required]
    public string Name { get; set; } = "";

    public decimal Rate { get; set; }

    public TaxType Type { get; set; }
}

public class BankTransaction
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public DateOnly Date { get; set; }

    [Required]
    public string Description { get; set; } = "";

    public decimal Amount { get; set; }

    public int? MatchedEntryId { get; set; }
}

public class BankRule
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    [Required]
    public string Pattern { get; set; } = "";

    [Required]
    public string AccountCode { get; set; } = "";
}

public class AuditLog
{
    public int Id { get; set; }
    public int OrgId { get; set; }

    public int? UserId { get; set; }

    [Required]
    public string Action { get; set; } = "";

    [Required]
    public string Entity { get; set; } = "";

    public string? EntityId { get; set; }

    public string? Before { get; set; }
    public string? After { get; set; }

    public DateTime Ts { get; set; } = DateTime.UtcNow;
}
