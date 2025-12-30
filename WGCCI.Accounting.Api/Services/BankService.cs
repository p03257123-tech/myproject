using Microsoft.EntityFrameworkCore;
using WGCCI.Accounting.Api.Data;

namespace WGCCI.Accounting.Api.Services;

public class BankService
{
    private readonly AppDbContext _db;

    public BankService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> ImportAsync(
        int orgId,
        IEnumerable<(DateOnly date, string description, decimal amount)> rows)
    {
        foreach (var r in rows)
        {
            _db.BankTransactions.Add(new BankTransaction
            {
                OrgId = orgId,
                Date = r.date,
                Description = r.description,
                Amount = r.amount
            });
        }

        return await _db.SaveChangesAsync();
    }

    public async Task<BankRule> AddRuleAsync(
        int orgId,
        string pattern,
        string accountCode)
    {
        var rule = new BankRule
        {
            OrgId = orgId,
            Pattern = pattern,
            AccountCode = accountCode
        };

        _db.BankRules.Add(rule);
        await _db.SaveChangesAsync();

        return rule;
    }

    public async Task<IEnumerable<object>> SuggestionsAsync(int orgId)
    {
        var rules = await _db.BankRules
            .Where(r => r.OrgId == orgId)
            .ToListAsync();

        var txs = await _db.BankTransactions
            .Where(t => t.OrgId == orgId && t.MatchedEntryId == null)
            .ToListAsync();

        var list = new List<object>();

        foreach (var t in txs)
        {
            var rule = rules.FirstOrDefault(r =>
                t.Description.Contains(
                    r.Pattern,
                    StringComparison.OrdinalIgnoreCase));

            if (rule != null)
            {
                list.Add(new
                {
                    bankTxId = t.Id,
                    suggestedAccountCode = rule.AccountCode,
                    amount = t.Amount
                });
            }
        }

        return list;
    }
}
