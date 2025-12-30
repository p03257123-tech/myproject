using Microsoft.EntityFrameworkCore;
using WGCCI.Accounting.Api.Data;

namespace WGCCI.Accounting.Api.Services;

public class AccountingService
{
    private readonly AppDbContext _db;

    public AccountingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Account?> GetAccountByCodeAsync(int orgId, string code) =>
        await _db.Accounts.FirstOrDefaultAsync(a => a.OrgId == orgId && a.Code == code);

    public async Task<JournalEntry> PostEntryAsync(
        int orgId,
        DateOnly date,
        string? memo,
        IEnumerable<(string accountCode, decimal debit, decimal credit)> lines)
    {
        decimal d = 0;
        decimal c = 0;

        var lineEntities = new List<JournalLine>();

        foreach (var ln in lines)
        {
            var acct = await GetAccountByCodeAsync(orgId, ln.accountCode)
                       ?? throw new InvalidOperationException($"Account {ln.accountCode} not found");

            d += ln.debit;
            c += ln.credit;

            lineEntities.Add(new JournalLine
            {
                OrgId = orgId,
                AccountId = acct.Id,
                Debit = ln.debit,
                Credit = ln.credit
            });
        }

        if (Math.Round(d, 2) != Math.Round(c, 2))
        {
            throw new InvalidOperationException("Unbalanced journal entry");
        }

        var je = new JournalEntry
        {
            OrgId = orgId,
            Date = date,
            Memo = memo,
            Lines = lineEntities
        };

        _db.JournalEntries.Add(je);
        await _db.SaveChangesAsync();

        return je;
    }
}
